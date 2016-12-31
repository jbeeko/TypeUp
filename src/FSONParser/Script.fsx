#r @"..\..\build\FParsecCS.dll"
#r @"..\..\build\FParsec.dll"
#load @".\FSONParser.fs"
open FSONParser

type Jurisdiction = 
    | BC | Alberta | Canada

type Address = 
    {Street: string;
    City: string; Region: Jurisdiction; 
    Postal: string option;
    Country: string}

let data = "
Street: 245 West Howe
City: Vancouver
Region: BC
Country: Canada"

let addr = (parseFSON typeof<Address> data) :?> Address

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

let emp = empty ("".GetType())

cons "bar" (cons "foo" emp)


let ty = typeof<string>
Reflection.FSharpType.GetUnionCases(typedefof<_ option>.MakeGenericType [|ty|]) 


let some element = 
    let ty = element.GetType()
    let uc = 
        Reflection.FSharpType.GetUnionCases(typedefof<_ option>.MakeGenericType [|ty|]) 
        |> Seq.filter (fun uc -> uc.Name = "Some") 
        |> Seq.exactlyOne
    Reflection.FSharpValue.MakeUnion(uc, [|box element|])

some "foo"
let init : int list = []
let folder state head =
    head :: state
[1; 2; 3] 
    |> List.fold  folder init
    |> List.rev

