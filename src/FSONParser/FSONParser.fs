module FSONParser

open System
open FParsec
open FSharp.Reflection
open System.Net
open System.Net.Mail

let empty ty = 
    let uc = 
        Reflection.FSharpType.GetUnionCases(typedefof<_ list>.MakeGenericType [|ty|]) 
        |> Seq.filter (fun uc -> uc.Name = "Empty") 
        |> Seq.exactlyOne
    Reflection.FSharpValue.MakeUnion(uc, [||])


let mayThrow (p : Parser<_,_>) : Parser<_,_> =
    fun stream ->
        let state = stream.State        
        try 
            p stream
        with :? FormatException as e-> 
            stream.BacktrackTo(state)
            Reply(FatalError, messageError e.Message)

type FSharpType with
    static member IsOption (t : Type) = t.FullName = "FSharpOption`1"

type FSharpType with
    static member IsList (t : Type) = t.Name = "FSharpList`1"

type MailAddress with
    static member Parse str = MailAddress(str)

type Uri with
    static member Parse str = Uri(str)

let primFromString (t:  Type) str : obj =
    match t.FullName with
    |"System.Int16" -> upcast Int16.Parse(str) 
    |"System.Int32" -> upcast Int32.Parse(str)
    |"System.Int64" -> upcast Int64.Parse(str) 
    |"System.UInt16" -> UInt16.Parse(str) |> box
    |"System.UInt32" -> UInt32.Parse(str) |> box
    |"System.UInt64" -> UInt64.Parse(str) |> box
    |"System.Single" -> Single.Parse(str) |> box
    |"System.Double" -> Double.Parse(str) |> box
    |"System.Decimal" -> Decimal.Parse(str) |> box
    |"System.Boolean" -> Boolean.Parse(str) |> box
    |"System.Byte" -> Byte.Parse(str) |> box
    |"System.SByte" -> Byte.Parse(str) |> box
    |"System.Char" -> Char.Parse(str) |> box
    |"System.String" -> str |> box
    |"System.DateTime" -> DateTime.Parse str |> box
    |"System.Guid" -> Guid.Parse str |> box
    |"System.Uri" -> Uri.Parse str |> box
    |"System.Net.IPAddress" -> IPAddress.Parse str |> box
    |"System.Net.Mail.MailAddress" -> MailAddress.Parse str |> box
    |_ -> failwith "Unsupported primative type"

let rec pfieldName (f: Reflection.PropertyInfo) =
    pstring f.Name >>.pchar ':'

and pfield f =
    pfieldName f>>.
    ptype f.PropertyType

and precord t =
    let makeType vals = 
        FSharpValue.MakeRecord(t,  vals)

    FSharpType.GetRecordFields (t)
        |> Array.map (fun f -> (pfield f.>>spaces) |>> Array.singleton)
        |> Array.reduce (fun p1 p2 -> pipe2 p1 p2 Array.append)
        |>> makeType

and punioninfo  (t: Type) =
    let parsers = 
        FSharpType.GetUnionCases t 
        |> Array.map (fun c -> spaces>>.pstring c.Name.>>spaces>>%c)
    choiceL parsers (sprintf "Expecting a case of %s" t.Name)

and punioncase  (c: UnionCaseInfo) =
    let makeType case args = 
        FSharpValue.MakeUnion(case, args)
    let initial : Parser<obj[], unit> = preturn [||]
    let vals = c.GetFields()
            |> Array.map (fun f -> (ptype f.PropertyType.>>spaces) |>> Array.singleton)
            |> Array.fold (fun p1 p2 -> pipe2 p1 p2 Array.append) initial
    vals |>> makeType c

and punion (t : Type)  =
    punioninfo t >>= punioncase 

and plistelement (t : Type) =
    spaces>>.pstring "-">>.ptype t
    
and plist (t : Type) =
    let elementT  = t.GenericTypeArguments |> Seq.exactlyOne
    let initial = empty elementT
    many (plistelement elementT)|>>List.singleton>>%initial|>>box

and ptype(t : Type)  =
    let (|Record|_|) t = if FSharpType.IsRecord(t) then Some(t)  else None
    let (|Union|_|) t = if FSharpType.IsUnion(t) then Some(t) else None
    let (|List|_|) t = if FSharpType.IsList(t) then Some(t) else None
    
    let (|EMail|_|) t = if t = typeof<MailAddress> then Some(t) else None        
    let (|URL|_|) t = if t = typeof<Uri> then Some(t) else None        
    let (|GUID|_|) t = if t = typeof<Guid> then Some(t) else None        
    let (|IP|_|) t = if t = typeof<IPAddress> then Some(t) else None        
    let (|Primative|_|) t = if Type.GetTypeCode(t) <> TypeCode.Object then Some(t) else None

    spaces >>.
    match t with
    | EMail t | GUID t | URL t | IP t
    | Primative t -> mayThrow(restOfLine false |>> (primFromString t))
    
    | List t -> plist t
    | Record t -> precord t
    | Union t -> punion t
    | _ -> fail "Unsupported type"
