module Program

open Pulumi
open Pulumi.FSharp
open Image

let ioString (o: Output<obj>) = io (o.Apply(fun o -> string o))

let getImageRegistry (stackRef: StackReference) =
    let registryLoginServer = stackRef.GetOutput("registryLoginServer")
    let registryUsername = stackRef.GetOutput("registryUsername")
    let registryPassword = stackRef.GetOutput("registryPassword")

    { Server = ioString registryLoginServer
      Username = ioString registryUsername
      Password = ioString registryPassword }

let infra () =
    let org = Deployment.Instance.OrganizationName
    let stack = Deployment.Instance.StackName
    let infraStackRef = StackReference($"{org}/Infra/{stack}")

    let imageRegistry = getImageRegistry infraStackRef
    let image = Image.create imageRegistry

    let kubeConfig = ioString (infraStackRef.GetOutput("kubeClusterConfig"))
    let serviceIP = Cluster.apply (io image.ImageName) kubeConfig

    dict [ ("serviceIP", serviceIP :> obj) ]

[<EntryPoint>]
let main _ = Deployment.run infra
