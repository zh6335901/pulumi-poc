module Acr

open Pulumi.AzureNative.ContainerRegistry
open Pulumi.AzureNative.Resources
open Pulumi.AzureNative.ContainerRegistry.Inputs
open Pulumi

let create (resourceGroup: ResourceGroup) name =
    Registry(
        name,
        RegistryArgs(AdminUserEnabled = true, ResourceGroupName = resourceGroup.Name, Sku = SkuArgs(Name = "Basic"))
    )

let getCredentials (resourceGroup: ResourceGroup) (registry: Registry) =
    let get (name: string) =
        let result =
            ListRegistryCredentials.Invoke(
                ListRegistryCredentialsInvokeArgs(ResourceGroupName = resourceGroup.Name, RegistryName = name)
            )

        result.Apply<(string * string)>(fun r -> Output.CreateSecret((r.Username, r.Passwords[0].Value)))

    registry.Name.Apply<(string * string)>(get)
