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

let mutable address =  
    {Street = "245 West Howe";
    City = "Vancouver";
    Region = "BC";
    Postal = Some("12345");
    Country = "Canada" }
let addressData1 = "        
Street: 245 West Howe
City: Vancouver
Region: BC
Country: Canada"
let addressData2 = "        
Street: 245 West Howe
City: Vancouver
Region: BC
Postal: V6R-2W5
Country: Canada"


address <- test (ptype typeof<Address> |>> (fun anObj -> anObj :?> Address)) addressData1
address <- test (ptype typeof<Address> |>> (fun anObj -> anObj :?> Address)) addressData2
address.Postal


let contractData = File.ReadAllText("./src/FSONParser/SampleData.fson")

let parsed = test (ptype typeof<Contract> |>> (fun anObj -> anObj :?> Contract)) contractData

parsed = SampleModels.constructed
parsed.Jurisdiction = BC
match parsed.Holder with
| Person p -> sprintf "Holder is %s" p.Name
| Company c -> sprintf "Holder company %s" c.Name
| Tag t -> sprintf "Holder is tag %s" t


