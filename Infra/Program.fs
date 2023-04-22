module Program

open Pulumi.FSharp

let infra () =
    let resourceGroup = ResourceGroup.create "my-product-infra"
    let registry = Acr.create resourceGroup "container-registry"
    let registryCred = Acr.getCredentials resourceGroup registry

    let cluster =
        Aks.create resourceGroup "aks-private-key" "aks-app" "aks-app" "aks-app-sp" "aks-app-sp-password" "1.26.0" 3

    Aks.assignAcrPullRole cluster registry |> ignore

    let clusterConfig = Aks.getClusterConfig cluster

    dict [ ("resourceGroupName", resourceGroup.Name :> obj)
           ("registryLoginServer", registry.LoginServer :> obj)
           ("registryUsername", registryCred.Apply(fun c -> fst c) :> obj)
           ("registryPassword", registryCred.Apply(fun c -> snd c) :> obj)
           ("kubeClusterConfig", clusterConfig :> obj) ]

[<EntryPoint>]
let main _ = Deployment.run infra
