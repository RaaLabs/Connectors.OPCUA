FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine3.17 AS build-env
WORKDIR /app

VOLUME [ "/app/config" ]

COPY Source/ ./Source/

WORKDIR /app/Source

RUN dotnet restore --runtime alpine-x64

RUN dotnet publish -c Release -o out --no-restore \
    --runtime alpine-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine3.17 AS final

WORKDIR /app
COPY --from=build-env /app/Source/out ./
COPY --from=build-env /app/Source/config ./config

RUN adduser -Ds /bin/sh moduleuser	
USER moduleuser

ENTRYPOINT ["./RaaLabs.Edge.Connectors.OPCUA"]