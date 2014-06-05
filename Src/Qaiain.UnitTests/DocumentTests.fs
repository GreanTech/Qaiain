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
    let expected = message |> EmailData
    let actual = message |> ToDocumentType
    verify <@ expected = actual @>

[<Fact>]
let ParseEmailReferenceReturnsCorrectResult () =
    let expected = { DataAddress = "http://blobs.foo.bar/baz/qux" }
    let message =
        @"<?xml version=""1.0""?>
          <email-reference xmlns=""urn:grean:schemas:email-reference:2014"">
            <email-data-address>" +  expected.DataAddress + "</email-data-address>
          </email-reference>"
        |> System.Xml.Linq.XDocument.Parse
    let actual = message |> CreateEmailReference
    verify <@ expected = actual @>

[<Fact>]
let ParseEmailReturnsCorrectResult () =
    let expected = {
        From = { SmtpAddress = "foo@foo.com"
                 DisplayName = "Foo" }

        To = [| { SmtpAddress = "bar@bar.com"
                  DisplayName = "Bar" }
                { SmtpAddress = "qux@qux.com"
                  DisplayName = "Qux" } |]

        Subject = "Test"
        Body = "This is a test message."
    }
    let message =
        @"<?xml version=""1.0""?>
          <email xmlns=""urn:grean:schemas:email:2014"">
            <from>
              <smtp-address>foo@foo.com</smtp-address>
              <display-name>Foo</display-name>
            </from>
            <to>
              <address>
                <smtp-address>bar@bar.com</smtp-address>
                <display-name>Bar</display-name>
              </address>
              <address>
                <smtp-address>qux@qux.com</smtp-address>
                <display-name>Qux</display-name>
              </address>
            </to>
            <subject>Test</subject>
            <body>This is a test message.</body>
          </email>"
        |> System.Xml.Linq.XDocument.Parse
    let actual = message |> CreateEmailData
    verify <@ expected = actual @>