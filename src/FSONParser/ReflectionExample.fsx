//https://msdn.microsoft.com/visualfsharpdocs/conceptual/fsharptype.getrecordfields-method-%5bfsharp%5d

#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
open FParsec
open System.Collections.Generic

let test p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg

test (restOfLine false) "asd"


module ReflectionExp = 
    open FSharp.Reflection
    open FSharp.Reflection.FSharpReflectionExtensions
    type TestRec = {
        name: string;
        dob: int;
    }
    
    let foo = { name = "joerg"; dob = 3}
    foo
    FSharpType.GetRecordFields (typeof<TestRec>)
    FSharpValue.MakeRecord (typeof<TestRec>, [|box "33"; box 45.0|])


    box "asdfasd"


    typeof<TestRec>
    typedefof<TestRec>

    Reflection.FSharpType.IsRecord(typeof<TestRec>)


    System.Type.GetType("TestRec")