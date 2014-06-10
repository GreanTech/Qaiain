module Grean.Qaiain.UnitTests.MailTests

open Program.Mail
open Xunit

let verify = Swensen.Unquote.Assertions.test

[<Fact>]
let ParseReturnsCorrectResult () =
    let input =
     """<?xml version="1.0"?>
        <email xmlns="urn:grean:schemas:email:2014">
          <from>
            <smtp-address>foo@foo.com</smtp-address>
            <display-name>Foo</display-name>
          </from>
          <to>
            <address>
              <smtp-address>bar@bar.com</smtp-address>
              <display-name>Bar</display-name>
            </address>
          </to>
          <subject>Test</subject>
          <body>This is a test message.</body>
        </email>"""
    let expected =
        {
            From = { SmtpAddress = "foo@foo.com"; DisplayName = "Foo" }
            To = [| { SmtpAddress = "bar@bar.com"; DisplayName = "Bar" } |]
            Subject = "Test"
            Body = "This is a test message."
        }
        |> EmailData

    let actual = Parse input

    verify <@ expected = actual @>
