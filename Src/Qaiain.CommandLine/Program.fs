open System
open System.IO
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Storage

[<AutoOpen>]
module Result =
    let Success = Choice1Of2
    let Failure = Choice2Of2

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

    type Attachment = {
        Content : byte array
        MimeType : string
        Name : string }

    type EmailData = {
        From : Address
        To : Address array
        Subject : string
        Body : string
        Attachments : Attachment list
    }

    type EmailReference = {
        DataAddress : string
    }

    type EmailMessage =
        | EmailData of EmailData
        | EmailReference of EmailReference
        | Unknown

    type ErrorMessage =
        | UnknownMessageType
        | InvalidMessageReference

    open System.Xml

    let private (|EmailData|_|) (xml : XmlDocument) =
        let ns = XmlNamespaceManager(xml.NameTable)
        ns.AddNamespace("e", "urn:grean:schemas:email:2014")

        let document = xml.DocumentElement

        let select path (x : XmlNode) = x.SelectSingleNode(path, ns).InnerText
        let selectAll path = document.SelectNodes(path, ns)

        try
            {
                From = { SmtpAddress = document |> select "e:from/e:smtp-address"
                         DisplayName = document |> select "e:from/e:display-name" }

                To = [| for node in selectAll "e:to/e:address" do
                            yield { SmtpAddress = node |> select "e:smtp-address"
                                    DisplayName = node |> select "e:display-name" } |]

                Subject = document |> select "e:subject"
                Body = document |> select "e:body"

                Attachments =
                    [ for node in selectAll "e:attachments/e:attachment" do
                          yield { Content = node |> select "e:content"
                                                 |> Convert.FromBase64String
                                  MimeType = node |> select "e:mime-type"
                                  Name = node |> select "e:name" } ]
            }
            |> Some
        with | :? NullReferenceException -> None

    let private (|EmailReference|_|) (xml : XmlDocument) =
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
    open FSharpx.Choice

    let private toMailAddress address =
        MailAddress(address.SmtpAddress, address.DisplayName)

    let private toAttachment attachment =
        new Attachment(
            new MemoryStream(attachment.Content),
            attachment.Name,
            attachment.MimeType)

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
        message.Attachments |> List.map toAttachment
                            |> List.iter smtpMsg.Attachments.Add

        client.Send smtpMsg

    let rec handle (getMessage) (deleteMessage) (sendEmail) msg =
        match msg |> parse with
        | EmailMessage.EmailData mail ->
            mail |> sendEmail |> Success
        | EmailMessage.EmailReference ref ->
            choose {
                let! message = ref.DataAddress |> getMessage
                do! message |> handle getMessage deleteMessage sendEmail
                do! ref.DataAddress |> deleteMessage }
        | _ -> UnknownMessageType |> Failure

let queue =
    let storageAccount =
        CloudConfigurationManager.GetSetting "storageConnectionString"
        |> CloudStorageAccount.Parse

    let name = CloudConfigurationManager.GetSetting "queue-name"
    let q = storageAccount.CreateCloudQueueClient().GetQueueReference(name)
    q.CreateIfNotExists() |> ignore
    q

let private blob =
    let storageAccount =
        CloudConfigurationManager.GetSetting "storageConnectionString"
        |> CloudStorageAccount.Parse

    let name = CloudConfigurationManager.GetSetting "messages-container"
    let blob = storageAccount.CreateCloudBlobClient().GetContainerReference(name)
    blob.CreateIfNotExists() |> ignore
    blob

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

// Select command-line return constants taken from 
// http://msdn.microsoft.com/en-us/library/windows/desktop/ms681382.aspx
[<Literal>]
let ERROR_SUCCESS = 0

[<Literal>]
let ERROR_INVALID_DATA = 13

// End of constants -------

open FSharpx.Validation

[<EntryPoint>]
let main argv = 
    let getMessage blobName =
        try blob.GetBlockBlobReference(blobName).DownloadText() |> Choice1Of2
        with | :? StorageException -> Mail.ErrorMessage.InvalidMessageReference |> Choice2Of2

    let deleteMessage blobName =
        try blob.GetBlockBlobReference(blobName).Delete() |> Choice1Of2
        with | :? StorageException -> () |> Choice1Of2

    let toString errorMessage =
        match errorMessage with
        | Mail.ErrorMessage.UnknownMessageType -> "Unknown message type."
        | Mail.ErrorMessage.InvalidMessageReference -> "Invalid message reference, possibly due to a non-existing block blob."

    let handle msg = Mail.handle getMessage deleteMessage send msg

    match queue |> AzureQ.dequeue with
    | Some(msg) ->
        match msg.AsString |> handle with
        | Success _ ->
            queue.DeleteMessage msg
            ERROR_SUCCESS
        | Failure f ->
            f |> toString |> Console.Error.WriteLine
            ERROR_INVALID_DATA
    | _ -> ERROR_SUCCESS
