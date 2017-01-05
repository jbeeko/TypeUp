#r @"../../build/FParsecCS.dll"
#r @"../../build/FParsec.dll"
#load @"./FSONParser.fs"
open FSONParser
open FParsec

// File for misc scratch pad stuff

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


type DU = 
    | One | Two | Three


 ((parseFSON typeof<DU list> "One Two") :?> DU list)


 charsTillString "bar" false 100>>.pstring "bar"


 let test p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg

test (charsTillString "bar" false 100 .>> pstring "bar") "foobar"