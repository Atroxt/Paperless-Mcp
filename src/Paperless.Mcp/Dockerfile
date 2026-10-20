# Build-Kontext ist das Repository-Wurzelverzeichnis, nicht dieser Ordner:
# Directory.Build.props, Directory.Packages.props und NuGet.config liegen dort und
# werden fuer Restore und Build gebraucht.
#
#   docker build -f src/Paperless.Mcp/Dockerfile -t paperless-mcp .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=
WORKDIR /src

# Erst nur das, was den Restore bestimmt: so bleibt der Layer-Cache nutzbar,
# solange sich keine Paketversion aendert.
COPY NuGet.config Directory.Build.props Directory.Packages.props ./
COPY src/Paperless.Mcp/Paperless.Mcp.csproj src/Paperless.Mcp/
RUN dotnet restore src/Paperless.Mcp/Paperless.Mcp.csproj

COPY src/Paperless.Mcp/ src/Paperless.Mcp/
RUN dotnet publish src/Paperless.Mcp/Paperless.Mcp.csproj \
      -c Release \
      -o /app/publish \
      --no-restore \
      ${VERSION:+-p:Version=${VERSION}}

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ARG VERSION=
WORKDIR /app

LABEL org.opencontainers.image.title="Paperless.Mcp"
LABEL org.opencontainers.image.description="Model Context Protocol server for Paperless-ngx"
LABEL org.opencontainers.image.version="${VERSION}"

COPY --from=build /app/publish .

# Der Server liest den Port aus MCP_PORT (siehe Program.cs) und bindet selbst auf
# 0.0.0.0, deshalb wird ASPNETCORE_URLS hier bewusst nicht gesetzt.
ENV MCP_PORT=5005
EXPOSE 5005

# Nicht als root laufen; APP_UID bringen die offiziellen .NET-Images mit.
USER $APP_UID

ENTRYPOINT ["dotnet", "Paperless.Mcp.dll"]
