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
