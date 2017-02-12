#r @"../../build/FParsecCS.dll"
#r @"../../build/FParsec.dll"
#load @"./FSONParser.fs"
open FSONParser
open FParsec
open System.Reflection

// File for misc scratch pad stuff

type Jurisdiction = 
    | BC | Alberta | Canada

type Address = 
    {Street: string;
    City: string; Region: Jurisdiction; 
    Postal: string option;
    Country: string}

let data = "
Street: 245 West Howe
City: Vancouver
Region: BC
Country: Canada"



typeof<string>.InvokeMember("Parse", BindingFlags.InvokeMethod, null, null, [|box "12"|])


parseFSON typeof<int16> "12"

typeof<string>.GetMethod("IsNullOrWhiteSpace")
"String".GetType() = typeof<string>

typeof<string>.GetMethods() 
    |> Array.filter (fun x -> x.IsStatic)
    |> Array.map (fun x -> x.Name)


type CustomParse2 =
  | Case1
  | Case2
  static member FSONParse(str: string) =
    match str with
      | "Case 1" -> Case1
      | "Case 2" -> Case2
      | _ -> failwith "Bad Custom Parse"

(parseFSON typeof<CustomParse2> "Case 2")

((parseFSON typeof<CustomParse2> "Case 1") :?> CustomParse2)