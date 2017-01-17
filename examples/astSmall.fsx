#r @"../build/FParsecCS.dll"
#r @"../build/FParsec.dll"
#load @"../src/FSONParser/fsonAST.fs"

open FsonAST

type Jurisdiction = 
    | BC | Alberta | Canada

type Address = 
    {Number: int16;
    Street: string;
    City: string; 
    Region: Jurisdiction array; 
    Postal: string;
    Country: string;}

let data = "
Number: 3670
Street: 245 West Howe
City: Vancouver
Region: 
    - BC
Postal: V6R2W5
Country: Canada"

let address = parseFSON typeof<Address> data