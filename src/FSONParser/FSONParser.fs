module FSONParser

open System
open FParsec
open FSharp.Reflection
open System.Net
open System.Net.Mail

module List =
    let empty ty = 
        let uc = 
            Reflection.FSharpType.GetUnionCases(typedefof<_ list>.MakeGenericType [|ty|]) 
            |> Seq.filter (fun uc -> uc.Name = "Empty") 
            |> Seq.exactlyOne
        Reflection.FSharpValue.MakeUnion(uc, [||])

let mayThrow (p: Parser<'t,'u>) : Parser<'t,'u> =
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

let primFromString (t:  Type) (str: String)  : obj =
    match t.FullName with
    |"System.Int16" -> Int16.Parse(str) |> box
    |"System.Int32" -> Int32.Parse(str) |> box
    |"System.Int64" -> Int64.Parse(str) |> box
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

let rec pfieldTag (field: Reflection.PropertyInfo) : Parser<_,_> =
    pstring field.Name >>.
    pchar ':'

and pfield (field: Reflection.PropertyInfo) : Parser<obj,unit> =
    pfieldTag field>>.
    ptype field.PropertyType

and precord (aType : Type) : Parser<obj,unit> =
    let makeType vals = 
        FSharpValue.MakeRecord(aType,  vals)

    FSharpType.GetRecordFields (aType)
        |> Array.map (fun f -> (pfield f.>>spaces) |>> Array.singleton)
        |> Array.reduce (fun p1 p2 -> pipe2 p1 p2 Array.append)
        |>> makeType

and punioninfo  (t: Type) : Parser<_,_> =
    let parsers = 
        FSharpType.GetUnionCases t 
        |> Array.map (fun c -> spaces>>.pstring c.Name.>>spaces>>%c)
    choiceL parsers (sprintf "Expecting a case of %s" t.Name)

and punioncase  (c: UnionCaseInfo) : Parser<_,_> =
    let makeType case args = 
        FSharpValue.MakeUnion(case, args)
    let initial : Parser<obj[], unit> = preturn [||]
    let vals = c.GetFields()
            |> Array.map (fun f -> (ptype f.PropertyType.>>spaces) |>> Array.singleton)
            |> Array.fold (fun p1 p2 -> pipe2 p1 p2 Array.append) initial
    vals |>> makeType c

and punion (t : Type) : Parser<obj,unit> =
    punioninfo t >>= punioncase 

and plistelement (t : Type) : Parser<obj, unit> =
    spaces>>.pstring "-">>.ptype t

and plist (t : Type) : Parser<obj, unit> =
    let initial = List.empty t.GenericTypeArguments.[0]
    preturn (box initial)

and ptype(t : Type) : Parser<obj,unit> =
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
    