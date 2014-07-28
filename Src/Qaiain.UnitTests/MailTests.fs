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
                       Body = "This is a test message."
                       Attachments = [] }
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
                       Body = "This is a test message."
                       Attachments = [] }
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
                       Body = "This is a test message."
                       Attachments = [] }
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
                       Body = "This is a test message."
                       Attachments = [] }
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

        { input = ""
          expected = Unknown }

        { input = "  "
          expected = Unknown }

        { input = "<foo>"
          expected = Unknown }

        { input = "<bar"
          expected = Unknown }

        { input =
           """<?xml version="1.0"?>
              <email-reference xmlns="urn:grean:schemas:email:2014">
                <data-address>http://blobs.foo.bar/baz/qux</data-address>
              </email-reference>"""
          expected = { DataAddress = "http://blobs.foo.bar/baz/qux" }
                     |> EmailReference }

        { input =
           """<?xml version="1.0"?>
              <email-reference xmlns="urn:grean:schemas:email:2014">
              </email-reference>"""
          expected = Unknown }

        { input =
           """<?xml version="1.0"?>
              <email-reference xmlns:e="urn:grean:schemas:email:2014">
                <e:data-address>http://a.b.c/x/y</e:data-address>
              </email-reference>"""
          expected = { DataAddress = "http://a.b.c/x/y" }
                     |> EmailReference }

        { input =
           """<?xml version="1.0"?>
              <email xmlns="urn:grean:schemas:email:2014">
                <from>
                  <smtp-address>foo@foo.com</smtp-address>
                  <display-name>Foo</display-name>
                </from>
                <to>
                  <address>
                    <display-name>Bar</display-name>
                    <smtp-address>bar@bar.com</smtp-address>
                  </address>
                  <address>
                    <display-name>Qux</display-name>
                    <smtp-address>qux@qux.com</smtp-address>
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
                       Body = "This is a test message."
                       Attachments = [] }
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
                    <display-name>Bar</display-name>
                    <smtp-address>bar@bar.com</smtp-address>
                  </address>
                  <address>
                    <display-name>Qux</display-name>
                    <smtp-address>qux@qux.com</smtp-address>
                  </address>
                </to>
                <subject>Test</subject>
                <body>This is a test message.</body>
                <attachments>
                  <attachment>
                    <content>MQ==</content>
                    <mime-type>image/png</mime-type>
                    <name>XyzImage</name>
                  </attachment>
                  <attachment>
                    <content>Mg==</content>
                    <mime-type>audio/mpeg</mime-type>
                    <name>XyzAudio</name>
                  </attachment>
                  <attachment>
                    <content>Mw==</content>
                    <mime-type>video/mp4</mime-type>
                    <name>XyzVideo</name>
                  </attachment>
                </attachments>
              </email>"""
          expected = { From = { SmtpAddress = "foo@foo.com"
                                DisplayName = "Foo" }
                       To = [| { SmtpAddress = "bar@bar.com";
                                 DisplayName = "Bar" }
                               { SmtpAddress = "qux@qux.com";
                                 DisplayName = "Qux" } |]
                       Subject = "Test"
                       Body = "This is a test message."
                       Attachments = [ { Content = [| 49uy |]
                                         MimeType = "image/png"
                                         Name = "XyzImage" }
                                       { Content = [| 50uy |]
                                         MimeType = "audio/mpeg"
                                         Name = "XyzAudio" }
                                       { Content = [| 51uy |]
                                         MimeType = "video/mp4"
                                         Name = "XyzVideo" } ] }
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
                    <e:display-name>Bar</e:display-name>
                    <e:smtp-address>bar@bar.com</e:smtp-address>
                  </e:address>
                  <e:address>
                    <e:display-name>Qux</e:display-name>
                    <e:smtp-address>qux@qux.com</e:smtp-address>
                  </e:address>
                </e:to>
                <e:subject>Test</e:subject>
                <e:body>This is a test message.</e:body>
                <e:attachments>
                  <e:attachment>
                    <e:content>MTIz</e:content>
                    <e:mime-type>audio/vnd.wave</e:mime-type>
                    <e:name>6771bc9c-5d67-452b-8b75</e:name>
                  </e:attachment>
                </e:attachments>
              </email>"""
          expected = { From = { SmtpAddress = "foo@foo.com"
                                DisplayName = "Foo" }
                       To = [| { SmtpAddress = "bar@bar.com";
                                 DisplayName = "Bar" }
                               { SmtpAddress = "qux@qux.com";
                                 DisplayName = "Qux" } |]
                       Subject = "Test"
                       Body = "This is a test message."
                       Attachments = [ { Content = [| 49uy; 50uy; 51uy |]
                                         MimeType = "audio/vnd.wave"
                                         Name = "6771bc9c-5d67-452b-8b75" } ] }
                     |> EmailData }
    ]
    |> Seq.map (fun tc -> TestCase (fun () ->
        let actual = parse tc.input
        verify <@ tc.expected = actual @>))

open Program
open System
open Swensen.Unquote.Assertions

[<Fact>]
let HandleSendsCorrectEmail () =
    let verified = ref false
    let expected = { From = { SmtpAddress = "foo@foo.com"
                              DisplayName = "Foo" }
                     To = [| { SmtpAddress = "bar@bar.com";
                               DisplayName = "Bar" } |]
                     Subject = "Test"
                     Body = "This is a test message."
                     Attachments = [] }
    let sendEmail actual =
        verified := expected = actual
        ()
    let handle message =
        handle (fun x -> "" |> Some) ignore sendEmail message |> ignore

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
    |> handle

    verify <@ verified = ref true @>

[<Fact>]
let HandleReturnsCorrectResultForUnknownMessage () =
    let verified = ref false
    let expected = UnknownMessageType
    let handle message =
        handle (fun x -> "" |> Some) ignore ignore message

    let actual =
        match "<bar" |> handle with
        | Failure actual -> verified := expected = actual
        | Success _ -> verified := false

    verify <@ verified = ref true @>

[<Fact>]
let HandleSendsCorrectEmailForPointerMessages () =
    let verified = ref false
    let expected = { From = { SmtpAddress = "foo@foo.com"
                              DisplayName = "Foo" }
                     To = [| { SmtpAddress = "bar@bar.com";
                               DisplayName = "Bar" } |]
                     Subject = "Test"
                     Body = "This is a test message."
                     Attachments = [ { Content = [| 49uy |]
                                       MimeType = "image/png"
                                       Name = "Quux" } ] }
    let sendEmail actual =
        verified := expected = actual
        ()
    let dataAddress = "http://blobs.foo.bar/baz/qux"
    let getMessage actual =
        if actual = dataAddress then
            """<?xml version="1.0"?>
               <email xmlns="urn:grean:schemas:email:2014">
                 <from>
                   <smtp-address>foo@foo.com</smtp-address>
                   <display-name>Foo</display-name>
                 </from>
                 <to>
                   <address>
                     <display-name>Bar</display-name>
                     <smtp-address>bar@bar.com</smtp-address>
                   </address>
                 </to>
                 <subject>Test</subject>
                 <body>This is a test message.</body>
                 <attachments>
                   <attachment>
                     <content>MQ==</content>
                     <mime-type>image/png</mime-type>
                     <name>Quux</name>
                   </attachment>
                 </attachments>
               </email>"""
            |> Some
        else None
    let handle message =
        handle getMessage ignore sendEmail message

    """<?xml version="1.0"?>
       <email-reference xmlns:e="urn:grean:schemas:email:2014">
         <e:data-address>""" + dataAddress + """</e:data-address>
       </email-reference>"""
    |> handle |> ignore

    verify <@ verified = ref true @>

[<Fact>]
let HandleDeletesCorrectMessageForPointerMessages () =
    let verified = ref false
    let expected = "http://blobs.foo.bar/baz/qux"
    let deleteMessage actual =
        verified := expected = actual
        ()
    let getMessage _ =
        """<?xml version="1.0"?>
           <email xmlns="urn:grean:schemas:email:2014">
             <from>
               <smtp-address>foo@foo.com</smtp-address>
               <display-name>Foo</display-name>
             </from>
             <to>
               <address>
                 <display-name>Bar</display-name>
                 <smtp-address>bar@bar.com</smtp-address>
               </address>
             </to>
             <subject>Test</subject>
             <body>This is a test message.</body>
             <attachments>
               <attachment>
                 <content>MQ==</content>
                 <mime-type>image/png</mime-type>
                 <name>Quux</name>
               </attachment>
             </attachments>
           </email>"""
        |> Some
    let handle message =
        handle getMessage deleteMessage ignore message

    """<?xml version="1.0"?>
       <email-reference xmlns:e="urn:grean:schemas:email:2014">
         <e:data-address>""" + expected + """</e:data-address>
       </email-reference>"""
    |> handle |> ignore

    verify <@ verified = ref true @>

[<Fact>]
let HandleDoesNotDeletesNonExistingBlobs () =
    let verified = ref false
    let deleteMessage _ =
        verified := true
        ()
    let handle message =
        handle (fun x -> None) deleteMessage ignore message

    """<?xml version="1.0"?>
       <email-reference xmlns:e="urn:grean:schemas:email:2014">
         <e:data-address>http://non.existing.blob/ni/kos</e:data-address>
       </email-reference>"""
    |> handle |> ignore

    verify <@ verified = ref false @>
