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

        { input =
           """<?xml version="1.0"?>
              <email xmlns:e="urn:grean:schemas:email:2014">
                <e:from>
                  <e:smtp-address>foo@foo.com</e:smtp-address>
                  <e:display-name>Foo</e:display-name>
                </e:from>
                <e:to>
                  <e:address>
                    <e:smtp-address>bar@bar.com</e:smtp-address>
                    <e:display-name>Bar</e:display-name>
                  </e:address>
                </e:to>
                <e:subject>Test</e:subject>
                <e:body>This is a test message.</e:body>
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
              <email xmlns:e="urn:grean:schemas:email:2014">
                <e:from>
                  <e:smtp-address>foo@foo.com</e:smtp-address>
                  <e:display-name>Foo</e:display-name>
                </e:from>
                <e:to>
                  <shouldBeIgnored>
                    <smtp>bar@bar.com</smtp>
                    <name>Bar</name>
                  </shouldBeIgnored>
                  <e:address>
                    <e:smtp-address>qux@qux.com</e:smtp-address>
                    <e:display-name>Qux</e:display-name>
                  </e:address>
                </e:to>
                <e:subject>Test</e:subject>
                <e:body>This is a test message.</e:body>
              </email>"""
          expected = { From = { SmtpAddress = "foo@foo.com"
                                DisplayName = "Foo" }
                       To = [| { SmtpAddress = "qux@qux.com";
                                 DisplayName = "Qux" } |]
                       Subject = "Test"
                       Body = "This is a test message." }
                     |> EmailData }

        { input =
           """<?xml version="1.0"?>
              <email xmlns="">
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
          expected = Unknown }

        { input = "<foo />"
          expected = Unknown }
    ]
    |> Seq.map (fun tc -> TestCase (fun () ->
        let actual = parse tc.input
        verify <@ tc.expected = actual @>))
