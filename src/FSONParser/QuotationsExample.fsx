//http://fsprojects.github.io/FSharp.Quotations.Evaluator/index.html
#r @"..\..\packages\FSharp.Quotations.Evaluator\lib\net40\FSharp.Quotations.Evaluator.dll"
open FSharp.Quotations.Evaluator
open System.Collections.Generic


let value = QuotationEvaluator.Evaluate <@ 1 + 1 @>  
