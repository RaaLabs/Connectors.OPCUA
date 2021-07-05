# Edge.ModuleTemplate
[![.NET 5.0](https://github.com/RaaLabs/Edge.ModuleTemplate/actions/workflows/dotnet.yml/badge.svg)](https://github.com/RaaLabs/Edge.ModuleTemplate/actions/workflows/dotnet.yml)

The template can be used to initialize new Raa Labs Edge modules. To use it in new repos, select
it as the "Template Repository" in the dialog that opens when creating a new GitHub repo. All files will be present in the newly created repo.

## Getting started
It contains a .sln file for use with Visual Studio and a .csproj file including the necessary Nuget references. The template itself is a working Edge module and it comes with a Dockerfile to create images. Run the following commands to verify it runs:

````bash
dotnet restore
dotnet build

docker build -f Source/Dockerfile -t moduletemplate:test-tag .
docker run moduletemplate:test-tag
````

You should see initial logs that the handlers are started.
