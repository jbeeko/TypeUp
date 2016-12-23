#r @"..\..\build\FParsecCS.dll"
#r @"..\..\build\FParsec.dll"
#r @"..\..\build\FSONParser.dll"
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

(parseFSON typeof<Address> data) :?> Address
