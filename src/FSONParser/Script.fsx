#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
#load "FSONParser.fs"
open FSONParser

type Address = 
    {Street: string;
    City: string; Region: string; 
    Postal: string option;
    Country: string}

let data = "
Street: 245 West Howe
City: Vancouver
Region: BC
Country: Canada"

(parseFSON typeof<Address> data) :?> Address
