namespace Grean.Qaiain

open System.Xml
open System.Xml.Linq

type EmailDocument =
    | EmailData of XDocument
    | EmailReference of XDocument
    | Unknown

[<AutoOpen>]
module Document =
    let ToEmailDocument (document : XDocument) =
        match document.Root.Name.Namespace.NamespaceName with
        | "urn:grean:schemas:email:2014" -> EmailData document
        | "urn:grean:schemas:email-reference:2014" -> EmailReference document
        | _ -> Unknown

    let CreateEmailReference (document : XDocument) =
        let (/-) (node : XContainer) name = node.Elements name
        let (/+) (el : XElement seq) name = el |> Seq.collect(fun x -> x /- name)
        let navigate name =
            XName.Get(name, document.Root.Name.Namespace.NamespaceName)

        {
            DataAddress =
                document
                /- navigate "email-reference"
                /+ navigate "email-data-address"
                |> Seq.map (fun (x : XElement) -> x.Value)
                |> Seq.exactlyOne
        }

    let CreateEmailData (document : XDocument) =
        let (/-) (node : XContainer) name = node.Elements name
        let (/+) (el : XElement seq) name = el |> Seq.collect(fun x -> x /- name)
        let navigate name =
            XName.Get(name, document.Root.Name.Namespace.NamespaceName)

        let getSenderSmtpAddress =
            document
            /- navigate "email"
            /+ navigate "from"
            /+ navigate "smtp-address"
            |> Seq.map (fun (x : XElement) -> x.Value)
            |> Seq.exactlyOne

        let getSenderDisplayName =
            document
            /- navigate "email"
            /+ navigate "from"
            /+ navigate "display-name"
            |> Seq.map (fun (x : XElement) -> x.Value)
            |> Seq.exactlyOne

        let getReceiversSmtpAddresses =
            document
            /- navigate "email"
            /+ navigate "to"
            /+ navigate "address"
            /+ navigate "smtp-address"
            |> Seq.map (fun (x : XElement) -> x.Value)

        let getReceiversDisplayNames =
            document
            /- navigate "email"
            /+ navigate "to"
            /+ navigate "address"
            /+ navigate "display-name"
            |> Seq.map (fun (x : XElement) -> x.Value)

        let getSubject =
            document
            /- navigate "email"
            /+ navigate "subject"
            |> Seq.map (fun (x : XElement) -> x.Value)
            |> Seq.exactlyOne

        let getBody =
            document
            /- navigate "email"
            /+ navigate "body"
            |> Seq.map (fun (x : XElement) -> x.Value)
            |> Seq.exactlyOne

        {
            From = { SmtpAddress = getSenderSmtpAddress
                     DisplayName = getSenderDisplayName }
            To = Seq.zip getReceiversSmtpAddresses getReceiversDisplayNames
                 |> Seq.map (fun (s, d) -> { SmtpAddress = s; DisplayName = d })
                 |> Seq.toArray
            Subject = getSubject
            Body = getBody
        }
