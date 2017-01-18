module FsonAST

open System
open FParsec
open FSharp.Reflection
open System.Net
open System.Net.Mail

let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
    fun stream ->
        printfn "%A: Entering %s" stream.Position label
        let reply = p stream
        printfn "%A: Leaving %s (%A)" stream.Position label reply.Status
        reply
        
type FSharpType with
    static member IsOption (t : Type) = t.Name = "FSharpOption`1"

type FSharpType with
    static member IsArray (t : Type) = t.IsArray

type FSharpType with
    static member IsList (t : Type) = t.Name = "FSharpList`1"

type Node = 
    | Primitive of Primitive 
    | Field of Field 
    | Record of Record
    | Union of Union
    | UnionCase of UnionCase
    | Element of Element
    | Collection of Collection

and Primitive = {
    Position: Position
    Type : Type;
    Value: String;
}
and Field = {
    Position: Position
    Type: Type;
    Name : string;
    IsOption: Boolean;
    Value: Node;
}
and Record = {
    Position: Position
    Type: Type;
    Fields: Node list;
}
and Union = {
    Position: Position;
    Type: Type;
    Case: UnionCase;
}
and UnionCase = {
    Position: Position
    CaseInfo: UnionCaseInfo;
    Values: Node list;
}
and Element = {
    Postion: Position;
    Type: Type;
    Value: Node;
}
and Collection = {
    Position: Position
    Type : Type;
    Items : Node list;
}

let pprimitive (t: Type) =
    let trim (str : string) =
        str.Trim()
    let prim p s : Node =
        Primitive {Value = s; Type = t; Position = p}
    pipe2 (getPosition>>.getPosition) ((restOfLine false)|>>trim) prim 

let rec pfield (f: Reflection.PropertyInfo) =
    let field p n v : Node =
        Field {Name = n; Type = f.PropertyType; IsOption = false; Value = v; Position = p}
    pipe3 getPosition (pstring (f.Name + ":" )) (ptype f.PropertyType) field

and precord t =
    let record p v = 
       Record {Type = t; Fields = v; Position = p}

    let p2 = 
        FSharpType.GetRecordFields (t)
        |> Array.toList
        |> List.map (fun f -> (pfield f.>>spaces) |>> List.singleton)
        |> List.reduce (fun p1 p2 -> pipe2 p1 p2 List.append)
    pipe2 getPosition p2 record

and pcaseInfo  (t: Type) =
    let parsers = 
        FSharpType.GetUnionCases t 
        |> Array.map (fun c -> spaces>>.pstring c.Name.>>spaces>>%c)
    choiceL parsers (sprintf "Expecting a case of %s" t.Name)

and punioncase(p, ci) =
    let makeType caseInfo args = 
        FSharpValue.MakeUnion(caseInfo, args)
    let initial : Parser<Node[], unit> = preturn [||]
    let case v =
        UnionCase {Position = p; CaseInfo = ci; Values = Array.toList v}
    let vals = ci.GetFields()
            |> Array.map (fun f -> (ptype f.PropertyType.>>spaces) |>> Array.singleton)
            |> Array.fold (fun p1 p2 -> pipe2 p1 p2 Array.append) initial
    vals |>> case

and punion (t : Type)  =
    let union p c v  =
        Union {Position = p; Type = t; Case = c;}
    (getPosition .>>. pcaseInfo t) >>= punioncase

and pelement (t : Type) =
    let elem p n =
        Element {Type = t; Value = n; Postion = p}
    spaces>>.pstring "-">>.(pipe2 getPosition (ptype t) elem)

and parray (t : Type) =
    let elementT = t.GetElementType()
    let coll p c = 
        Collection {Type = t; Items = c; Position = p}
    pipe2 getPosition (many(pelement elementT)) coll

and plist (t : Type) =
    let elementT  = t.GenericTypeArguments |> Seq.exactlyOne
    let coll p c = 
        Collection {Type = t; Items = c; Position = p}
    pipe2 getPosition (many(pelement elementT)) coll

and ptype(t : Type)  =
    let (|Record|_|) t = if FSharpType.IsRecord(t) then Some(t)  else None
    let (|Union|_|) t = if FSharpType.IsUnion(t) then Some(t) else None
    let (|List|_|) t = if FSharpType.IsList(t) then Some(t) else None
    let (|Array|_|) t = if FSharpType.IsArray(t) then Some(t) else None

    let (|EMail|_|) t = if t = typeof<MailAddress> then Some(t) else None        
    let (|URL|_|) t = if t = typeof<Uri> then Some(t) else None        
    let (|GUID|_|) t = if t = typeof<Guid> then Some(t) else None        
    let (|IP|_|) t = if t = typeof<IPAddress> then Some(t) else None        
    let (|Primitive|_|) t = if Type.GetTypeCode(t) <> TypeCode.Object then Some(t) else None

    spaces >>.
    match t with
    | EMail t | GUID t | URL t | IP t
    | Primitive t -> pprimitive t
    
    | List t -> plist t
    | Array t -> parray t
    | Record t -> precord t
    | Union t -> punion t
    | _ -> fail "Unsupported type"

let parseFSON t fson = 
    match run (ptype t) fson with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, _, _) -> failwith (sprintf "Failure: %s" errorMsg)

