namespace Grean.Qaiain

open System.Xml
open System.Xml.Linq

type DocumentType =
    | Email of XDocument
    | EmailReference of XDocument
    | Unknown

[<AutoOpen>]
module Document =
    let ToDocumentType (document : XDocument) =
        match document.Root.Name.Namespace.NamespaceName with
        | "urn:grean:schemas:email:2014" -> Email document
        | "urn:grean:schemas:email-reference:2014" -> EmailReference document
        | _ -> Unknown

    let ParseEmailReference (document : XDocument) =
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
