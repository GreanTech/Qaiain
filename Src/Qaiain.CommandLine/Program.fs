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

    type EmailMessage =
        | EmailData of EmailData
        | Unknown

    open System.Xml

    let private toEmailData (xml : XmlDocument) =
        let ns = XmlNamespaceManager(xml.NameTable)
        ns.AddNamespace("e", "urn:grean:schemas:email:2014")

        let select xp = xml.DocumentElement.SelectSingleNode(xp, ns).InnerText
        let selectAll xp = xml.DocumentElement.SelectNodes(xp, ns)

        try
            {
                From = { SmtpAddress = select <| "e:from/e:smtp-address"
                         DisplayName = select <| "e:from/e:display-name" }

                To = seq { for n in selectAll <| "e:to/e:address"  do
                                yield { SmtpAddress = n.FirstChild.InnerText;
                                        DisplayName = n.LastChild.InnerText } }
                        |> Seq.toArray

                Subject = select <| "e:subject"
                Body = select <| "e:body"
            }
            |> EmailData
        with | :? NullReferenceException -> Unknown

    let Parse input =
        let xml = XmlDocument()
        xml.LoadXml(input)
        match xml.DocumentElement.Name with
        | "email" -> xml |> toEmailData
        | _ -> Unknown

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

    let q = storageAccount.CreateCloudQueueClient().GetQueueReference("qaiain")
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
        match msg.AsString |> Mail.Parse with
        | Mail.EmailData mail ->
            send mail
            queue.DeleteMessage msg
        | _ -> raise <| InvalidOperationException("Unknown message type.")
    | _ -> ()

    0 // return an integer exit code
