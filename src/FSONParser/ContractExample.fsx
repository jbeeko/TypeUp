//Sample data file
(*
---
module : ContractModel
---

let billsAddress: 
    Address street: 44 East 5th Ave
    city: Vancouver region: BC
    country: Canada

Contract number: 34343
start: 2009-05-01
provider:
    Company name: Cedar Systems
    phones:
        1: Office 604 897 9729
        2: AfterHours 604 897 9729
    site: http://www.cedar.com
    incorpLoc: Alberta
    beneficialOwners:
        1: Person name: Joerg        
            id: 81a130d2-502f-4cf1-a376-63edeb000e9f
            ipAddress: 10.0.0.12
            eMail: None
            phone: Mobile 604 897 9729
            dob: 1967-03-27    sex: Male
            occupations: 
        2: Company name: Acme Holdings
            phones: 
            site: None
            incorpLoc: Alberta
            beneficialOwners:
            1: Person name: Cindy        
                id: 51a130d2-502f-4cf1-a376-63edeb000da1
                ipAddress: 10.0.0.11
                eMail: cindy@bar.com
                phone: none
                dob: 1961-04-2    sex: Female
                occupations: 
holder: 
    Person name: Bill         
    id: 8f76ba2d-f5e9-4c28-a145-b42325be1ea7
    ipAddress: 2001:db8:a0b:12f0::1
    eMail: bill@foo.com
    phone: None
    dob: 1988-05-01    sex: Male
    occupations: 1: Doctor 2: Pilot
    address: billsAddress

*)



// Comments
(*  Editor gives option to create new where ever a record is expected. 
    * root populated by default
    * record fields
    * after a let, this is used to create records that will be reused.

    When selecting "new" command a new record is full dropped in with 
        * None for options
        * Some placeholder for other values such as Strings, Ints, DateTimes etc.  Not sure how this 
          will work. Need an invalid but plausible looking value for all basic types.
        * Tuples a pair of plausible values. 
        * Choose for union types with choice of cases
        * Choose where one or more let defined records will fit

    Records defined in a let and then reused in root will be written out that way when "pretty printed"

    Record types are always introducted by a lable and then begin on the next line. 

    Type anotation are not needed. However since a union constructor is needed it appears such an anotation is needed
    when introducting a union case. For example Person or Company. 

    Items defined in let bindings but not used are an error.

    There is no explicit separator character. Separators are: 
    * For records the list of all possible <whitespace><field name<colon><whitespace> values.
    * For lists the tokens <whitespace><integer<colon><whitespace>
    * For tuples <whitespace><dash<whitespace> NOTE: this is not the greatest as "dash" is not uncommon.
    
    No such values may appear in any of the data elements. If any data element requires such a value it must be 
    entered as a tripple escabled string. 

    The last item defined is returned as the value of the document.

    When serializing back should they system perform automatic extraction of equal values?

    The fson representation should be convertible to json representation using the following rules:
        *...

    Arbitrary simplifications
        * no record types or lists into tupples. Tupples are only for simple pairs like:
          'Mobile - 604 234 2233' where the tuple is a phone type and a number. 
        * actually perhaps no need to support tupples at all. They are not a very strong modeling concept
*)

open System
open System.Net
open System.Net.Mail
open System.Text.RegularExpressions

module ContractModel =
    type MailAddress with
        static member Parse email =
            MailAddress email

    type Uri with
        static member Parse uri =
            Uri uri

    type Gender = 
        | Male
        | Female
    
    type Phone =
        | Mobile of String
        | Office of String
        | Home of String
        | AfterHours of String 
        | Other of String with
         static member Parse number =
            if (Regex.Match(number, @"[0-9]{3}[-. ][0-9]{3}[-. ][0-9]{4}")).Success
                then number
                else failwithf "'%s' is not a phone number" number
     
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
    
    type Address = {
        street: String;
        city: String; region: String;
        country: String;
    }

    type Contract = {
        number: int64
        start : DateTime;
        provider : LegalEntity;
        holder : LegalEntity;
    }
    and LegalEntity = 
        | Person of Person
        | Company of Company
        | Tag of String
    and Company = {
        name: String;
        phones: Phone list;
        site: Uri option;
        incorpLoc: Jurisdiction;
        beneficialOwners: LegalEntity list;
    }
    and Person = { 
        name : String;
        id : Guid;
        ipAddress: IPAddress;
        eMail : MailAddress option;
        phone: Phone option;
        dob: DateTime; sex: Gender;
        occupations: Occupation list;
        address: Address option;
        }


module data =
    open ContractModel
    let root = {
        number = Int64.Parse "34343";
        start = DateTime.Parse "2009-05-01"
        provider = Company {
            name = "Cedar Systems";
            phones = [Office (Phone.Parse "778-234-5567"); AfterHours (Phone.Parse "604-444-7788")];
            site = Some(Uri.Parse "http://www.cedar.com");
            incorpLoc = Alberta;
            beneficialOwners = 
                [   Person {
                    name = "Joerg";
                    id = Guid.Parse "81a130d2-502f-4cf1-a376-63edeb000e9f";
                    eMail = None
                    phone = Some(Mobile (Phone.Parse "604 897 9729"));
                    ipAddress = IPAddress.Parse "10.0.0.12"
                    dob = DateTime.Parse "1967-03-27";  sex = Male;
                    occupations = [];
                    address = None; 
                    };
                    Company {
                        name = "Acme Holdings";
                        phones = [];
                        site = None;
                        incorpLoc = Alberta;
                        beneficialOwners = 
                            [   Person {
                                name = "Cindy";
                                id = Guid.Parse "51a130d2-502f-4cf1-a376-63edeb000da1";
                                eMail = Some(MailAddress ("cindy@bar.com"));
                                phone = None;
                                ipAddress = IPAddress.Parse "10.0.0.11"
                                dob = DateTime.Parse "1961-04-2";   sex = Female;
                                occupations = [];
                                address = None;
                                };
                            ]
                    }
                ]
            };
        holder = Person {
            name = "Bill";
            id = Guid.Parse "8f76ba2d-f5e9-4c28-a145-b42325be1ea7";
            ipAddress = IPAddress.Parse "2001:db8:a0b:12f0::1";
            eMail = Some(MailAddress.Parse "bill@foo.com");
            phone = None;
            dob = DateTime.Parse "1988-05-01";  sex = Male;
            occupations = [Doctor; Pilot];
            address = Some({
                            street = "44 East 5th Ave";
                            city = "Vancouver";
                            region = "BC";
                            country = "Canada";})      
            };
    }
data.root


// let contract : Contract = 
//     {Number = 34343L;
//     ID = Guid.Parse  "872ccb13-2e12-4eec-a2f5-ab64b3652b1c";
//     Start = DateTime.Parse "2009-05-01";
//     Jurisdiction = BC;
//     Provider = 
//         {Name = "Bill Smith";
//         DOB = DateTime.Parse "1988-01-20";
//         eMail = MailAddress.Parse "bill@co.com";
//         Address =
//             {Street = "245 West Howe";
//             City = "Vancouver";
//             Region = "BC";
//             Country = "Canada" }};
//     Holder =
//         {Name = "Anne Brown";
//         DOB = DateTime.Parse "1998-10-25";
//         eMail = MailAddress.Parse "anne@co.com";
//         Address =
//             {Street = "5553 West 12th Ave";
//             City = "Vancouver";
//             Region = "BC";
//             Country = "Canada" }}}
