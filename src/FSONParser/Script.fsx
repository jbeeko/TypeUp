#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
#load "FSONParser.fs"
#load "Models.fs"
#load "SampleModels.fs"

open System
open System.IO
open FParsec
open FSONParser
open Models

let test p str =
    match run p str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, _, _) -> failwith (sprintf "Failure: %s" errorMsg)

let contractData = File.ReadAllText("./src/FSONParser/SampleData.fson")

let parsed = test (ptype typeof<Contract> |>> (fun anObj -> anObj :?> Contract)) contractData

parsed = SampleModels.constructed
parsed.Jurisdiction = BC
match parsed.Holder with
| Person p -> sprintf "Holder is %s" p.Name
| Company c -> sprintf "Holder company %s" c.Name
| Tag t -> sprintf "Holder is tag %s" t
