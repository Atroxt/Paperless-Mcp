# Authentifizierung und Konfiguration

[Zurück zur Übersicht](../README.md)

## Authentifizierung

**Woher das API-Token kommt, hängt vom Transport ab — und das ist der wichtigste Punkt
beim öffentlichen Hosten.**

| Transport | Token | Begründung |
| --- | --- | --- |
| stdio | aus der Konfiguration (`PAPERLESS_API_TOKEN`, User Secrets) | Der Prozess läuft auf dem Rechner des Benutzers, sein Token ist sein eigenes |
| HTTP | **ausschließlich** aus dem `Authorization`-Header der MCP-Anfrage | Jeder Aufrufer nutzt sein eigenes Paperless-Konto |

Im HTTP-Transport gibt es **keinen Rückfall** auf ein konfiguriertes Token. Gäbe es ihn,
würde ein öffentlich erreichbarer Server jedem Aufrufer ohne eigenes Token die Dokumente
des Betreibers zeigen. Der Schutz greift an zwei Stellen:

1. Middleware in `Program.cs` weist jede Anfrage an `/mcp` ohne verwertbaren
   `Authorization`-Header mit **401** und `WWW-Authenticate: Bearer` ab — bevor ein Tool
   überhaupt läuft.
2. `PaperlessAuthHandler` setzt ohne Request-Token gar keinen Header, sodass Paperless
   selbst mit 401 antwortet. Der Schutz hängt damit nicht an einer einzigen Stelle.

Akzeptiert werden `Authorization: Bearer <token>` und `Authorization: Token <token>`;
nach Paperless geht immer `Token <token>` hinaus.

```bash
curl -X POST http://localhost:5005/mcp \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json, text/event-stream" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"paperless_tags_list","arguments":{}}}'
```

Ein abgelehntes Token meldet der Server als `Paperless rejected the API token.` — ein
Fehler, den der Aufrufer beheben kann, kein Infrastrukturproblem.

`PAPERLESS_API_TOKEN` wird im HTTP-Transport also **nicht** verwendet. Für den Betrieb
hinter einem Reverse Proxy gilt zusätzlich: TLS verwenden, sonst geht das Token im Klartext
über die Leitung.

## Konfiguration

Umgebungsvariablen haben Vorrang vor `appsettings.json`.

| Variable | `appsettings.json` | Default | Bedeutung |
| --- | --- | --- | --- |
| `PAPERLESS_BASE_URL` oder `PAPERLESS_URL` | `Paperless:BaseUrl` | — | URL der Paperless-Instanz, **erforderlich** |
| `PAPERLESS_API_TOKEN` oder `PAPERLESS_TOKEN` | `Paperless:ApiToken` | — | API-Token, **nur im stdio-Transport** (siehe Authentifizierung) |
| `HTTP_TIMEOUT_SECONDS` | `Paperless:HttpTimeoutSeconds` | `30` | Timeout für Anfragen an Paperless |
| `MAX_PAGE_SIZE` | `Paperless:MaxPageSize` | `100` | Obergrenze für Treffer je Seite bei der Dokumentsuche |
| `MCP_PORT` | `Mcp:Port` | `5005` | Port des HTTP-Transports |
| `MCP_PUBLIC_URL` | `Mcp:PublicUrl` | Adresse aus der Anfrage | Basis-URL für Download-Links, nötig hinter einem Reverse Proxy |
| `DOWNLOAD_LINK_LIFETIME_SECONDS` | `Mcp:DownloadLinkLifetimeSeconds` | `300` | Gültigkeitsdauer eines Download-Links |

Ohne `PAPERLESS_BASE_URL` bricht der Server beim ersten Zugriff auf die Paperless-API mit
einer klaren Meldung ab. Ein **fehlendes Token ist dagegen kein Startfehler**: Paperless
antwortet dann mit 401, und genau das meldet `paperless_ping` — sonst könnte der
Health-Check das häufigste Problem nicht diagnostizieren.

### Token lokal ablegen

Damit das Token nicht im Repository landet:

```bash
dotnet user-secrets --project src/Paperless.Mcp set "Paperless:ApiToken" "<token>"
dotnet user-secrets --project src/Paperless.Mcp set "Paperless:BaseUrl" "http://localhost:8000"
```

Beide Host-Builder lesen User Secrets im Development-Modus automatisch mit.
Umgebungsvariablen haben weiterhin Vorrang.

Ein Token erzeugt man in Paperless-ngx im Benutzerprofil (Zahnrad → **My Profile** →
*API Auth Token*) oder über die Django-Administration.
