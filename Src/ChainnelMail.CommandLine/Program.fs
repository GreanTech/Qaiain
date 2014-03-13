open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Storage

module AzureQ =
    let dequeue (q : Queue.CloudQueue) =
        match q.GetMessage() with
        | null -> None
        | msg -> Some(msg)

[<EntryPoint>]
let main argv = 
    let storageAccount =
        CloudConfigurationManager.GetSetting "storageConnectionString"
        |> CloudStorageAccount.Parse

    let q = storageAccount.CreateCloudQueueClient().GetQueueReference("qaiain")
    q.CreateIfNotExists() |> ignore
    
    let msg = q |> AzureQ.dequeue
    0 // return an integer exit code
