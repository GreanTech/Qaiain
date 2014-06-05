module Grean.Qaiain.UnitTests.DocumentTests

open Grean.Qaiain
open Xunit
open Xunit.Extensions

let verify = Swensen.Unquote.Assertions.test

[<Theory>]
[<InlineData("")>]
[<InlineData("1234567890")>]
[<InlineData("urn:foo:schemas:bar")>]
[<InlineData("urn:bar:schemas:baz:1234")>]
[<InlineData("urn:grean:schemas:email-reference:2011")>]
[<InlineData("urn:grean:schemas:email-reference:2012")>]
[<InlineData("urn:grean:schemas:email-reference:2013")>]
[<InlineData("urn:grean:schemas:email-reference:2015")>]
let ToDocumentTypeForUnknownNamespaceNameReturnsCorrectResult namespaceName =
    let message =
        @"<?xml version=""1.0""?>
          <email-reference xmlns=""" + namespaceName + @""">
          </email-reference>"
        |> System.Xml.Linq.XDocument.Parse
    let actual = message |> ToDocumentType
    verify <@ Unknown = actual @>

[<Fact>]
let ToDocumentTypeForKnownEmailReferenceNamespaceNameReturnsCorrectResult () =
    let message =
        @"<?xml version=""1.0""?>
          <email-reference xmlns=""urn:grean:schemas:email-reference:2014"">
          </email-reference>"
        |> System.Xml.Linq.XDocument.Parse
    let expected = message |> EmailReference
    let actual = message |> ToDocumentType
    verify <@ expected = actual @>

[<Fact>]
let ToDocumentTypeForKnownEmailNamespaceNameReturnsCorrectResult () =
    let message =
        @"<?xml version=""1.0""?>
          <email xmlns=""urn:grean:schemas:email:2014"">
          </email>"
        |> System.Xml.Linq.XDocument.Parse
    let expected = message |> Email
    let actual = message |> ToDocumentType
    verify <@ expected = actual @>
