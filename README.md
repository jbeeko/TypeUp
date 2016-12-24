# TypeUp
TypeUp consists of a FSharp object notation (FSON) and a matching FSONParser. TypeUp lets you represent a wide range of FSharp types in a simple text format and then parse them into matching FSharp types on demand. Effectivly FSharp provides the data definition language for a simple human readable object notation. 

TypeUp is useful where typed data or documents need to be specified in a text file. For example project configuration files, blog posts, contracts and, service definition documents. 

For the FSharp developer using TypeUp is very simple and direct. Most FSharp domain models will define a valid matching FSON dialect that can be used to specify data directly. There is no need to parse another representation such as JSON and then translate the parsed structure into the FSharp types. 

Here, `../../examples/small.fsx`,  is an small example (a larger one is [here](#larger-example)) of defining a type, creating some data and parsing it. 

```
#r @"..\build\FSONParser.dll"

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

let address = (parseFSON typeof<Address> data) :?> Address
```

Sending the above to FSharp Interactive results in:

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

Despite the size of the example the potential of FSharp types as a data definition language is apperent. For example a misnamed field results in
```
Error in Ln: 7 Col: 1
Cty: Vancouver
^
Expecting: 'City:'
```

and an out of range value results in:
```
Error in Ln: 3 Col: 9
Number: 3670555555555555555555555
        ^
Value was either too large or too small for an Int16.
```

Missing fields, extra fields, wrong types etc result in similar errors. 


## FSON Language

### Why Another Object Notation Language?

### Comparison to YAML

### Block Structure

### Records

### Union types

### Collections

### Primative Values
The following .Net types are implements as primatives. In each case a string represening all of the data in the field is passed to the native .Net parse method or constructor as is and either a value is created or the parser fails with the .Net error message.

**Signed and Unsigned Integers:** [Int16](https://msdn.microsoft.com/en-us/library/system.int16(v=vs.110).aspx), [Int32](https://msdn.microsoft.com/en-us/library/system.int32(v=vs.110).aspx), [Int64](https://msdn.microsoft.com/en-us/library/system.int64(v=vs.110).aspx), [UInt16](https://msdn.microsoft.com/en-us/library/system.uint16(v=vs.110).aspx), [UInt32](https://msdn.microsoft.com/en-us/library/system.uint32(v=vs.110).aspx), [UInt64](https://msdn.microsoft.com/en-us/library/system.uint64(v=vs.110).aspx)

**Floats and Decimals:** [Single](https://msdn.microsoft.com/en-us/library/system.single(v=vs.110).aspx), [Double](https://msdn.microsoft.com/en-us/library/system.double(v=vs.110).aspx), [Decimal](https://msdn.microsoft.com/en-us/library/system.decimal(v=vs.110).aspx)

**Bytes, Chars, Strings and Booleans** [Boolean](https://msdn.microsoft.com/en-us/library/system.boolean(v=vs.110).aspx), [Byte](https://msdn.microsoft.com/en-us/library/system.byte(v=vs.110).aspx), [SByte](https://msdn.microsoft.com/en-us/library/system.sbyte(v=vs.110).aspx), [Char](https://msdn.microsoft.com/en-us/library/system.char(v=vs.110).aspx), [String](https://msdn.microsoft.com/en-us/library/system.string(v=vs.110).aspx)

**Other primatives:** [DateTime](https://msdn.microsoft.com/en-us/library/system.datetime(v=vs.110).aspx), [Guid](https://msdn.microsoft.com/en-us/library/system.guid(v=vs.110).aspx), [IPAddress](https://msdn.microsoft.com/en-us/library/system.net.ipaddress(v=vs.110).aspx), [Uri](https://msdn.microsoft.com/en-us/library/system.uri(v=vs.110).aspx), [MailAddress](https://msdn.microsoft.com/en-us/library/system.net.mail.mailaddress(v=vs.110).aspx)


##FSON Limitations
A list of limitations of FSON. Where these are by design that is indicated. Others should perhaps be lifted.

### No Multi Field Union Types - Maybe
Union types with multiple fields are not supported. In principle it shoud be possible to support multiple fields. Those with labels would be supported like in the case of records. Unlabled fields would need to be designated using say a '-'.

### No Single Line Collections - Maybe
Collections must be written one element per line even when they could be un-ambiguously written several per line. For example given
```
type Occupation = 
    | Programmer | Doctor | Pilot | Cook | Painter

type Person =
    {Name : string;
    Occupations : Occupation list}
```
The FSON could be written as 
```
Name: Bill
Occupations: Cook Painter
```
It may be worth allowing this where possible, for example Union Types without data.  

### No Support for Validation - A Goal
By convention, the parser could invoke a standard `validate` function with the type constructed.

### No Support for Constained Strings and other Types - A Goal
FSharp does not support Dependant Types but several authors have outlined how the type and module system could be used to implement similar features, for example  [constrained types])http://fsharpforfunandprofit.com/posts/designing-with-types-non-strings/). Providing support for these would be useful when entering data. 

### No Environment to Support "let" Bindings - Maybe
Currenlty the FSONParser parses a single type passed to the parseFSON function and returns the matching instance. YAML contains the notion of references to avoid duplication when editing. A similar extension would be to parse a environment of typed binding of the form `<identifier>:<type><data>`. A special binding called `root` is the one to be returned. For example:
```
home: Address 
    Number: 3670
    Street: 245 West Howe
    City: Vancouver
    Region: BC
    Country: Canada
```
This binding could then be used when defining subsequent values. For example: 
```
root: Person 
    Name: Bill Smith
    DOB: 1988-01-20
    eMail: bill@co.com
    Phone: Mobile 604 666 7777
    WebSite: http://www.bill.com
    IP: 127.0.0.1
    Occupations:
    Address: <home>
```
The benefit of this complication is that it allows the reuse of specific values. The tricky bit is how to integrate this into round tripping i.e. how to go from `text -> type -> text2` and have `text = text2`?

One approach might to be have a way to denote a type as a *reference* type and by convention define those in the environment? 

This change will add a lot of complexity that may not be worth while. The more advanced YAML feature do not seem to get used much, possibly due to the complexity. But it could be that the better tooling support possible due to typing may make it simpler to use.

### No Support for Classes - Maybe?
The supported types are limited to the FSharp types. There is no support for classes and other OO types. This probably limits the appeal of TypeUp to c# developers. 

### Fixed Field Order - By Design
Fields in FSON must be provided in the order they are declared in the type begining parsed. This definitly simplifies the parser. But it also gives a consistent expectation when entering data. If Address is defined as
```
type Address = 
    {Street: string;
    City: string; Region: Jurisdiction; 
    Postal: string option;
    Country: string;}
```
Then the FSON will always be written as
```
Street: 3345 West 14th
City: Vancouver
Region: BC
Country: Canada
``` 

Where the postal code is optional. Enforcing the order is desirable. There is no purpose to letting users enter this in an arbitrary order. 

### No Ad-hoc Comments - By Design
Comments are widely used in configuration files. They are not supported by FSON for three reasons:

1. Adding support for them will probably require introducing either delimaters or escape characters. For example if `\\` was to designate line end comments then the field `WebSite: https:\\mysite.com` will not be parsed correctly. This will make the language harder to user for novices. 
2. Since ad-hoc comments a not part of the model they will be lost when recreating the text from from the parsed data. This limits their usefulness.
3. In many cases if something warrents a commont or note it should be in the data model. Either explicitly or there should be an optional `Notes:` field. 


## Roadmap

### FSON Parser
The current implementation is very simple. It walks the provided type from the top down in the order of declaration and uses `FSharpReflection` to build the instance. Parsing is very simple (not even really parsing) because the types to expect are know. For exmample when encountering a field defined to be `double` it is sufficient to take all the characters up to the next field and call `Double.Parse`. If it works great, if not an error can be show. 

Combinators from the library [FParsec](http://www.quanttec.com/fparsec/) are used to string the parsing together. 

### Fix Outstanding Issues

#### Better Error Messages
So far the error messages are the default provided by FParsec, they are already surprizingly good but could be improved in a few places. 

#### Indent Based Parsing
The current parser does not implement indent based parsing and hence can't parse data like multi-line strings. This is also needed to reliably support collections. 

#### Options
The parser currently only works for `string option`. This is due an issue constructing the correct type to pass to `FSharpValue.MakeRecord`. 

#### Collections
Collections are not working. There seem to be two issues:

1) knowing when to stop applying the parser parsing list elements `many` from `FParsec`. 
2) like in the case of options knowing how to return a correclty typed list to `FSharpValue.MakeRecord`. 


### Tooling Support

#### Compiler Services
To support editor tooling the FSONParser needst to be able to provide access to the verious syntactic elements like lists of Field names, union cases etc. 

#### Language Server for VSCode and other editors
A language server for VCCode should be able to provide the following:
* Reparsing after edits to provide feedback
* Suggestions for mistyped field names
* Suggestions for things like union cases
* Hints in the case of missing options
* Commands to add new elements to a list. For example if a LegalEntity has a list of owners the language server should provide a command to *Add Owner* This should drop in a new empty template to be completed.  


##Larger Example

Here `../../examples/larger.fsx` is a larger example demonstrating a wider range of primative values, nested records, union types and collections.

```
#r @"..\build\FSONParser.dll"

open System
open System.Net
open System.Net.Mail
open FSONParser

type Phone =
    | Mobile of String
    | Office of String

type Address = {
    Street: String;
    City: String; Region: String; 
    Postal: String option;
    Country: String}

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
    Occupations : Occupation list;
    Address : Address}

and Company = {
        Name: String;
        WebSite: Uri;
        IncorporationLoc: Jurisdiction;
        BeneficialOwner: LegalEntity}

and LegalEntity = 
    | Person of Person
    | Company of Company

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

let contract = (parseFSON typeof<Contract> data) :?> Contract

```

Sending the above to FSharp Interactive results in:

``` 
val it : Models.Contract =
  {Number = 34343L;
   ID = 872ccb13-2e12-4eec-a2f5-ab64b3652b1c;
   Start = 5/1/2009 12:00:00 AM;
   Jurisdiction = BC;
   Provider =
    Company {Name = "Acme Widgets";
             WebSite = http://www.acme.com/;
             IncorporationLoc = BC;
             BeneficialOwner = Person {Name = "Bill Smith";
                                       DOB = 1/20/1988 12:00:00 AM;
                                       eMail = bill@co.com;
                                       Phone = Mobile "604 666 7777";
                                       WebSite = http://www.bill.com/;
                                       IP = 127.0.0.1;
                                       Occupations = [];
                                       Address = {Street = "245 West Howe";
                                                  City = "Vancouver";
                                                  Region = "BC";
                                                  Postal = Some "V6R-3L6";
                                                  Country = "Canada";};};};
   Holder = Person {Name = "Anne Brown";
                    DOB = 10/25/1998 12:00:00 AM;
                    eMail = anne@co.com;
                    Phone = Office "604 666 8888";
                    WebSite = http://www.anne.com/;
                    IP = 2001:0:9d38:6abd:2c48:1e19:53ef:ee7e;
                    Occupations = [];
                    Address = {Street = "5553 West 12th Ave";
                               City = "Vancouver";
                               Region = "BC";
                               Postal = null;
                               Country = "Canada";};};}

>
```