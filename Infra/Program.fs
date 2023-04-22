module Program

open Pulumi.FSharp

let infra () =
    let resourceGroup = ResourceGroup.create "my-product-infra"
    let registry = ContainerRegistry.create resourceGroup "container-registry"
    let cluster =
        Cluster.create
            resourceGroup
            "aks-private-key"
            "aks-app"
            "aks-app"
            "aks-app-sp"
            "aks-app-sp-password"
            "1.26.0"
            3
    
    // Export the primary key for the storage account
    dict [
        ("resourceGroupName", resourceGroup.Name :> obj)
        ("registryName", registry.Name :> obj)
        ("kebeClusterConfig", (Cluster.getClusterConfig cluster) :> obj)
    ]

[<EntryPoint>]
let main _ =
  Deployment.run infra
