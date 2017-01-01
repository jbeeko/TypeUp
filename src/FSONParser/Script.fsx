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

([|7|].GetType()).GetElementType()

([7].GetType()).IsArray

"7".GetType().IsArray

let ty = typeof<int>
ty.MakeArrayType()

.GetElementType()

open System
open System.Reflection
(Array.CreateInstance(typeof<int>, 1)).GetType() = [|0|].GetType()

[2; 3].[1]


let elem = ["foo"; "bar"]
let elementT = typeof<string>
let toArrayT (elements : obj list)  =
    let arrayT = Array.CreateInstance(elementT, elements.Length)
    for i = 1 to elements.Length do
        arrayT.[i] <- elements.[i]


let a = Array.CreateInstance(typeof<string>, 1)
a.[0] <- "bar"

a.GetType()

a.SetValue("bar", 0)
a.GetValue(0)

[|"foo"|].[0] <- "bar"

