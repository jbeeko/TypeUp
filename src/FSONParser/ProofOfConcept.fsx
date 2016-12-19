//http://trelford.com/blog/post/parser.aspx
//http://www.quanttec.com/fparsec/reference/charparsers.html

#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
open System
open FParsec
open FSharp.Reflection
open System.Net
open System.Net.Mail

let mayThrow (p: Parser<'t,'u>) : Parser<'t,'u> =
    fun stream ->
        let state = stream.State        
        try 
            p stream
        with :? FormatException as e-> 
            stream.BacktrackTo(state)
            Reply(FatalError, messageError e.Message)

type FSharpType with
    static member IsOption (t : Type) = t.FullName = "FSharpOption`1"

type MailAddress with
    static member Parse str = MailAddress(str)

type Uri with
    static member Parse str = Uri(str)

let primTypeFromString (t:  Type) (str: String)  : obj =
    match t.FullName with
    |"System.Int16" -> Int16.Parse(str) |> box
    |"System.Int32" -> Int32.Parse(str) |> box
    |"System.Int64" -> Int64.Parse(str) |> box
    |"System.UInt16" -> UInt16.Parse(str) |> box
    |"System.UInt32" -> UInt32.Parse(str) |> box
    |"System.UInt64" -> UInt64.Parse(str) |> box
    |"System.Single" -> Single.Parse(str) |> box
    |"System.Double" -> Double.Parse(str) |> box
    |"System.Decimal" -> Decimal.Parse(str) |> box
    |"System.Boolean" -> Boolean.Parse(str) |> box
    |"System.Byte" -> Byte.Parse(str) |> box
    |"System.SByte" -> Byte.Parse(str) |> box
    |"System.Char" -> Char.Parse(str) |> box
    |"System.String" -> str |> box
    |"System.DateTime" -> DateTime.Parse str |> box
    |"System.Guid" -> Guid.Parse str |> box
    |"System.Uri" -> Uri.Parse str |> box
    |"System.Net.IPAddress" -> IPAddress.Parse str |> box
    |"System.Net.Mail.MailAddress" -> MailAddress.Parse str |> box
    |_ -> failwith "Unsupported primative type"

let primFromString (t: Type) (str: string) : obj =
    //this does not work for some reasion
    //t.GetMethod("Parse", [|typeof<string>|]).Invoke(t, [|str|]) |> box
    primTypeFromString t str

let rec pfieldValue(t : Type) : Parser<obj,unit> =
    let (|Record|_|) t = if FSharpType.IsRecord(t) then Some(t)  else None
    let (|Union|_|) t = if FSharpType.IsUnion(t) then Some(t) else None
    let (|EMail|_|) t = if t = typeof<MailAddress> then Some(t) else None        
    let (|GUID|_|) t = if t = typeof<Guid> then Some(t) else None        
    let (|Primative|_|) t = if Type.GetTypeCode(t) <> TypeCode.Object then Some(t) else None

    match t with
    | Record t -> precord t
    | Union t -> punion t
    | EMail t -> mayThrow(restOfLine false |>> (primFromString t))
    | GUID t -> mayThrow(restOfLine false |>> (primFromString t))
    | Primative t -> mayThrow(restOfLine false |>> (primFromString t))
    | _ -> fail "Unsupported object type"

and pfieldTag (field: Reflection.PropertyInfo) : Parser<_,_> =
    spaces>>.
    pstring field.Name >>.
    spaces>>.pchar ':'>>.
    spaces

and pfield (field: Reflection.PropertyInfo) : Parser<obj,unit> =
    pfieldTag field>>.
    pfieldValue field.PropertyType

and precord (aType : Type) : Parser<obj,unit> =
    let makeType vals = 
        FSharpValue.MakeRecord(aType, List.toArray vals)

    let pvalues = 
        FSharpType.GetRecordFields (aType)
        |> Array.map (fun f -> (pfield f) |>> List.singleton)
        |> Array.reduce (fun p1 p2 -> pipe2 p1 p2 List.append)
    spaces>>.pvalues |>> makeType

and puniontag  (t: Type) : Parser<_,_> =
    let parsers = 
        FSharpType.GetUnionCases t 
        |> Array.map (fun c -> spaces>>.pstring c.Name.>>spaces)
    choiceL parsers (sprintf "Expecting a case of %s" t.Name)

and punion (t : Type) : Parser<obj,unit> =
    let makeType tag = 
        let case = 
            FSharpType.GetUnionCases t
            |> Array.find (fun (c) -> c.Name = tag)
        FSharpValue.MakeUnion(case, [||])

    puniontag t |>> makeType

and ptype (aType : Type) : Parser<_,_> =
    if FSharpType.IsRecord aType then
       precord aType 
    else
        fun stream ->
            Reply(Error, expectedString "A Record")

//======================= TESTING ======================================

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

let paddress : Parser<Address,unit> = 
    let toAddress (anObj : obj) : Address =
        anObj :?> Address
    ptype typeof<Address> |>> toAddress

let test p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A of type %O" result result.GetType
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg
let addressData = "
Street: 245 West Howe
City: Vancouver
Region: BC
Country: Canada
"

test (ptype typeof<Address>) addressData
test paddress addressData

let pperson : Parser<Person,unit> = 
    let toPerson (anObj : obj) : Person =
        anObj :?> Person
    ptype typeof<Person> |>> toPerson

let personData = "
Name: Bill Smith
DOB: 1988-01-20
eMail: bill@co.com
Address: 
    Street: 245 West Howe
    City: Vancouver 
    Region: BC
    Country: Canada"

test (ptype typeof<Person>) personData
test pperson personData


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

test (ptype typeof<Contract>) contractData
test pcontract contractData

let contract : Contract = 
    {Number = 34343L;
    ID = Guid.Parse  "872ccb13-2e12-4eec-a2f5-ab64b3652b1c";
    Start = DateTime.Parse "2009-05-01";
    Jurisdiction = BC;
    Provider = 
        {Name = "Bill Smith";
        DOB = DateTime.Parse "1988-01-20";
        eMail = MailAddress.Parse "bill@co.com";
        Address =
            {Street = "245 West Howe";
            City = "Vancouver";
            Region = "BC";
            Country = "Canada" }};
    Holder =
        {Name = "Anne Brown";
        DOB = DateTime.Parse "1998-10-25";
        eMail = MailAddress.Parse "anne@co.com";
        Address =
            {Street = "5553 West 12th Ave";
            City = "Vancouver";
            Region = "BC";
            Country = "Canada" }}}

type tst = {Name : string option}
((FSharpType.GetRecordFields (typeof<tst>)).[0]).PropertyType
