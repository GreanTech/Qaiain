open System
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Storage

module AzureQ =
    let dequeue (q : Queue.CloudQueue) =
        match q.GetMessage() with
        | null -> None
        | msg -> Some(msg)

module Mail =
    open System.Xml.Serialization

    [<CLIMutable>]
    [<XmlRoot("address", Namespace = "http://grean.com/email/2014")>]
    type Address = {
        [<XmlElement("smtp-address")>]
        SmtpAddress : string

        [<XmlElement("display-name")>]
        DisplayName : string
    }

    [<CLIMutable>]
    [<XmlRoot("email", Namespace = "http://grean.com/email/2014")>]
    type EmailData = {
        [<XmlElement("from")>]
        From : Address

        [<XmlArray("to")>]
        [<XmlArrayItem("address")>]
        To : Address array

        [<XmlElement("subject")>]
        Subject : string

        [<XmlElement("body")>]
        Body : string
    }

    open System.IO

    let deserializeMailData xml =
        let serializer = XmlSerializer(typeof<EmailData>)
        use reader = new StringReader(xml)
        (serializer.Deserialize reader) :?> EmailData

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

[<EntryPoint>]
let main argv = 
    let storageAccount =
        CloudConfigurationManager.GetSetting "storageConnectionString"
        |> CloudStorageAccount.Parse

    let q = storageAccount.CreateCloudQueueClient().GetQueueReference("qaiain")
    q.CreateIfNotExists() |> ignore

    let host = CloudConfigurationManager.GetSetting "email-host"
    let port = CloudConfigurationManager.GetSetting "email-port" |> Int32.Parse
    let userName = CloudConfigurationManager.GetSetting "email-username"
    let password = CloudConfigurationManager.GetSetting "email-password"

    let config = {
        Mail.Host = host
        Mail.Port = port
        Mail.UserName = userName
        Mail.Password = password }

    let send = Mail.send config
    
    match q |> AzureQ.dequeue with
    | Some(msg) ->
        msg.AsString |> Mail.deserializeMailData |> send
        q.DeleteMessage msg
    | _ -> ()

    0 // return an integer exit code
