#r @"../build/FParsecCS.dll"
#r @"../build/FParsec.dll"
#load @"../src/FSONParser/FSONParser.fs"

open System
open System.Net
open System.Net.Mail
open FSONParser

type Phone =
    | Mobile of string
    | Office of string

type Address = {
    Street: string;
    City: string; Region: string; 
    Postal: string option;
    Country: string}

type Jurisdiction = 
    | BC | Alberta | Saskatchewan | Manitoba | Ontario | Quebec 

type Occupation = 
    | Programmer | Doctor | Pilot | Cook | Painter

type Person = {
    Name : string;
    DOB : DateTime;
    eMail : MailAddress;
    Phone : Phone;
    WebSite : Uri;
    IP : IPAddress;
    Occupations : Occupation array;
    Address : Address}

and Account ={
    Name : string;
    IBAN : string
}

and Company = {
        Name: string;
        WebSite: Uri;
        IncorporationLoc: Jurisdiction;
        BeneficialOwners: Person list}

and Contract = {
    Number : int64;
    ID : Guid;
    Start : DateTime;
    Jurisdiction : Jurisdiction option;
    Provider : Company;
    Holder : Company}

and LegalEntity = 
    | Person of Person
    | Company of Company


let data = "
Number: 34343
ID:  872ccb13-2e12-4eec-a2f5-ab64b3652b1c
Start: 2009-05-01
Jurisdiction: BC
Provider:
    Name: Acme Widgets
    WebSite: http://www.acme.com
    IncorporationLoc: BC
    BeneficialOwners:
        -   Name: Bill Smith
            DOB: 1988-01-20
            eMail: bill@co.com
            Phone: Mobile 604 666 7777
            WebSite: http://www.bill.com
            IP: 127.0.0.1
            Occupations: 
                - Doctor 
                - Pilot
            Address: 
                Street: 245 West Howe
                City: Vancouver
                Region: BC
                Postal: V6R-3L6
                Country: Canada
Holder: 
    Name: Acme Widgets
    WebSite: http://www.acme.com
    IncorporationLoc: BC
    BeneficialOwners:
        -   Name: Bill Smith
            DOB: 1988-01-20
            eMail: bill@co.com
            Phone: Mobile 604 666 7777
            WebSite: http://www.bill.com
            IP: 127.0.0.1
            Occupations: 
                - Doctor 
                - Pilot
            Address: 
                Street: 245 West Howe
                City: Vancouver
                Region: BC
                Postal: V6R-3L6
                Country: Canada"



let contract = (parseFSON typeof<Contract> data) :?> Contract

