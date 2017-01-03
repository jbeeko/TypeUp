module Tests

open System
open System.Net
open System.Net.Mail

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

      testCase "IP Address" <| fun _ -> Expect.equal ((parseFSON typeof<IPAddress> " 127.0.0.1 ") :?> IPAddress)  (IPAddress.Parse "127.0.0.1")  ""
      testCase "Guid" <| fun _ -> Expect.equal ((parseFSON typeof<Guid> " 872ccb13-2e12-4eec-a2f5-ab64b3652b1c ") :?> Guid)  (Guid.Parse "872ccb13-2e12-4eec-a2f5-ab64b3652b1c") ""
      testCase "MailAddress" <| fun _ -> Expect.equal ((parseFSON typeof<Guid> " bob@aaa.com ") :?> MailAddress)  (MailAddress.Parse "bob@aaa.com") ""
      testCase "URL" <| fun _ -> Expect.equal ((parseFSON typeof<Uri> " http://something.com ") :?> Uri)  (Uri.Parse "http://something.com") ""
      testCase "DateTime" <| fun _ -> Expect.equal ((parseFSON typeof<DateTime> " feb 1, 2016 ") :?> DateTime)  (DateTime.Parse "Feb 1, 2016") ""
    ]

    testList "Records" [
      testCase "parse address" <| fun _ ->
        let address = (parseFSON typeof<Address> data) :?> Address
        Expect.isTrue (address.City = "Vancouver") "Parse an address and check city."
    ]

    testList "Unions" [
      testCase "zero field DU" <| fun _ -> Expect.equal ((parseFSON typeof<Region> " BC ") :?> Region)  Region.BC ""
    ]

    testList "Lists" [
      testCase "DU List" <| fun _ -> 
        let data = "
          - BC
          - Alberta   "
        Expect.equal ((parseFSON typeof<Region list> data) :?> Region list) [BC; Alberta] ""

      testCase "String List" <| fun _ -> 
        let data = "
          - BC
          - Alberta   "
        Expect.equal ((parseFSON typeof<string list> data) :?> string list) ["BC"; "Alberta"] ""
    ]

    testList "Other Collections]" [
      testCase "DU Array" <| fun _ -> 
        let data = "
          - BC
          - Alberta   "
        Expect.equal ((parseFSON typeof<Region array> data) :?> Region array) [|BC; Alberta|] ""
    ]

  ]


  
