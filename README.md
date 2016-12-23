# TypeUp

TypeUp consists of a FSharp Object Notation (FSON) language and a matching  FSONParser. TypeUp lets you represent a wide rage of FSharp types in a simple text format and then parse them into matching FSharp types on demand. Effectivly FSharp provides the data definition language for a simple human readable object notation. TypeUp is useful where strickly typed data needs to be specified in a text file. For example configuration files or structured documents such as contracts or service definition documents. 

Here is an example of defining a type, creating some data and parsing it.

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

Sending the above to FSharp Interactive followed by `(parseFSON typeof<Address> data) :?> Address` results in:

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

This is a very simple example (a larger example is [here](#a-larger-example)), and tooling support is still rudimentatary but already the advantage using FSharp as a data definition language can be seen. For example a misnamed field results in
```
Error in Ln: 7 Col: 5
Citys: Vancouver
```

and an out of range value results in:
```
Number: 36705555555555555
        ^
Value was either too large or too small for an Int16.
```

Even better messages with better tooling support such as intellisense and suggestions would allow expert domain users to enter structured data without the need for domain specific UIs. 

## FSON Language

### Block Structure

### Records

#### Fields

### Union types

### Collections

### Primative Values

The following .Net types are implements as primatives. In each case a string represening all of the data in the field is passed to the native .Net parse method or constructor as is and either a value is created or the parser fails with the .Net error message.

**Signed and Unsigned Integers:** [Int16](https://msdn.microsoft.com/en-us/library/system.int16(v=vs.110).aspx), [Int32](https://msdn.microsoft.com/en-us/library/system.int32(v=vs.110).aspx), [Int64](https://msdn.microsoft.com/en-us/library/system.int64(v=vs.110).aspx), [UInt16](https://msdn.microsoft.com/en-us/library/system.uint16(v=vs.110).aspx), [UInt32](https://msdn.microsoft.com/en-us/library/system.uint32(v=vs.110).aspx), [UInt64](https://msdn.microsoft.com/en-us/library/system.uint64(v=vs.110).aspx)

**Floats and Decimals:** [Single](https://msdn.microsoft.com/en-us/library/system.single(v=vs.110).aspx), [Double](https://msdn.microsoft.com/en-us/library/system.double(v=vs.110).aspx), [Decimal](https://msdn.microsoft.com/en-us/library/system.decimal(v=vs.110).aspx)

**Bytes, Chars, Strings and Booleans**, [Boolean](https://msdn.microsoft.com/en-us/library/system.boolean(v=vs.110).aspx), [Byte](https://msdn.microsoft.com/en-us/library/system.byte(v=vs.110).aspx), [SByte](https://msdn.microsoft.com/en-us/library/system.sbyte(v=vs.110).aspx), [Char](https://msdn.microsoft.com/en-us/library/system.char(v=vs.110).aspx), [String](https://msdn.microsoft.com/en-us/library/system.string(v=vs.110).aspx)

**Other primatives:** [DateTime](https://msdn.microsoft.com/en-us/library/system.datetime(v=vs.110).aspx), [Guid](https://msdn.microsoft.com/en-us/library/system.guid(v=vs.110).aspx), [IPAddress](https://msdn.microsoft.com/en-us/library/system.net.ipaddress(v=vs.110).aspx), [Uri](https://msdn.microsoft.com/en-us/library/system.uri(v=vs.110).aspx), [MailAddress](https://msdn.microsoft.com/en-us/library/system.net.mail.mailaddress(v=vs.110).aspx)

### Limitations

#### Multi field Union types
Union types with multiple fields are not supported. In principle it shoud be possible to support multiple fields. Those with labels would be supported like in the case of records. Unlabled fields would need to be designated using say a '-'.


## FSON Parser

### Validation Approach

### Adding Primatives

## Roadmap

### Outstanding Issues

#### Indent Based Parsing
The current parser does not implement indent based parsing and hence can't parse data like multi-line strings. 

#### Options
The parser currently only works for `string option`. This is due an issue constructing the correct type to pass to `FSharpValue.MakeRecord`. 

#### Collections

Collections are not working. There seem to be two issues:
1) knowing when to stop applying the parser parsing list elements `many` from `FParsec`. 
2) like in the case of options knowing how to return a correclty typed list to `FSharpValue.MakeRecord`. 


### Tooling Support

### FSON Language Extensions

#### "let" Bindings

#### Validation Approach

#### Support for Constained Strings and Types

## Appendix I Larger Example

Here is a larger example demonstrating a wider range of primative values, nested records, union types and collections.


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