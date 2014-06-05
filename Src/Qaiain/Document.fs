namespace Grean.Qaiain

open System.Xml
open System.Xml.Linq

type DocumentType =
    | Email of XDocument
    | EmailReference of XDocument
    | Unknown

[<AutoOpen>]
module Document =

    let ToDocumentType message =
        let document = message |> XDocument.Parse
        match document.Root.Name.Namespace.NamespaceName with
        | _ -> Unknown
