module Cluster

open Pulumi
open Pulumi.AzureAD
open Pulumi.AzureNative.ContainerService
open Pulumi.AzureNative.ContainerService.Inputs
open Pulumi.AzureNative.Network
open Pulumi.AzureNative.Network.Inputs
open Pulumi.AzureNative.Resources
open Pulumi.FSharp
open Pulumi.Tls

module private Helpers =
    let createPrivateKey name =
        PrivateKey(name, PrivateKeyArgs(Algorithm = input "RSA", RsaBits = input 4096))
        
    let createApplication name =
        Application(name, ApplicationArgs(DisplayName = input "aks"))
        
    let createServicePrincipal name (app: Application) =
        ServicePrincipal(name, ServicePrincipalArgs(ApplicationId = io app.ApplicationId))
        
    let createServicePrincipalPassword name (servicePrincipal: ServicePrincipal) =
        ServicePrincipalPassword(
            name,
            ServicePrincipalPasswordArgs(
                ServicePrincipalId = io servicePrincipal.Id,
                EndDate = input "2099-01-01T00:00:00Z"
            )
        )
        
    let createVnet name (resourceGroup: ResourceGroup) =
        VirtualNetwork(
            name,
            VirtualNetworkArgs(
                ResourceGroupName = io resourceGroup.Name,
                AddressSpace = input (AddressSpaceArgs(AddressPrefixes = inputList [ input "10.2.0.0/16" ])),
                Subnets = inputList [ input (SubnetArgs(AddressPrefix = input "10.2.1.0/24")) ]
            )
        )
        
    let createCluster
        name
        (privateKey: PrivateKey)
        (app: Application)
        (servicePrincipalPassword: ServicePrincipalPassword)
        (resourceGroup: ResourceGroup)
        kubernetesVersion
        nodeCount
        =
        let nodePoolArgs =
            ManagedClusterAgentPoolProfileArgs(
                Name = input "agentpool",
                OsType = "Linux",
                Mode = "System",
                Type = "VirtualMachineScaleSets",
                Count = input nodeCount,
                MaxPods = input 110,
                VmSize = input "Standard_DS2_v2",
                OsDiskSizeGB = input 30
            )

        let linuxProfileArgs =
            let keyArgs =
                ContainerServiceSshPublicKeyArgs(KeyData = io privateKey.PublicKeyOpenssh)

            ContainerServiceLinuxProfileArgs(
                AdminUsername = input "testuser",
                Ssh = input (ContainerServiceSshConfigurationArgs(PublicKeys = inputList [ input keyArgs ]))
            )

        let servicePrincipalProfileArgs =
            ManagedClusterServicePrincipalProfileArgs(
                ClientId = io app.ApplicationId,
                Secret = io servicePrincipalPassword.Value
            )

        ManagedCluster(
            name,
            ManagedClusterArgs(
                ResourceGroupName = io resourceGroup.Name,
                AgentPoolProfiles = inputList [ input nodePoolArgs ],
                DnsPrefix = input "fsaks",
                LinuxProfile = input linuxProfileArgs,
                ServicePrincipalProfile = input servicePrincipalProfileArgs,
                KubernetesVersion = input kubernetesVersion,
                EnableRBAC = input true,
                NodeResourceGroup = input "MC_azure-fs_my_aks"
            )
        )
        
let create
    (resourceGroup: ResourceGroup)
    privateKeyName
    appName
    spName
    spPasswordName
    clusterName
    kubeVersion
    nodeCount
    =
    let app = Helpers.createApplication appName
    let privateKey = Helpers.createPrivateKey privateKeyName
    let servicePrincipal = Helpers.createServicePrincipal spName app
    let servicePrincipalPassword = Helpers.createServicePrincipalPassword spPasswordName servicePrincipal
    
    Helpers.createCluster
        clusterName
        privateKey
        app
        servicePrincipalPassword
        resourceGroup
        kubeVersion
        nodeCount
        
let getClusterConfig (cluster: ManagedCluster) =
    let userCredentials =
        ListManagedClusterUserCredentials.Invoke(
            ListManagedClusterUserCredentialsInvokeArgs(
                ResourceGroupName = cluster.NodeResourceGroup,
                ResourceName = cluster.Name
            )
        )

    userCredentials.Apply (fun cr ->
        let encoded = cr.Kubeconfigs[0].Value
        let data = System.Convert.FromBase64String(encoded)
        System.Text.Encoding.UTF8.GetString(data))