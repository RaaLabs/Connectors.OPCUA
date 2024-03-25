FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.19 AS build-env
ARG TARGETARCH
WORKDIR /app

VOLUME [ "/app/config" ]

COPY Source/ ./Source/

WORKDIR /app/Source

RUN --mount=type=secret,id=NUGET_GITHUB_PACKAGES_USERNAME \
    --mount=type=secret,id=NUGET_GITHUB_PACKAGES_TOKEN \
    dotnet nuget add source \
    --username $(cat /run/secrets/NUGET_GITHUB_PACKAGES_USERNAME) \
    --password $(cat /run/secrets/NUGET_GITHUB_PACKAGES_TOKEN) \
    --store-password-in-clear-text --name "githubpackagesnuget" "https://nuget.pkg.github.com/RaaLabs/index.json"

RUN dotnet restore --runtime="linux-musl-${TARGETARCH/amd64/x64}"

RUN dotnet publish -c Release -o out --no-restore \
    --runtime alpine-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine3.19 AS final

WORKDIR /app
COPY --from=build-env /app/Source/out ./
COPY --from=build-env /app/Source/config ./config

RUN adduser -Ds /bin/sh moduleuser	
USER moduleuser

ENTRYPOINT ["./RaaLabs.Edge.Connectors.OPCUA"]
