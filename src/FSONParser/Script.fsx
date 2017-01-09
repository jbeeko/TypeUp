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


