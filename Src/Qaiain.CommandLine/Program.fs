open System
open Grean.Qaiain
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Storage

module AzureQ =
    let dequeue (q : Queue.CloudQueue) =
        match q.GetMessage() with
        | null -> None
        | msg -> Some(msg)

module Mail =

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

open System.Xml.Linq

[<EntryPoint>]
let main argv =
    match queue |> AzureQ.dequeue with
    | Some(msg) ->
        match msg.AsString |> XDocument.Parse |> ToDocumentType with
        | EmailData document ->
            ParseEmailData document |> send
            queue.DeleteMessage msg
        | _ -> ()
    | _ -> ()

    0 // return an integer exit code
