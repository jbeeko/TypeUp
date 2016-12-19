#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
#load "FSONParser.fs"
open System
open FParsec
open FSharp.Reflection
open System.Net
open System.Net.Mail
open FSONParser



type Address = {
    Street: String;
    City: String; Region: String;
    Country: String;
    }
Type.GetTypeCode(typeof<Address>)

type Jurisdiction = 
    | BC
    | Alberta
    | Canada

type Person = {
    Name : string;
    DOB : DateTime;
    eMail : MailAddress;
    //Phone : Phone;
    Address : Address;
    //url : Uri option
    }

type Contract = {
    Number : Int64;
    ID : Guid;
    Start : DateTime;
    Jurisdiction : Jurisdiction;
    Provider : Person;
    Holder : Person;
    }

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
    Name: Bill Smith
    DOB: 1988-01-20
    eMail: bill@co.com
    Address: 
        Street: 245 West Howe
        City: Vancouver 
        Region: BC
        Country: Canada
Holder: 
    Name: Anne Brown
    DOB: 1998-10-25
    eMail: anne@co.com
    Address:
        Street: 5553 West 12th Ave
        City: Vancouver 
        Region: BC
        Country: Canada"

test pcontract contractData


