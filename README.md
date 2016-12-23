# TypeUp

TypeUp consists of a FSharp Object Notation (FSON) language and a matching  FSONParser. TypeUp lets you represent a wide rage of FSharp types in a simple text format and then parse them into matching FSharp types on demand. FSON files can be used as typed configuration files or to maintain an archive of structured documents. 

Here is a simple example of defining a type, creating some data and parsing it. A more complex example is provided later.

```
open FSONParser

type Jurisdiction = 
    | BC | Alberta | Canada

type Address = 
    {Number: int16;
    Street: string;
    City: string; Region: Jurisdiction; 
    Postal: string option;
    Country: string;}

let data = "
Number: 3670
Street: 245 West Howe
City: Vancouver
Region: BC
Country: Canada"
```

Then sending `(parseFSON typeof<Address> data) :?> Address` to FSharp Interactive results in:

```
> (parseFSON typeof<Address> data) :?> Address;;
val it : Address = {Number = 3670s;
                    Street = "245 West Howe";
                    City = "Vancouver";
                    Region = BC;
                    Postal = null;
                    Country = "Canada";}
>
```

This is a very simple example but by making some errors we can already see the advantage to letting the FSharp type provide a data definition language. For example a misnamed field results in
```
System.Exception: Failure: Error in Ln: 7 Col: 5
Citys: Vancouver
```

and an out of range value results in:
```
Number: 36705555555555555
        ^
Value was either too large or too small for an Int16.
```

Other error condictions provide similar "compiler" errors. A larger example is [here](#a-larger-example).


## A Larger Example

Here is a larger example demonstrating:
* A wider range of primative values
* Nested records
* Union types
* Lists

```
open System
open System.Net
open System.Net.Mail
open FSONParser

type Address = {
    Street: String;
    City: String; Region: String; Postal: String option;
    Country: String}

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
    Occupations : Occupation list;
    Address : Address}

and Company = {
        Name: String;
        IncorporationLoc: Jurisdiction;
        BeneficialOwner: LegalEntity}

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
    Holder : LegalEntity}

let data = "
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
        Occupations:
        Address: 
            Street: 245 West Howe
            City: Vancouver
            Region: BC
            Postal: V6R-3L6
            Country: Canada
Holder: 
    Person Name: Anne Brown
    DOB: 1998-10-25
    eMail: anne@co.com
    Phone: Office 604 666 8888
    WebSite: http://www.anne.com
    IP: 2001:0:9d38:6abd:2c48:1e19:53ef:ee7e
    Occupations: 
    Address:
        Street: 5553 West 12th Ave
        City: Vancouver
        Region: BC
        Country: Canada"

(parseFSON typeof<Contract> data) :?> Contract
```

This will result in a Contract equal to the following: 

``` 
Contract = 
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
                Occupations = [];
                Address =
                        {Street = "245 West Howe";
                        City = "Vancouver";
                        Region = "BC";
                        Postal = Some("12345");
                        Country = "Canada" }}};
    Holder =
        Person {Name = "Anne Brown";
        DOB = DateTime.Parse "1998-10-25";
        eMail = MailAddress.Parse "anne@co.com";
        Phone = Office "604 666 8888";
        WebSite = Uri.Parse "http://www.anne.com";
        IP = IPAddress.Parse "2001:0:9d38:6abd:2c48:1e19:53ef:ee7e";
        Occupations = [];
        Address =
                {Street = "5553 West 12th Ave";
                City = "Vancouver";
                Region = "BC";
                Postal = None;
                Country = "Canada" }}}
```