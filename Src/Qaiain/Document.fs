namespace Grean.Qaiain

open System.Xml
open System.Xml.Linq

type DocumentType =
    | Email of XDocument
    | EmailReference of XDocument