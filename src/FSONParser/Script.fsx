#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
#load "FSONParser.fs"
#load "Models.fs"
#load "SampleModels.fs"

open System
open System.IO
open System.Reflection
open FParsec
open FSONParser
open Models
open FSharp.Reflection
open System.Net
open System.Net.Mail




type ListHelper =
  static member Empty<'T>() : list<'T> = []

let makeEmpty =
  let empty = typeof<ListHelper>.GetMethod("Empty")
  let emptyArr : obj[] = [| |]
  fun ty -> empty.MakeGenericMethod([| ty |]).Invoke(null, emptyArr)

let l1 = makeEmpty typeof<string>
"ads"::l1
List.append l1 ["foo"]

List.append (makeEmpty typeof<string>) ["foo"]

let empty ty = 
    let uc = 
        Reflection.FSharpType.GetUnionCases(typedefof<_ list>.MakeGenericType [|ty|]) 
        |> Seq.filter (fun uc -> uc.Name = "Empty") 
        |> Seq.exactlyOne
    Reflection.FSharpValue.MakeUnion(uc, [||])

let l2 = empty typeof<string>
"foo"::l2
List.append l2 "foo"

let l : string list = []
"foo"::l
List.append l ["foo"]