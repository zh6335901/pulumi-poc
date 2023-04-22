module ContainerRegistry

open Pulumi.AzureNative.ContainerRegistry
open Pulumi.AzureNative.Resources
open Pulumi.AzureNative.ContainerRegistry.Inputs

let create (resourceGroup: ResourceGroup) name =
    Registry(
        name,
        RegistryArgs(
            AdminUserEnabled = true,
            ResourceGroupName = resourceGroup.Name,
            Sku = SkuArgs(Name = "Basic")
        )
    )