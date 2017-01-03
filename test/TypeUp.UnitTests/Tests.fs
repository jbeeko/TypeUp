module Tests

open Expecto
open FSONParser

type Region = 
    | BC | Alberta | Canada

type Address = 
    {Number: int16;
    Street: string;
    City: string; Region: Region; 
    Postal: string option;
    Country: string;}

let data = " 
Number: 3670 
Street: 245 West Howe 
City: Vancouver 
Region: BC 
Country: Canada 
 "

[<Tests>]
let tests =
  testList "All" [
    testList "Primatives" [
      testCase "string" <| fun _ -> Expect.equal ((parseFSON typeof<string> " foo ") :?> string)  "foo" ""
      testCase "string empty" <| fun _ -> Expect.equal ((parseFSON typeof<string> "") :?> string)  "" ""
      testCase "string only whitespace" <| fun _ -> Expect.equal ((parseFSON typeof<string> "  ") :?> string)  "" ""

      testCase "int32" <| fun _ -> Expect.equal ((parseFSON typeof<int32> " 12 ") :?> int32)  12 ""
    ]

    testList "Records" [
      testCase "parse address" <| fun _ ->
        let address = (parseFSON typeof<Address> data) :?> Address
        Expect.isTrue (address.City = "Vancouver") "Parse an address and check city."
    ]

    testList "Unions" [
      testCase "zero field DU" <| fun _ -> Expect.equal ((parseFSON typeof<Region> " BC ") :?> Region)  Region.BC ""
    ]
  ]


  
