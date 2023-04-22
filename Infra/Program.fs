module Program

open Pulumi.FSharp

let infra () =
    let resourceGroup = ResourceGroup.create "my-product-infra"
    let registry = Acr.create resourceGroup "containerRegistry"
    let registryCred = Acr.getCredentials resourceGroup registry

    let cluster =
        Aks.create resourceGroup "aksPrivateKey" "aksApp" "aksAppSp" "aksAppSpPassword" "aks" "1.26.0" 3

    Aks.assignAcrPullRole cluster registry |> ignore

    let clusterConfig = Aks.getClusterConfig resourceGroup cluster

    dict [ ("resourceGroupName", resourceGroup.Name :> obj)
           ("registryLoginServer", registry.LoginServer :> obj)
           ("registryUsername", registryCred.Apply(fun c -> fst c) :> obj)
           ("registryPassword", registryCred.Apply(fun c -> snd c) :> obj)
           ("kubeClusterConfig", clusterConfig :> obj) ]

[<EntryPoint>]
let main _ = Deployment.run infra
