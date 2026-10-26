# Paperless.Mcp

[![CI](https://github.com/Atroxt/Paperless-Mcp/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Atroxt/Paperless-Mcp/actions/workflows/ci.yml)
[![Latest Release](https://img.shields.io/github/v/release/Atroxt/Paperless-Mcp?sort=semver)](https://github.com/Atroxt/Paperless-Mcp/releases/latest)
[![Docker Image](https://img.shields.io/badge/ghcr.io-atroxt%2Fpaperless--mcp-blue?logo=docker)](https://github.com/Atroxt/Paperless-Mcp/pkgs/container/paperless-mcp)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

MCP-Server für [Paperless-ngx](https://docs.paperless-ngx.com/). Er macht die
Paperless-REST-API als Tools für KI-Assistenten verfügbar — lokal über stdio
(Claude Desktop) und über HTTP für entfernte Nutzung.

## Features

- **Über 40 Tools** für Dokumente, Notizen, Tags, Korrespondenten, Dokumenttypen, Ablagepfade, Felddefinitionen, Aufträge und Statistiken
- **Zwei Transporte:** stdio für Claude Desktop, Streamable HTTP für den gehosteten Betrieb
- **Sicher löschen:** destruktive Vorgänge laufen per Default als Trockenlauf und nennen die Tragweite
- **Hochladen und warten** in einem Aufruf, inklusive Auftragsverfolgung
- **Einmal-Links** zum Herunterladen im HTTP-Betrieb
- **Token pro Anfrage** im HTTP-Transport — kein geteiltes Betreiber-Token
- **Docker-Image** auf ghcr.io

## Dokumentation

| Thema | Inhalt |
| --- | --- |
| [Tools](docs/tools.md) | Alle Tools, Trockenlauf, Upload, Download-Links, Sammeloperationen |
| [Authentifizierung und Konfiguration](docs/configuration.md) | Token je Transport, Umgebungsvariablen |
| [Docker](docker/README.md) | Betrieb mit Docker Compose und hinter einem Reverse Proxy |
| [Entwicklung](docs/development.md) | Build, Tests, Integrationstests, Versionierung |
| [Architektur](docs/architecture.md) | Schichten, Exceptions |

## Voraussetzungen

- .NET SDK 10.0
- Eine erreichbare Paperless-ngx-Instanz

## Schnellstart

```bash
git clone https://github.com/Atroxt/Paperless-Mcp.git
cd Paperless-Mcp
dotnet build Paperless-Mcp.slnx -c Release
```

**stdio (Claude Desktop):**

```bash
dotnet run --project src/Paperless.Mcp -- --stdio
```

**HTTP:**

```bash
dotnet run --project src/Paperless.Mcp
# lauscht auf http://localhost:5005/mcp
```

Kurzer Funktionstest des HTTP-Transports:

```bash
curl -X POST http://localhost:5005/mcp -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'
```

### Einbindung in einen MCP-Client

**Gehostet über HTTP, direkt** — der einfachste Weg für Clients, die Streamable HTTP
selbst sprechen, etwa Claude Code (`.mcp.json`), Cursor oder VS Code (dort heißt der
oberste Schlüssel `servers` statt `mcpServers`). Der Server läuft woanders, etwa aus
[docker/](docker/), und das Token gehört in den Header, nicht auf den Server:

```json
{
  "mcpServers": {
    "paperless": {
      "type": "http",
      "url": "https://mcp.example.org/mcp",
      "headers": {
        "Authorization": "Bearer <token>"
      }
    }
  }
}
```

`Token <token>` geht ebenso — beides nimmt der Server entgegen.

**Claude Desktop** liest in `claude_desktop_config.json` nur Server, die es selbst
startet. Dort gibt es zwei Wege, je nachdem wo der Server läuft.

**Lokal über stdio**
der Konfiguration und darf zusätzlich Dateien auf Pfade schreiben. In
`claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "paperless": {
      "command": "dotnet",
      "args": ["run", "--project", "<Pfad>/src/Paperless.Mcp", "--", "--stdio"],
      "env": {
        "PAPERLESS_BASE_URL": "https://paperless.example.com",
        "PAPERLESS_API_TOKEN": "<token>"
      }
    }
  }
}
```

Im stdio-Modus transportiert stdout das Protokoll selbst, deshalb gehen alle Logzeilen
nach stderr.

**Gehostet über HTTP mit `mcp-remote`** — die direkte Form oben kennt Claude Desktop
nicht, deshalb übersetzt [`mcp-remote`](https://github.com/geelen/mcp-remote) zwischen
stdio und HTTP:

```json
{
  "mcpServers": {
    "paperless": {
      "command": "npx",
      "args": [
        "mcp-remote",
        "https://mcp.example.org/mcp",
        "--transport",
        "http-only",
        "--header",
        "Authorization:${PAPERLESS_AUTH}"
      ],
      "env": {
        "PAPERLESS_AUTH": "Token <token>"
      }
    }
  }
}
```

Drei Dinge sind daran wichtig:

- **Der Doppelpunkt hat keinen Abstand**, und der Wert steht in einer
  Umgebungsvariablen. Claude Desktop unter Windows maskiert Leerzeichen in `args` nicht,
  wenn es `npx` aufruft — ein `"Authorization: Token abc"` käme zerlegt an. In
  Umgebungsvariablen sind Leerzeichen unproblematisch.
- **`Token` oder `Bearer`**, beides nimmt der Server entgegen. `Token` ist das Schema,
  das Paperless selbst erwartet.
- **`--transport http-only`** ist nicht zwingend, aber ehrlicher: ohne diese Angabe
  fällt `mcp-remote` bei einem Fehler auf SSE zurück und verdeckt damit eine falsche
  Adresse.

Gehostet fehlen die beiden Pfad-Werkzeuge — sie würden auf dem Server lesen und
schreiben, nicht bei dir. Dafür gibt es `paperless_documents_download_link`.
