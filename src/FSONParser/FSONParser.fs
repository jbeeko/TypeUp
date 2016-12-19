module FSONParser

open System
open FParsec
open FSharp.Reflection
open System.Net
open System.Net.Mail

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

type MailAddress with
    static member Parse str = MailAddress(str)

type Uri with
    static member Parse str = Uri(str)

let primFromString (t:  Type) (str: String)  : obj =
    //this does not seem to work
    //t.GetMethod("Parse", [|typeof<string>|]).Invoke(t, [|str|]) |> box
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
    spaces>>.
    pstring field.Name >>.
    pchar ':'

and pfield (field: Reflection.PropertyInfo) : Parser<obj,unit> =
    pfieldTag field>>.
    ptype field.PropertyType

and precord (aType : Type) : Parser<obj,unit> =
    let makeType vals = 
        FSharpValue.MakeRecord(aType, List.toArray vals)

    FSharpType.GetRecordFields (aType)
        |> Array.map (fun f -> (pfield f) |>> List.singleton)
        |> Array.reduce (fun p1 p2 -> pipe2 p1 p2 List.append)
        |>> makeType

and punionname  (t: Type) : Parser<_,_> =
    let parsers = 
        FSharpType.GetUnionCases t 
        |> Array.map (fun c -> spaces>>.pstring c.Name.>>spaces)
    choiceL parsers (sprintf "Expecting a case of %s" t.Name)

and punion (t : Type) : Parser<obj,unit> =
    let makeType name = 
        let case = 
            FSharpType.GetUnionCases t
            |> Array.find (fun (c) -> c.Name = name)
        //case.
        FSharpValue.MakeUnion(case, [||])
    punionname t |>> makeType

and ptype(t : Type) : Parser<obj,unit> =
    let (|Record|_|) t = if FSharpType.IsRecord(t) then Some(t)  else None
    let (|Union|_|) t = if FSharpType.IsUnion(t) then Some(t) else None
    let (|EMail|_|) t = if t = typeof<MailAddress> then Some(t) else None        
    let (|URL|_|) t = if t = typeof<Uri> then Some(t) else None        
    let (|GUID|_|) t = if t = typeof<Guid> then Some(t) else None        
    let (|IP|_|) t = if t = typeof<IPAddress> then Some(t) else None        
    let (|Primative|_|) t = if Type.GetTypeCode(t) <> TypeCode.Object then Some(t) else None

    spaces >>.
    match t with
    | Record t -> precord t
    | Union t -> punion t
    | EMail t | GUID t | URL t | IP t
    | Primative t -> mayThrow(restOfLine false |>> (primFromString t))
    | _ -> fail "Unsupported type"
