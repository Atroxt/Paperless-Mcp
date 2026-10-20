# Paperless.Mcp mit Docker Compose

Dieser Ordner enthält alles, was zum Betrieb nötig ist. Du brauchst eine laufende
Paperless-ngx-Instanz und ein API-Token dafür.

```bash
cp .env.example .env     # PAPERLESS_BASE_URL eintragen
docker compose up -d
```

Danach liegt der MCP-Endpunkt unter `http://localhost:5005/mcp`.

## Das Token gehört nicht hierher

In `.env` steht **kein** API-Token, und das ist der wichtigste Punkt an diesem Setup:
im HTTP-Betrieb bringt **jede Anfrage ihr eigenes Token** im `Authorization`-Header mit.
Der Server fällt bewusst **nicht** auf ein konfiguriertes Token zurück — sonst könnte
jeder, der den Port erreicht, die Dokumente des Betreibers lesen. Ein trotzdem gesetztes
`PAPERLESS_API_TOKEN` wird schlicht ignoriert.

Eine Anfrage ohne verwertbaren Header bekommt `401`:

```bash
curl -X POST http://localhost:5005/mcp \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Authorization: Token <dein-paperless-token>" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"paperless_capabilities","arguments":{}}}'
```

`paperless_capabilities` ist der erste Aufruf, der sich lohnt: er sagt, welche
Paperless-Version dahinter hängt, ob das Token trägt und welche Grenzen gelten.

## Wie der Container an Paperless kommt

Das ist die häufigste Stolperstelle. `localhost` im Container ist der Container selbst,
nicht dein Rechner.

| Wo läuft Paperless? | `PAPERLESS_BASE_URL` |
| --- | --- |
| Auf demselben Rechner, ohne Compose | `http://host.docker.internal:8000` |
| In einem eigenen Compose-Projekt | `http://<service-name>:8000`, dazu das Netz verbinden |
| Auf einem anderen Server | dessen URL, etwa `https://paperless.example.org` |

Für den mittleren Fall: Netzname mit `docker network ls` nachsehen, unten in
`compose.yaml` den auskommentierten `networks`-Block ausfüllen und beim Service
`networks: [paperless]` ergänzen.

## Hinter einem Reverse Proxy

Dann muss `MCP_PUBLIC_URL` gesetzt werden. Ohne diesen Wert baut der Server die
temporären Download-Links aus der Adresse der eingehenden Anfrage zusammen — und die
zeigt hinter einem Proxy ins interne Netz, sodass der Link beim Empfänger nicht
funktioniert.

## Eigenes Image bauen

Das veröffentlichte Image liegt unter `ghcr.io/atroxt/paperless-mcp`. Selbst bauen geht
aus der Repository-Wurzel, nicht aus diesem Ordner — der Build braucht
`Directory.Build.props`, `Directory.Packages.props` und `NuGet.config`:

```bash
docker build -f src/Paperless.Mcp/Dockerfile -t paperless-mcp .
```

## Lokal statt über HTTP

Für Claude Desktop auf dem eigenen Rechner ist Compose der falsche Weg: dort läuft der
Server über stdio, bekommt sein Token aus der Konfiguration und kann zusätzlich Dateien
auf Pfade schreiben. Siehe [../README.md](../README.md), Abschnitt „Einbindung in
Claude Desktop".
