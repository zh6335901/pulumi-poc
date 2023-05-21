module Image

open Pulumi.Docker
open Pulumi.Docker.Inputs
open Pulumi.FSharp

type DockerRegistry =
    { Server: string
      Username: string
      Password: string }

let build registry =
    let image =
        Image(
            "MyProductImage",
            ImageArgs(
                ImageName = input "pulumi-poc/MyProduct",
                Build =
                    input (
                        DockerBuildArgs(
                            Context = "./Source/MyProduct.Api",
                            Dockerfile = "./Source/MyProduct.Api/Dockerfile"
                        )
                    ),
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
