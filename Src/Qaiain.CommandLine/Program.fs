open System
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Storage

module AzureQ =
    let dequeue (q : Queue.CloudQueue) =
        match q.GetMessage() with
        | null -> None
        | msg -> Some(msg)

module Mail =

    type Address = {
        SmtpAddress : string
        DisplayName : string
    }

    type EmailData = {
        From : Address
        To : Address array
        Subject : string
        Body : string
    }

    type EmailReference = {
        DataAddress : string
    }

    type EmailMessage =
        | EmailData of EmailData
        | EmailReference of EmailReference
        | Unknown

    open System.Xml

    let private (|EmailData|_|)  (xml : XmlDocument) =
        let ns = XmlNamespaceManager(xml.NameTable)
        ns.AddNamespace("e", "urn:grean:schemas:email:2014")

        let select path = xml.DocumentElement.SelectSingleNode(path, ns).InnerText
        let selectAll path = xml.DocumentElement.SelectNodes(path, ns)

        try
            {
                From = { SmtpAddress = select "e:from/e:smtp-address"
                         DisplayName = select "e:from/e:display-name" }

                To = seq { for n in selectAll "e:to/e:address"  do
                                yield { SmtpAddress = n.FirstChild.InnerText;
                                        DisplayName = n.LastChild.InnerText } }
                     |> Seq.toArray

                Subject = select "e:subject"
                Body = select "e:body"
            }
            |> Some
        with | :? NullReferenceException -> None

    let private (|EmailReference|_|)  (xml : XmlDocument) =
        let ns = XmlNamespaceManager(xml.NameTable)
        ns.AddNamespace("e", "urn:grean:schemas:email:2014")

        let select path = xml.DocumentElement.SelectSingleNode(path, ns).InnerText

        try
            {
                DataAddress = select "e:data-address"
            }
            |> Some
        with | :? NullReferenceException -> None

    let parse input =
        let xml = XmlDocument()

        try
            xml.LoadXml(input)
            match xml with
            | EmailData data -> EmailData data
            | EmailReference ref -> EmailReference ref
            | _ -> Unknown
        with _ -> Unknown

    type SmtpConfiguration = {
        Host : string
        Port : int
        UserName : string
        Password : string
    }

    open System.Net
    open System.Net.Mail

    let private toMailAddress address =
        MailAddress(address.SmtpAddress, address.DisplayName)

    let send config message =
        use client = new SmtpClient(config.Host, config.Port)
        client.Credentials <-
            NetworkCredential(config.UserName, config.Password)

        use smtpMsg = new MailMessage()
        smtpMsg.From <- message.From |> toMailAddress
        message.To |> Array.map toMailAddress |> Array.iter smtpMsg.To.Add
        smtpMsg.IsBodyHtml <- false
        smtpMsg.Subject <- message.Subject
        smtpMsg.Body <- message.Body
        client.Send smtpMsg

let queue =
    let storageAccount =
        CloudConfigurationManager.GetSetting "storageConnectionString"
        |> CloudStorageAccount.Parse

    let name = CloudConfigurationManager.GetSetting "queue-name"
    let q = storageAccount.CreateCloudQueueClient().GetQueueReference(name)
    q.CreateIfNotExists() |> ignore
    q

let send =
    let host = CloudConfigurationManager.GetSetting "email-host"
    let port = CloudConfigurationManager.GetSetting "email-port" |> Int32.Parse
    let userName = CloudConfigurationManager.GetSetting "email-username"
    let password = CloudConfigurationManager.GetSetting "email-password"
    
    let config = {
        Mail.Host = host
        Mail.Port = port
        Mail.UserName = userName
        Mail.Password = password }

    Mail.send config

[<EntryPoint>]
let main argv = 
    match queue |> AzureQ.dequeue with
    | Some(msg) ->
        match msg.AsString |> Mail.parse with
        | Mail.EmailData mail ->
            send mail
            queue.DeleteMessage msg
        | _ -> raise <| InvalidOperationException("Unknown message type.")
    | _ -> ()

    0 // return an integer exit code
