module ResourceGroup

open Pulumi.AzureNative.Resources

let create name = ResourceGroup(name)
