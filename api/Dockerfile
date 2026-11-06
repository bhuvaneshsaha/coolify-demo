# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine3.24 AS build
WORKDIR /src

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    NUGET_XMLDOC_MODE=skip

COPY CoolifyDemo.Api.csproj .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore CoolifyDemo.Api.csproj

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish CoolifyDemo.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0.9-alpine3.24 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --from=build --link --chown=1654:1654 /app/publish/ ./

USER app
EXPOSE 8080

HEALTHCHECK --interval=5s --timeout=3s --start-period=10s --retries=5 \
    CMD wget -qO- http://127.0.0.1:8080/health | grep -q ok

LABEL org.opencontainers.image.source="https://github.com/bhuvaneshsaha/coolify-demo" \
      org.opencontainers.image.title="coolify-demo-api"

ENTRYPOINT ["dotnet", "CoolifyDemo.Api.dll"]
