//https://fsharp.github.io/FSharp.Compiler.Service/index.html
//http://trelford.com/blog/post/parser.aspx
//http://www.quanttec.com/fparsec/reference/charparsers.html

#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
open System
open FParsec
open FSharp.Reflection

let test p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg

let pdatetime : Parser<DateTime,_> =                                        
    let error = expectedString "a DateTime"  
    let pattern = "\d\d\d\d-\d\d-\d\d"
    fun stream ->  
        let reply = (regex pattern) stream  
        if reply.Status = Ok then 
            try 
                Reply(DateTime.Parse reply.Result)
            with 
            | :? FormatException as ex ->
                Reply(Error, error)
        else   
            Reply(Error, error)
test pdatetime "1988-07-23"

let pguid : Parser<Guid,_> =                                        
    let error = expectedString "a GUID"  
    let pattern = "[0-9a-f]{32}|[({]?[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}[)}]?"
    fun stream ->  
        let reply = (regex pattern) stream  
        if reply.Status = Ok then 
            try 
                Reply(Guid.Parse reply.Result)
            with 
            | :? FormatException as ex ->
                Reply(Error, error)
        else   
            Reply(Error, error)

test pguid "{12345678-9012-3456-7890-123456789012}"
Guid.Parse("56ec5aab2b-c136-4ac2-b4d0-dce7da32ceb3")
Guid.Parse("12345678a01234567890123456789012")