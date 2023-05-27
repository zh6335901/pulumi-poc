module Cluster

open Pulumi
open Pulumi.FSharp
open Pulumi.Kubernetes.Apps.V1
open Pulumi.Kubernetes.Core.V1
open Pulumi.Kubernetes.Types.Inputs.Apps.V1
open Pulumi.Kubernetes.Types.Inputs.Meta.V1
open Pulumi.Kubernetes.Types.Inputs.Core.V1
open Pulumi.Kubernetes

let apply imageName kubeConfig =
    let labels = inputMap [ ("app", input "MyProductApi") ]

    let containers =
        ContainerArgs(
            Name = "MyProductApi",
            Image = imageName,
            Resources =
                ResourceRequirementsArgs(
                    Requests =
                        inputMap [ ("cpu", input "100m")
                                   ("memory", input "100Mi") ]
                ),
            Ports = inputList [ input (ContainerPortArgs(ContainerPortValue = input 80)) ]
        )

    let provider = Provider("MyProduct", ProviderArgs(KubeConfig = kubeConfig))

    let deployment =
        Deployment(
            "MyProductApi",
            DeploymentArgs(
                Spec =
                    DeploymentSpecArgs(
                        Selector = LabelSelectorArgs(MatchLabels = labels),
                        Replicas = 3,
                        Template =
                            PodTemplateSpecArgs(
                                Metadata = ObjectMetaArgs(Labels = labels),
                                Spec = PodSpecArgs(Containers = containers)
                            )
                    )
            ),
            CustomResourceOptions(Provider = provider)
        )

    let deploymentLabels = deployment.Metadata.Apply(fun m -> m.Labels)

    let deploymentTemplateLabels =
        deployment.Spec.Apply(fun sp -> sp.Template.Metadata.Labels)

    let service =
        Service(
            "MyProductApi",
            ServiceArgs(
                Metadata = ObjectMetaArgs(Name = "MyProductApi", Labels = deploymentLabels),
                Spec = ServiceSpecArgs(Type = inputUnion1Of2 "ClusterIP", Selector = deploymentTemplateLabels)
            )
        )

    service.Spec.Apply(fun sp -> sp.ClusterIP)
