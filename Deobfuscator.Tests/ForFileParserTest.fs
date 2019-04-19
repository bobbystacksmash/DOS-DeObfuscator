namespace Deobfuscator.Tests.StatementMatcherTest

open NUnit.Framework
open Deobfuscator
open Deobfuscator.DomainTypes
open Deobfuscator.For.ForFileParser
open NUnit.Framework

[<TestFixture>]

type TestClass () =

    [<Test>]
    member this.ParseForFileSuccess() =

        let defaultTokenExpr = { Cols = []; UseWildcard = false }

        let defaults = {
            Skip = 0
            EOL = ';'
            Delims = [' '; '\t']
            Tokens = defaultTokenExpr
            UseBackq = false
        }

        // Successful Tests
        // ----------------
        let successfulTests = [

            ("", defaults, "Empty string = fetch defaults.")

            // Skip, decimal.
            ("skip=5",  { defaults with Skip = 5 }, "Interpret a skip value (dec).")
            ("skip=1",  { defaults with Skip = 1 }, "Interpret a skip value (dec).")
            ("skip=03", { defaults with Skip = 3 }, "Interpret a skip value with leading zero (dec).")

            // Skip, hex.
            ("skip=0xF",    { defaults with Skip = 15   }, "Interpret a skip value (hex, uc)")
            ("skip=0xa",    { defaults with Skip = 10   }, "Interpret a skip value (hex, lc)")
            ("skip=0xff",   { defaults with Skip = 255  }, "Interpret a skip value (hex, mixed)")
            ("skip=0x0012", { defaults with Skip = 18   }, "Interpret a skip value (hex, leading zeros)")

            // Skip, oct. (TODO)
            ("skip=017", { defaults with Skip = 15 }, "Interpret a skip value (oct)")

            // Skip, whtiespace
            ("  skip=2",  { defaults with Skip = 2 }, "Interpret a skip value (leading whitespace)")
            ("skip=4  ",  { defaults with Skip = 4 }, "Interpret a skip value (trailing whitespace)")
            ("  skip=6  ",{ defaults with Skip = 6 }, "Interpret a skip value (leading & trailing whitespace)")

            // Skip, Skip (overwriting defaults)
            ("skip=2 skip=3", { defaults with Skip = 3}, "Overwrite an existing Skip value")

            // Skip, with empty eol value.
            ("skip=2 eol=", {defaults with Skip = 2}, "Correctly handle a (trailing) empty EOL value")

            // EOL, with empty skip.

            ("eol= skip=3", {defaults with Skip = 3}, "Correctly handle (leading) empty EOL ")
            ("eol=", defaults, "Ignore empty EOL")
            ("eol= ", {defaults with EOL = ' '}, "Set EOL to an empty string when it appears last.")
            ("eol= eol=", defaults, "Ignore two empty EOLs.")
            ("eol=a eol=b", {defaults with EOL = 'b'}, "Take the latter of two EOLs")
            ("eol=x", {defaults with EOL = 'x'}, "Accept a single-char EOL value.")
            // Useback
            ("useback",  {defaults with UseBackq = true}, "Set usebackq when only 'useback' is given" )
            ("usebackq", {defaults with UseBackq = true}, "Set usebackq when only 'usebackq' is given" )

            // Tokens
            ("tokens=", {defaults with Tokens = { Cols = []; UseWildcard = false}}, "Clear the tokens array when empty.")
            ("tokens=*", {defaults with Tokens = { Cols = []; UseWildcard = true}}, "Handle lone wildcard.")
            ("tokens=1", {defaults with Tokens = { Cols = [1]; UseWildcard = false }}, "Handle simple tokens parsing.")
            ("tokens=3*", {defaults with Tokens = { Cols = [3]; UseWildcard = true}}, "Handle num + wildcard.")
            ("tokens=3,*", {defaults with Tokens = { Cols = [3]; UseWildcard = true}}, "Handle num + wildcard separated by comma.")
            ("tokens=1,5*", {defaults with Tokens = { Cols = [1; 5]; UseWildcard = true }}, "Handle decimal range with wildcard.")
            ("tokens=1,2,3,4,5,6", {defaults with Tokens = { Cols = [1..6]; UseWildcard = false }}, "Handle large number of columns.")
            ("tokens=1-2,2-3,3,*", {defaults with Tokens = { Cols = [1; 2; 3]; UseWildcard = true }}, "Correctly parse token expr.")
            ("tokens=0xa", {defaults with Tokens = { Cols = [10]; UseWildcard = false }}, "Handle a single hex column.")
            ("tokens=0x1,0xa,*", {defaults with Tokens = { Cols = [1; 10]; UseWildcard = true}}, "Parse hex tokens.")
            ("tokens=0xa-0xf", {defaults with Tokens = { Cols = [10; 11; 12; 13; 14; 15]; UseWildcard = false } }, "Handle hex ranges")
            ("tokens=03", {defaults with Tokens = { Cols = [3]; UseWildcard = false }}, "Handle single octal literal.")
            ("tokens=011", {defaults with Tokens = { Cols = [9]; UseWildcard = false}}, "Handle literal octal.")
            ("tokens=017", {defaults with Tokens = { Cols = [15]; UseWildcard = false}}, "Handle octal numbers.")
            ("tokens=015-017*", {defaults with Tokens = { Cols = [13; 14; 15]; UseWildcard = false}}, "Handle octal ranges + wildcard")
            ("tokens=1-2 tokens=2-4", {defaults with Tokens = { Cols = [2..4]; UseWildcard = false}}, "Latter 'tokens=' overwrites former")
            ("tokens= ", defaults, "Allow empty tokens keyword when at the end of the expr.")

            //
            // Delims
            //
            ("delims=,", {defaults with Delims = [',']}, "Set delimiter correctly to a comma.")
            ("delims=abc", {defaults with Delims = ['a'; 'b'; 'c';]}, "Set delimiter to be multiple chars.")
            ("delims= ", {defaults with Delims = [' ']}, "Allow empty string 'delims' keyword.")
            ("delims=", defaults, "Allow unallocated delims keyword.")
            ("delims= delims=", {defaults with Delims = []}, "Allow two unallocated delims keywords.")
            ("delims=a delims=b", {defaults with Delims = ['b']}, "Overwrite previous delimiters.")

            //
            // Mixed-keyword tests
            //
            (
                "tokens=1 eol=; useback",
                {defaults with Tokens = { Cols = [1]; UseWildcard = false}; EOL = ';'; UseBackq = true},
                "Correctly parse multiple keywords in the same expr."
            )
            (
                "eol=! tokens=",
                {defaults with EOL = '!'; Tokens = { Cols = []; UseWildcard = false}},
                "Correctly handle an empty 'tokens=' keyword in a mixed-keyword expression."
            )
            (
                "eol= delims=",
                defaults,
                "Handle empty eol and delims keywords."
            )
        ]

        successfulTests |> List.iter (fun test ->
            let input, expected, msg = test
            let output = parseForFArgs input

            printfn "========================="
            printfn "Input    -> [%s]" input
            printfn "Output   -> %A"   output
            printfn "Expected -> %A"   expected
            printfn "Msg      -> %s"   msg
            printfn "========================="

            match output with
            | Ok output ->
                Assert.That(output, Is.EqualTo(expected), msg)

            | Error reason ->
                Assert.Fail("FAILED: " + msg)
        )

    [<Test>]
    member this.ParseForFileErrors() =
        // April, 2019:
        // This is *REALLY* hacky, and I'm sure there's a smarter way to do exception
        // checking.  Perhaps returning a Result is the wrong pattern -- maybe exceptions
        // are better suited?  Either way, still learning F# so this'll have to do (for now).
        let checkCorrectErrorReturned errName output =
            printfn "Checking ErrName -> %A" errName
            match output with
            | Ok _ -> false
            | Error err ->
                printfn "What type of err? > %A" err
                match err with
                | FeatureNotImplemented -> false
                | InvalidKeyword _ when errName = "InvalidKeyword" -> true
                | KeywordSkipValueIsNotNumeric _ when errName = "KeywordSkipValueIsNotNumeric" -> true
                | KeywordSkipCannotBeZero _ when errName = "KeywordSkipCannotBeZero" -> true
                | KeywordTokensIsInvalid _ when errName = "KeywordTokensIsInvalid" -> true
                | ExpectedParseKeywordValue _ when errName = "ExpectedParseKeywordValue" -> true
                | KeywordEolTooManyChars _ when errName = "KeywordEolTooManyChars" -> true
                | _ -> false


        // Error Tests
        // -----------
        // "eol=delims="                [elims="] was unexpected at this time.
        // "delims=a b c"               [b c"] was unexpected at this time.
        // "tokens=a"                   [a"] was unexpected at this time.
        // "tokens=0a"                  [a"] was unexpected at this time.
        // "tokens=0,3"                 [,3"] was unexpected at this time.
        // "tokens=0"                   ["] was unexpected at this time.
        // "tokens=1,0"                 ["] was unexpected at this time.
        //
        let failingTests = [
            // EOL
            ("eol",    "InvalidKeyword", "EOL on its own is illegal.")
            ("skip",   "InvalidKeyword", "SKIP on its own is illegal.")
            ("delims", "InvalidKeyword", "DELIMS on its own is illegal.")
            ("tokens", "InvalidKeyword", "TOKENS on its own is illegal.")
            ("eol=abc", "KeywordEolTooManyChars", "Should not allow multiple chars to be set for EOL.")


            ("skip=a", "KeywordSkipValueIsNotNumeric", "Should fail to parse a `skip' keyword when the value is not numeric.")
            ("skip=0", "KeywordSkipCannotBeZero", "Should not allow skip to equal zero.")

            ("tokens=0x00-0x01", "KeywordTokensIsInvalid", "Should not allow (hex) zero values to be set in tokens keyword.")
            ("tokens=0", "KeywordTokensIsInvalid", "Should not allow (dec) zero values to be set in tokens keyword.")
            ("tokens=00", "KeywordTokensIsInvalid", "Should not allow (oct) zero values to be set in tokens keyword.")

            ("tokens= eol=;", "ExpectedParseKeywordValue", "Should not allow the tokens= keyword to be a single space.")
        ]

        let runTest test =
            let input, expectedErr, msg = test
            let output = parseForFArgs input
            let outbuf = [
                "========================="
                (sprintf "Input  -> [%s]" input)
                (sprintf "Output -> %A"   output)
                (sprintf "Msg    -> %s"   msg)
                "========================="
            ]
            (checkCorrectErrorReturned expectedErr output, outbuf)


        let findFailingTests testResult =
            match testResult with
            | (false, reason) -> true
            | _ -> false


        let results =
            failingTests
            |> List.map runTest
            |> List.filter findFailingTests

        if results.Length = 0 then
            Assert.Pass("All expected errors were thrown.")
        else
            results
            |> List.map snd
            |> List.iter (fun output -> output |> List.iter (fun line -> printfn "%s" line))
            Assert.Fail("Test ran without erroring (this is bad - these tests check errors!)")


