module Grean.Qaiain.UnitTests.MailTests

open Xunit

let verify = Swensen.Unquote.Assertions.test

[<Fact>]
let Passes () =
    verify <@ 1 = 1 @>
