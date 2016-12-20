#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
#load "FSONParser.fs"
#load "Models.fs"
#load "SampleModels.fs"

open System
open System.IO
open FParsec
open System.Net
open System.Net.Mail
open FSONParser
open Models

let test p str =
    match run p str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, _, _) -> failwith (sprintf "Failure: %s" errorMsg)
// let toType t anObj = anObj :?> 

// let pcontract : Parser<Contract,unit> = 
//     let toType (anObj : obj) : Contract =
//         anObj :?> Contract
//     ptype typeof<Contract> |>> toType

let contractData = File.ReadAllText("./src/FSONParser/SampleData.fson")

let parsed = test (ptype typeof<Contract> |>> (fun anObj -> anObj :?> Contract)) contractData
parsed = SampleModels.constructed
parsed.Jurisdiction = BC



