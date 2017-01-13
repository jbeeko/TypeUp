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
    static member IsArray (t : Type) = t.IsArray

type FSharpType with
    static member IsList (t : Type) = t.Name = "FSharpList`1"

type ParseState = {
    Line: int;
    Col: int;    
}

type Node = 
    | Primitive of Primitive 
    | FieldName of FieldName
    | Field of Field 
    | Record of Record
    | Union of Union
    | Element of Element
    | Collection of Collection

and Primitive = {
    Type : Type;
    Value: String;
    Position: Position
}
and FieldName = {
    Name : String;
    Position: Position
}
and Field = {
    Name : Node;
    Type: Type;
    IsOption: Boolean;
    Value: Node;
    Position: Position
}
and Record = {
    Type: Type;
    Fields: Node list;
    Position: Position
}
and CaseName ={
    Name: String;
    Position: Position
}
and Case = {
    Name: CaseName;
    Type: Type
    Value: Node;
    Position: Position
}
and Union = {
    Case: Case;
    Type: Type;
    Position: Position
}
and Element = {
    Type: Type;
    Value: Node;
    Postion: Position;
}
and Collection = {
    Items : Node list;
    Type : Type;
    Position: Position
}

let pprimitive (t: Type) =
    let trim (str : string) =
        str.Trim()
    let prim p s : Node =
        Primitive {Value = s; Type = t; Position = p}
    pipe2 getPosition ((restOfLine false)|>>trim) prim 

let rec pfieldName (f: Reflection.PropertyInfo) =
    let name p s : Node =
        FieldName {Name = s; Position = p}
    pipe2 getPosition (pstring (f.Name + ":")) name

and pfield (f: Reflection.PropertyInfo) =
    let field p n v : Node =
        Field {Name = n; Type = f.PropertyType; IsOption = false; Value = v; Position = p}
    pipe3 getPosition (pfieldName f) (ptype f.PropertyType) field

and precord t =
    let makeType p vals = 
       Record {Type = t; Fields = vals; Position = p}

    let p2 = 
        FSharpType.GetRecordFields (t)
        |> Array.toList
        |> List.map (fun f -> (pfield f.>>spaces) |>> List.singleton)
        |> List.reduce (fun p1 p2 -> pipe2 p1 p2 List.append)
    pipe2 getPosition p2 makeType

// and punioninfo  (t: Type) =
//     let parsers = 
//         FSharpType.GetUnionCases t 
//         |> Array.map (fun c -> spaces>>.pstring c.Name.>>spaces>>%c)
//     choiceL parsers (sprintf "Expecting a case of %s" t.Name)

// and punioncase  (cInfo: UnionCaseInfo) =
//     let makeType caseInfo args = 
//         FSharpValue.MakeUnion(caseInfo, args)
//     let initial : Parser<obj[], unit> = preturn [||]
//     let vals = cInfo.GetFields()
//             |> Array.map (fun f -> (ptype f.PropertyType.>>spaces) |>> Array.singleton)
//             |> Array.fold (fun p1 p2 -> pipe2 p1 p2 Array.append) initial
//     vals |>> makeType cInfo

// and punion (t : Type)  =
//     punioninfo t >>= punioncase 

and pelement (t : Type) =
    let elem p n =
        Element {Type = t; Value = n; Postion = p}
    spaces>>.pstring "-">>.(pipe2 getPosition (ptype t) elem)


// and parray (t : Type) =
//     let elementT = t.GetElementType()
//     let toArrayT (elements : obj list)  =
//         let arrayT = Array.CreateInstance(elementT, elements.Length)
//         for i = (elements.Length - 1) downto 0 do
//             arrayT.SetValue(elements.[i], i)
//         arrayT

//     many (pelement elementT)|>>toArrayT|>>box

and plist (t : Type) =
    let elementT  = t.GenericTypeArguments |> Seq.exactlyOne
    let toListT elements =
        let folder state head =
            head::state 
        elements |> List.fold folder []
    let coll p c = 
        Collection {Type = t; Items = c; Position = p}

    pipe2 getPosition (many (pelement elementT)|>>List.rev|>>toListT) coll

and ptype(t : Type)  =
    let (|Record|_|) t = if FSharpType.IsRecord(t) then Some(t)  else None
    let (|Union|_|) t = if FSharpType.IsUnion(t) then Some(t) else None
    let (|List|_|) t = if FSharpType.IsList(t) then Some(t) else None
    //let (|Array|_|) t = if FSharpType.IsArray(t) then Some(t) else None
    
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
    //| Array t -> parray t
    | Record t -> precord t
    //| Union t -> punion t
    | _ -> fail "Unsupported type"

let parseFSON t fson = 
    match run (ptype t) fson with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, _, _) -> failwith (sprintf "Failure: %s" errorMsg)

