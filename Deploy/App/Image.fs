module Image

open Pulumi
open Pulumi.Docker
open Pulumi.Docker.Inputs
open Pulumi.FSharp
open System

type DockerRegistry =
    { Server: Input<string>
      Username: Input<string>
      Password: Input<string> }

let create registry =
    let version = $"""v1.0-{DateTime.Now.ToString("yyyyMMddHHmmss")}"""

    let image =
        Image(
            "MyProductApi",
            ImageArgs(
                ImageName = input $"pulumi-poc/MyProductApi:{version}",
                Build =
                    input (DockerBuildArgs(Context = "../../.", Dockerfile = "../../Source/MyProduct.Api/Dockerfile")),
                Registry =
                    input (
                        RegistryArgs(
                            Server = registry.Server,
                            Username = registry.Username,
                            Password = registry.Password
                        )
                    )
            )
        )

    image
