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

let cons element list = 
    let ty = element.GetType()
    let uc = 
        Reflection.FSharpType.GetUnionCases(typedefof<_ list>.MakeGenericType [|ty|]) 
        |> Seq.filter (fun uc -> uc.Name = "Cons") 
        |> Seq.exactlyOne
    Reflection.FSharpValue.MakeUnion(uc, [|box element; box list|])

let castToSting (s : obj)  =
    // used for hacks where reflection is not understood
    s :?> String

let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
    fun stream ->
        printfn "%A: Entering %s" stream.Position label
        let reply = p stream
        printfn "%A: Leaving %s (%A)" stream.Position label reply.Status
        reply
        
let mayThrow (p : Parser<_,_>) : Parser<_,_> =
    fun stream ->
        let state = stream.State        
        try 
            p stream
        with e-> 
            stream.BacktrackTo(state)
            Reply(FatalError, messageError e.Message)

type FSharpType with
    static member IsOption (t : Type) = t.Name = "FSharpOption`1"

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
    |"System.UInt16" -> upcast UInt16.Parse(str)
    |"System.UInt32" -> upcast UInt32.Parse(str)
    |"System.UInt64" -> upcast UInt64.Parse(str)
    |"System.Single" -> upcast Single.Parse(str)
    |"System.Double" -> upcast Double.Parse(str)
    |"System.Decimal" -> upcast Decimal.Parse(str)
    |"System.Boolean" -> upcast Boolean.Parse(str)
    |"System.Byte" -> upcast Byte.Parse(str)
    |"System.SByte" -> upcast SByte.Parse(str)
    |"System.Char" -> upcast Char.Parse(str)
    |"System.String" -> upcast str
    |"System.DateTime" -> upcast DateTime.Parse str
    |"System.Guid" -> upcast Guid.Parse str
    |"System.Uri" -> upcast Uri.Parse str
    |"System.Net.IPAddress" -> upcast IPAddress.Parse str
    |"System.Net.Mail.MailAddress" -> upcast MailAddress.Parse str
    |_ -> failwith "Unsupported primative type"

let pprimative t =
    mayThrow(restOfLine false |>> (primFromString t))

let rec pfieldName (f: Reflection.PropertyInfo) =
    pstring (f.Name + ":")

and pfield (f: Reflection.PropertyInfo) =
    if FSharpType.IsOption f.PropertyType then
        //This is avery limited support for options, only string options are supported. 
        opt (pfieldName f>>.ptype (f.PropertyType.GenericTypeArguments |> Seq.exactlyOne)|>> castToSting)|>>box
    else 
        pfieldName f>>.ptype f.PropertyType

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

and punioncase  (cInfo: UnionCaseInfo) =
    let makeType caseInfo args = 
        FSharpValue.MakeUnion(caseInfo, args)
    let initial : Parser<obj[], unit> = preturn [||]
    let vals = cInfo.GetFields()
            |> Array.map (fun f -> (ptype f.PropertyType.>>spaces) |>> Array.singleton)
            |> Array.fold (fun p1 p2 -> pipe2 p1 p2 Array.append) initial
    vals |>> makeType cInfo

and punion (t : Type)  =
    punioninfo t >>= punioncase 

and plistelement (t : Type) =
    spaces>>.pstring "-">>.ptype t
    
and plist (t : Type) =
    let elementT  = t.GenericTypeArguments |> Seq.exactlyOne
    let emptyT = empty elementT

    let mockElement = FSharpValue.MakeUnion((FSharpType.GetUnionCases elementT).[1], [||]) 
    let mockList = cons mockElement  emptyT

    many (plistelement elementT)|>>box>>%mockList|>>box

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
    | Primative t -> pprimative t
    
    | List t -> plist t
    | Record t -> precord t
    | Union t -> punion t
    | _ -> fail "Unsupported type"

let parseFSON t fson = 
    match run (ptype t) fson with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, _, _) -> failwith (sprintf "Failure: %s" errorMsg)
