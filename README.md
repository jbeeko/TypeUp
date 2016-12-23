# TypeUp

TypeUp consists of a FSharp Object Notation (FSON) language and a matching  FSONParser. TypeUp lets you represent a wide rage of FSharp types in a simple text format and then parse them into matching FSharp types on demand. FSON files can be used as typed configuration files or to maintain an archive of structured documents. 

A sample use is the following. A more complex example is provided later.

```
open FSONParser

type Address = 
    {Street: String;
    City: String; Region: String; 
    Postal: String option;
    Country: String}

let data = "
Street: 245 West Howe
City: Vancouver
Region: BC
Country: Canada"
```

Then sending `(parseFSON typeof<Address> data) :?> Address` to FSharp Interactive results in:

```
> (parseFSON typeof<Address> data) :?> Address;;
val it : Address = {Street = "245 West Howe";
                    City = "Vancouver";
                    Region = "BC";
                    Postal = null;
                    Country = "Canada";}
>
```


## A Larger Example

```
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

Will result in a Contract identical to the following: 

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




![Sample output](docs/sample-output-2.png)

  * [expecto](#expecto)
    * [Installing](#installing)
    * [Testing "Hello world"](#testing-hello-world)
    * [Running tests](#running-tests)
      * [runTests](#runtests)
      * [runTestsInAssembly](#runtestsinassembly)
      * [testList for grouping](#testlist-for-grouping)
      * [Filtering with filter](#filtering-with-filter)
      * [Focusing tests](#focusing-tests)
      * [Pending tests](#pending-tests)
    * [Expectations](#expectations)
      * [Expect module](#expect-module)
    * [main argv – how to run console apps](#main-argv--how-to-run-console-apps)
      * [The config](#the-config)
    * [FsCheck usage](#fscheck-usage)
    * [BenchmarkDotNet usage](#benchmarkdotnet-usage)
    * [You're not alone\!](#youre-not-alone)
    * [Sending e\-mail on failure – custom printers](#sending-e-mail-on-failure--custom-printers)
    * [About test parallelism](#about-test-parallelism)
    * [About upgrading from Fuchu](#about-upgrading-from-fuchu)

## Installing

In your paket.dependencies:

```
nuget Expecto
nuget Expecto.PerfUtil
nuget Expecto.FsCheck
```

Tests should be first-class values so that you can move them around and execute
them in any context that you want.

Let's have look at what an extensive unit test suite looks like when running
with Expecto:

![Sample output from Logary](docs/sample-output-logary.png)

## Testing "Hello world"

The test runner is the test assembly itself. It's recommended to compile your
test assembly as a console application. You can run a test directly like this:

```fsharp
open Expecto

[<Tests>]
let tests =
  testCase "yes" <| fun () ->
    let subject = "Hello world"
    Expect.equal subject "Hello World"
                 "The strings should equal"

[<EntryPoint>]
let main args =
  runTestsInAssembly defaultConfig args
```

The base class is called `Expect`, containing functions you can use to assert
with. A testing library without a good assertion library is like love without
kisses.

Now compile and run! `xbuild Sample.fsproj && mono --debug bin/Debug/Sample.exe`


## Running tests

Here's the simplest test possible:

```fsharp
open Expecto

let simpleTest =
  testCase "A simple test" <| fun _ ->
    let expected = 4
    Expect.equal expected (2+2) "2+2 = 4"
```

Then run it like this, e.g. in the interactive or through a console app.

```fsharp
runTests defaultConfig simpleTest
```

which returns 1 if any tests failed, otherwise 0. Useful for returning to the
operating system as error code.

### `runTests`

Signature `ExpectoConfig -> Test -> int`. Runs the passed tests with the passed
configuration record.

