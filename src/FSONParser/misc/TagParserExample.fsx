#r @"..\..\packages\FParsec\lib\net40-client\FParsecCS.dll"
#r @"..\..\packages\FParsec\lib\net40-client\FParsec.dll"
open FParsec
open System.Collections.Generic

type Tag = Bold of string
         | Url of string * string

// We store the tag parser dictionary in the user state, so that we can
// concurrently parse multiple input streams with the same parser instance
// but differerent tag dictionaries.

type TagParserMap = Dictionary<string,Parser<Tag,UserState>>

and UserState = {
        TagParsers: TagParserMap
     }

let defaultTagParsers = TagParserMap()

let isTagNameChar1 = fun c -> isLetter c || c = '_'
let isTagNameChar = fun c -> isTagNameChar1 c || isDigit c
let expectedTag = expected "tag starting with '['"

let tag : Parser<Tag, UserState> =
  fun stream ->
    if stream.Skip('[') then
        let name = stream.ReadCharsOrNewlinesWhile(isTagNameChar1, isTagNameChar,
                                                   false)
        if name.Length <> 0 then
            let mutable p = Unchecked.defaultof<_>
            if stream.UserState.TagParsers.TryGetValue(name, &p) then p stream
            else
                stream.Skip(-name.Length)
                Reply(Error, messageError ("unknown tag name '" + name + "'"))
        else Reply(Error, expected "tag name")
    else Reply(Error, expectedTag)

let str s = pstring s
let ws = spaces
let text = manySatisfy (function '['|']' -> false | _ -> true)

defaultTagParsers.Add("b", str "]" >>. text .>> str "[/c]" |>> Bold)

defaultTagParsers.Add("url",      (str "=" >>. manySatisfy ((<>)']') .>> str "]")
                             .>>. (text .>> str "[/url]")
                             |>> Url)

let parseTagString str =
    runParserOnString tag {TagParsers = TagParserMap(defaultTagParsers)} "" str

//parseTagString "[b]bold text[/b]";;

//parseTagString "[url=http://tryfsharp.org]try F#[/url]";;

//parseTagString "[bold]test[/bold]";;




