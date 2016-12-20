#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
#load "FSONParser.fs"

open System
open FParsec
open FSharp.Reflection
open System.Net
open System.Net.Mail
open FSONParser
open Microsoft.FSharp.Collections

type Phone =
    | Mobile of String
    | Office of String
    | Home of String
    | AfterHours of String 
    | Other of String

type Address = {
    Street: String;
    City: String; Region: String;
    Country: String;
    }

type Jurisdiction = 
    | BC
    | Alberta
    | Canada

type Occupation = 
    | Programmer
    | Doctor
    | Pilot
    | Cook
    | Painter

type Person = {
    Name : string;
    DOB : DateTime;
    eMail : MailAddress;
    Phone : Phone;
    WebSite : Uri;
    IP : IPAddress;
//    Occupations : Occupation list;
    Address : Address;
    }

and Company = {
        Name: String;
//        phones: Phone list;
        WebSite: Uri;
        IncorporationLoc: Jurisdiction;
        BeneficialOwner: LegalEntity;
    }

and LegalEntity = 
    | Person of Person
    | Company of Company
    | Tag of String

and Contract = {
    Number : Int64;
    ID : Guid;
    Start : DateTime;
    Jurisdiction : Jurisdiction;
    Provider : LegalEntity;
    Holder : LegalEntity;
    }

let constructed : Contract = 
    {Number = 34343L;
    ID = Guid.Parse  "872ccb13-2e12-4eec-a2f5-ab64b3652b1c";
    Start = DateTime.Parse "2009-05-01";
    Jurisdiction = BC;
    Provider = 
        Company {Name = "Acme Widgets";
            WebSite = Uri.Parse "http://www.acme.com";
            IncorporationLoc = BC;
            BeneficialOwner =
                Person {Name = "Bill Smith";
                DOB = DateTime.Parse "1988-01-20";
                eMail = MailAddress.Parse "bill@co.com";
                Phone = Mobile "604 666 7777";
                WebSite = Uri.Parse "http://www.bill.com";
                IP = IPAddress.Parse "127.0.0.1";
                //Occupations = [];
                Address =
                    {Street = "245 West Howe";
                    City = "Vancouver";
                    Region = "BC";
                    Country = "Canada" }}};
    Holder =
        Person {Name = "Anne Brown";
        DOB = DateTime.Parse "1998-10-25";
        eMail = MailAddress.Parse "anne@co.com";
        Phone = Office "604 666 8888";
        WebSite = Uri.Parse "http://www.anne.com";
        IP = IPAddress.Parse "2001:0:9d38:6abd:2c48:1e19:53ef:ee7e";
        //Occupations = [];
        Address =
            {Street = "5553 West 12th Ave";
            City = "Vancouver";
            Region = "BC";
            Country = "Canada" }}}

let test p str =
    match run p str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, _, _) -> failwith (sprintf "Failure: %s" errorMsg)

let pcontract : Parser<Contract,unit> = 
    let toType (anObj : obj) : Contract =
        anObj :?> Contract
    ptype typeof<Contract> |>> toType

let contractData = "
Number: 34343
ID:  872ccb13-2e12-4eec-a2f5-ab64b3652b1c
Start: 2009-05-01
Jurisdiction: BC
Provider:
    Company Name: Acme Widgets
    WebSite: http://www.acme.com
    IncorporationLoc: BC
    BeneficialOwner:
        Person Name: Bill Smith
        DOB: 1988-01-20
        eMail: bill@co.com
        Phone: Mobile 604 666 7777
        WebSite: http://www.bill.com
        IP: 127.0.0.1
        Address: 
            Street: 245 West Howe
            City: Vancouver
            Region: BC
            Country: Canada
Holder: 
    Person Name: Anne Brown
    DOB: 1998-10-25
    eMail: anne@co.com
    Phone: Office 604 666 8888
    WebSite: http://www.anne.com
    IP: 2001:0:9d38:6abd:2c48:1e19:53ef:ee7e
    Address:
        Street: 5553 West 12th Ave
        City: Vancouver
        Region: BC
        Country: Canada"

let parsed = test pcontract contractData
parsed = constructed


// let foo = List.singleton "foo"
// let six = List.singleton 6

// foo.GetType()
// let sixtype = (six.GetType())
// sixtype.GenericTypeArguments

