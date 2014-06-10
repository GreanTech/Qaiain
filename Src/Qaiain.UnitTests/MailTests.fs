module Grean.Qaiain.UnitTests.MailTests

open Grean.Exude
open Program.Mail
open Xunit

let verify = Swensen.Unquote.Assertions.test

type private TestCase = { input : string
                          expected : EmailMessage }

[<FirstClassTests>]
let ParseReturnsCorrectResult () =
    [
        { input =
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
          expected = { From = { SmtpAddress = "foo@foo.com"
                                DisplayName = "Foo" }
                       To = [| { SmtpAddress = "bar@bar.com";
                                 DisplayName = "Bar" } |]
                       Subject = "Test"
                       Body = "This is a test message." }
                     |> EmailData }

        { input =
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
                  <address>
                    <smtp-address>qux@qux.com</smtp-address>
                    <display-name>Qux</display-name>
                  </address>
                </to>
                <subject>Test</subject>
                <body>This is a test message.</body>
              </email>"""
          expected = { From = { SmtpAddress = "foo@foo.com"
                                DisplayName = "Foo" }
                       To = [| { SmtpAddress = "bar@bar.com";
                                 DisplayName = "Bar" }
                               { SmtpAddress = "qux@qux.com";
                                 DisplayName = "Qux" } |]
                       Subject = "Test"
                       Body = "This is a test message." }
                     |> EmailData }
    ]
    |> Seq.map (fun tc -> TestCase (fun () ->
        let actual = Parse tc.input
        verify <@ tc.expected = actual @>))
