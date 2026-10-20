# Tools

[Zurück zur Übersicht](../README.md)

Der Server stellt **42 Tools** im HTTP-Transport bereit und **43** über stdio — die
beiden Pfad-Werkzeuge gibt es nur dort, den Einmal-Link nur im HTTP-Transport.

| Tool | Zweck |
| --- | --- |
| `paperless_ping` | Erreichbarkeit und API-Token prüfen |
| `paperless_capabilities` | Was dieser Server kann: Transport, Instanz, Grenzen |
| `paperless_statistics` | Überblick: Dokumente gesamt, im Posteingang, Anzahl Tags/Korrespondenten/Dokumenttypen/Ablagepfade, Dateitypen |
| `paperless_search` | Alles auf einmal finden: Dokumente, Tags, Korrespondenten, Dokumenttypen, Ablagepfade, Felddefinitionen |
| `paperless_tags_list` | Tags seitenweise auflisten |
| `paperless_tags_get` | Einzelnen Tag über seine Id holen |
| `paperless_tags_create` | Tag anlegen |
| `paperless_tags_update` | Tag ersetzen |
| `paperless_tags_delete` | Tag löschen (Trockenlauf per Default) |
| `paperless_correspondents_list` | Korrespondenten seitenweise auflisten |
| `paperless_correspondents_get` | Einzelnen Korrespondenten über seine Id holen |
| `paperless_correspondents_create` | Korrespondenten anlegen |
| `paperless_correspondents_update` | Korrespondenten ersetzen |
| `paperless_correspondents_delete` | Korrespondenten löschen (Trockenlauf per Default) |
| `paperless_document_types_list` | Dokumenttypen seitenweise auflisten |
| `paperless_document_types_get` | Einzelnen Dokumenttyp über seine Id holen |
| `paperless_document_types_create` | Dokumenttyp anlegen |
| `paperless_document_types_update` | Dokumenttyp ersetzen |
| `paperless_document_types_delete` | Dokumenttyp löschen (Trockenlauf per Default) |
| `paperless_storage_paths_list` | Ablagepfade seitenweise auflisten |
| `paperless_storage_paths_get` | Einzelnen Ablagepfad über seine Id holen |
| `paperless_storage_paths_create` | Ablagepfad anlegen |
| `paperless_storage_paths_update` | Ablagepfad ersetzen |
| `paperless_storage_paths_delete` | Ablagepfad löschen (Trockenlauf per Default) |
| `paperless_custom_fields_list` | Feld­definitionen seitenweise auflisten |
| `paperless_custom_fields_get` | Einzelne Felddefinition über ihre Id holen |
| `paperless_custom_fields_create` | Felddefinition anlegen |
| `paperless_custom_fields_update` | Felddefinition ersetzen |
| `paperless_custom_fields_delete` | Felddefinition löschen (Trockenlauf per Default) |
| `paperless_documents_search` | Dokumente suchen und filtern |
| `paperless_documents_get` | Einzelnes Dokument samt Text holen |
| `paperless_documents_update` | Titel, Korrespondent, Typ, Ablagepfad, Tags oder Datum ändern |
| `paperless_documents_set_custom_fields` | Feldwerte eines Dokuments setzen |
| `paperless_documents_upload` | Datei hochladen (Base64) |
| `paperless_documents_upload_and_wait` | Hochladen, auf die Verarbeitung warten und das Dokument zurückgeben |
| `paperless_documents_thumbnail` | Vorschaubild eines Dokuments |
| `paperless_documents_download_link` | Einmal-Link zum Herunterladen einer Datei — **nur HTTP** |
| `paperless_documents_delete` | Dokument löschen (Trockenlauf per Default) |
| `paperless_documents_reprocess` | Dokument neu einlesen (Trockenlauf per Default) |
| `paperless_documents_bulk_update` | Zuordnungen und Tags vieler Dokumente auf einmal ändern (Trockenlauf per Default) |
| `paperless_documents_bulk_delete` | Viele Dokumente auf einmal löschen (Trockenlauf per Default) |
| `paperless_tasks_get` | Verarbeitungsauftrag nachschlagen |
| `paperless_documents_upload_from_path` | Datei vom Pfad hochladen — **nur stdio** |
| `paperless_documents_download_to_path` | Dokument in eine Datei schreiben — **nur stdio** |

`paperless_ping` antwortet **auch dann mit einem Bericht, wenn die Verbindung kaputt
ist** — ein Health-Check, der bei einem Fehler wirft, wäre nutzlos. Er unterscheidet
dabei zwei Fälle: `/api/remote_version/` beantwortet Anfragen ohne Token und zeigt, ob
die Instanz überhaupt antwortet; `/api/status/` verlangt ein gültiges Token und zeigt, ob
die Anmeldedaten tragen.

```json
{ "isReachable": true, "isAuthenticated": false, "version": "v3.3.2",
  "updateAvailable": false,
  "detail": "The Paperless instance answered, but rejected the API token. Check PAPERLESS_API_TOKEN." }
```

Gegen eine echte Instanz (Paperless-ngx v3.3.2) ist der Durchstich für alle sechs Entities
verifiziert: über MCP angelegt, gelesen, geändert, seitenweise aufgelistet und gelöscht,
dazu die Fehlerabbildung bei fehlender Ressource und doppeltem Namen.

Dokumente lassen sich lesen und ändern. Das Ändern nutzt **PATCH**: nur die
übergebenen Felder werden geschrieben, alles andere bleibt — einschließlich des
extrahierten Textes. Eine Referenz zu **leeren** (Korrespondent entfernen) ist noch nicht
möglich, bei Tags und Feldwerten geht es über eine leere Liste.

## Löschen und andere destruktive Vorgänge

Alles, was Daten vernichten kann, läuft **per Default als Trockenlauf**. Ohne
`confirm: true` wird nichts verändert — stattdessen kommt ein Bericht, was geschehen
würde und **wen es trifft**:

```json
{ "tagId": 110, "name": "zz-guard-probe", "documentCount": 1, "removed": false,
  "summary": "Dry run: tag 'zz-guard-probe' would be deleted. 1 document(s) carry it
             and lose the assignment; the documents themselves are kept.
             Call again with confirm set to true to actually delete it." }
```

Die Zahl ist der Punkt: „hängt an 143 Dokumenten" ist eine andere Entscheidung als „hängt
an keinem". Betroffen sind `paperless_*_delete` für alle fünf Ordnungsmerkmale, das
Löschen von Dokumenten einzeln und im Bund, `paperless_documents_bulk_update`
und `paperless_documents_reprocess` — letzteres, weil es den
erkannten Text neu erzeugt und von Hand korrigierte Inhalte vernichtet.

Durchgesetzt wird das in der Processing-Schicht, nicht im Tool: die Foundation Services
löschen weiterhin bedingungslos, nur erreicht sie niemand mehr direkt.

## Dateizugriff: nur über stdio

`paperless_documents_upload_from_path` und `paperless_documents_download_to_path` fassen
das **lokale Dateisystem** an — und zwar das der Maschine, auf der der Server läuft. Über
HTTP wäre das ein Loch: ein entfernter Aufrufer könnte beliebige Serverdateien auslesen
oder überschreiben. Deshalb zwei Linien:

1. `Program.cs` meldet diese beiden Werkzeuge **nur im stdio-Zweig** an. Im HTTP-Transport
   erscheinen sie nicht in `tools/list`, und ein direkter Aufruf bekommt
   `Unknown tool`.
2. `DocumentFileProcessingService` prüft zusätzlich `Mcp:AllowLocalFileAccess` und
   verweigert ohne diese Erlaubnis, bevor es das Dateisystem anfasst.

Der Schalter ist bewusst **nicht** über eine Umgebungsvariable setzbar — genau wie
`Mcp:RequireRequestToken`. Der Transport entscheidet, nicht die Umgebung.

## Herunterladen: der Einmal-Link

Über stdio schreibt `paperless_documents_download_to_path` die Datei einfach auf die
Platte. Im HTTP-Betrieb geht das nicht — der Server läuft woanders, und der Inhalt einer
PDF ist nichts, was sinnvoll durch eine Werkzeugantwort passt. Dafür gibt es
`paperless_documents_download_link`:

```
paperless_documents_download_link  ->  "https://mcp.example.org/files/7inHntgjmfALPgZ…"
```

Der Link braucht **kein Token** und lässt sich deshalb direkt öffnen oder weitergeben.
Dass das vertretbar ist, hängt an vier Eigenschaften, und jede einzelne davon ist nötig:

- **Nicht zu erraten.** Die Kennung sind 32 Zufallsbytes aus `RandomNumberGenerator`, base64url
  kodiert. Durchprobieren ist aussichtslos.
- **Einmalig.** Das Einlösen nimmt den Link aus dem Vorrat (`TryRemove`), bevor irgendetwas
  anderes geschieht. Ein zweiter Abruf bekommt 404 — auch dann, wenn der erste
  fehlgeschlagen ist.
- **Kurzlebig.** Nach `DOWNLOAD_LINK_LIFETIME_SECONDS` (Default: fünf Minuten) ist er
  wertlos.
- **Er trägt das Token des Aufrufers.** Wer den Link anfordert, hinterlegt damit sein
  eigenes Paperless-Token; der spätere Abruf läuft mit **seinen** Rechten. Einen Rückfall
  auf ein konfiguriertes Token des Betreibers gibt es hier so wenig wie im
  `PaperlessAuthHandler` — aus demselben Grund.

Unbekannt, verbraucht, abgelaufen oder frei erfunden ergeben alle dieselbe Antwort: `404`.
Von außen soll nicht unterscheidbar sein, ob es einen Link je gab.

Hinter einem Reverse Proxy stimmt die Adresse aus der Anfrage nicht mit der überein, die
der Aufrufer kennt. Dann muss `MCP_PUBLIC_URL` gesetzt werden, sonst zeigt der Link ins
interne Netz.

Der Vorrat liegt im Arbeitsspeicher: ein Neustart entwertet alle offenen Links, und bei
mehreren Instanzen muss derselbe Prozess antworten, der den Link ausgestellt hat.

## Sammeloperationen

`paperless_documents_bulk_update` und `paperless_documents_bulk_delete` ändern
beziehungsweise löschen bis zu 100 Dokumente auf einmal. Beide laufen per Default als
Trockenlauf, und ihr Bericht nennt die Tragweite:

```json
{ "requestedDocumentIds": [1, 2, 9999], "affectedDocumentIds": [2, 1],
  "unknownDocumentIds": [9999], "affectedTitles": ["Rechnung Mai", "Rechnung Juni"],
  "changes": ["Correspondent set to 106", "Tags added: 149"], "applied": false,
  "summary": "Dry run: 2 of 3 requested document(s) (1 do not exist: 9999) would be
             changed. Call again with confirm set to true to apply it." }
```

`unknownDocumentIds` ist dabei wichtiger, als es aussieht: Paperless lässt unbekannte Ids
stillschweigend weg und antwortet trotzdem mit `200 OK`. Ohne diese Liste ginge ein
Tippfehler als Erfolg durch, und man hätte eine andere Menge geändert als gemeint.

Die Obergrenze von 100 Dokumenten je Aufruf begrenzt die Tragweite und hält die
betroffenen Dokumente auf einer Seite nachlesbar.

**Eine Zuordnung leeren** geht nur hier: `clearCorrespondent`, `clearDocumentType` und
`clearStoragePath` entfernen die Zuordnung, statt eine andere zu setzen. Die
Einzeländerung `paperless_documents_update` kann das nicht — dort heißt ein fehlendes
Feld „nicht ändern", und beides zugleich lässt sich nicht ausdrücken. Id und `clear`
gemeinsam anzugeben ist deshalb ein Fehler und kein Kompromiss.

Die API kennt darüber hinaus `merge`, `split`, `rotate`, `edit_pdf`, `delete_pages`,
`remove_password` und `set_permissions`. Die schreiben Dateien oder Rechte um und sind
bewusst nicht abgebildet. Ebenso wenig `all` und `filters`, mit denen ein einziger Aufruf
den gesamten Bestand treffen könnte.

## Hochladen

Paperless verarbeitet eine hochgeladene Datei **im Hintergrund** und antwortet deshalb mit
der Kennung eines Auftrags, nicht mit einem Dokument:

```
paperless_documents_upload  ->  "affaec8b-c97a-4e88-a83e-95b776d8b13a"
paperless_tasks_get         ->  { "status": "success", "relatedDocumentIds": [13], … }
```

Erst wenn der Auftrag durch ist, gibt es das Dokument — `relatedDocumentIds` nennt dann
seine Id. Der Status kommt klein geschrieben (`pending`, `started`, `success`, `failure`).

**Oder in einem Aufruf:** `paperless_documents_upload_and_wait` lädt hoch, wartet auf den
Auftrag und gibt das fertige Dokument zurück. Das ist der Normalfall, wenn man mit dem
Dokument danach weiterarbeiten will:

```json
{ "taskId": "affaec8b-…", "status": "success", "completed": true, "documentId": 23,
  "document": { "id": 23, "title": "Rechnung Mai", … },
  "summary": "Document 'Rechnung Mai' was created with id 23." }
```

Dauert die Verarbeitung länger als `timeoutSeconds` (1 bis 120, Default 30), ist der
Upload **nicht verloren**: `completed` ist dann `false`, und die `taskId` führt mit
`paperless_tasks_get` weiter. Ein Fehlschlag kommt ebenso als Bericht zurück und nicht als
Fehler — bei einem Upload ist „hat nicht geklappt" eine Auskunft, kein Ausfall.

Herunterladen, Löschen und Reprocess gibt es ebenfalls — siehe die Abschnitte oben.

## Was kann dieser Server?

`paperless_capabilities` beantwortet die Frage, die am Anfang einer größeren Aufgabe
steht. `tools/list` sagt bereits, **welche** Werkzeuge es gibt; hier steht, was man daraus
nicht ablesen kann:

```json
{ "serverVersion": "1.0.0", "transport": "http",
  "paperless": { "isReachable": true, "isAuthenticated": true, "version": "v3.3.2" },
  "localFileAccess": false, "downloadLinks": true, "requiresRequestToken": true,
  "maxPageSize": 100, "maxDocumentsPerBulkCall": 100, "maxUploadWaitSeconds": 120,
  "downloadLinkLifetimeSeconds": 300,
  "summary": "Paperless.Mcp 1.0.0 over http, talking to Paperless-ngx v3.3.2;
             files are handed out as temporary download links." }
```

Die Grenzen kommen aus den Diensten, die sie durchsetzen, nicht aus einer zweiten Zahl,
die irgendwann auseinanderliefe. Und wie `paperless_ping` antwortet der Bericht **auch
dann**, wenn die Paperless-Instanz nicht erreichbar ist — gerade dann ist er nützlich.

## Accept-Header: auch einfache Clients

Streamable HTTP verlangt vom Aufrufer, dass er **beides** akzeptiert:
`application/json` **und** `text/event-stream`. Wer nur `application/json` sendet, bekommt
von der Protokollbibliothek ein `406 Not Acceptable` und kommt nicht weiter. Genau das
senden aber viele einfache Clients — Skripte, Automatisierungswerkzeuge, `curl` mit
eigenem Header.

Der Server bedient sie, ohne zu lügen: er ergänzt den Header für die Bibliothek und packt
die Antwort danach wieder aus. Aus dem SSE-Rahmen wird die nackte JSON-Antwort, die der
Aufrufer angefragt hat:

```bash
curl -X POST http://localhost:5005/mcp \
  -H "Content-Type: application/json" -H "Accept: application/json" \
  -H "Authorization: Token <token>" \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'
```

Den Rahmen blind durchzureichen wäre schlechter als das 406: der Aufrufer bekäme `200` und
einen Rumpf, den er nicht lesen kann. Deshalb bleibt alles unangetastet, was sich nicht
eindeutig auspacken lässt — etwa eine Antwort mit mehr als einer Nachricht.

## Globale Suche

`paperless_search` ist der Einstiegspunkt, wenn man die Instanz nicht kennt: ein Aufruf
findet Treffer über sechs Sammlungen hinweg und liefert **nur Id und Bezeichnung**.

```json
{ "total": 2, "documents": [{ "id": 2, "label": "Rechnung 4712" }],
  "tags": [], "correspondents": [{ "id": 9, "label": "Musterfirma" }],
  "documentTypes": [], "storagePaths": [], "customFields": [] }
```

Zwei bewusste Abweichungen von `/api/search/`:

- **Nur die sechs fachlichen Sammlungen.** `users`, `groups`, `mail_accounts`,
  `mail_rules`, `workflows` und `saved_views` sind Verwaltung und teils heikel — ein
  Benutzerverzeichnis gehört nicht ungefragt in die Antwort eines Dokumenten-Werkzeugs.
- **Leichte Treffer statt Vollobjekte.** Paperless liefert bei Dokumenten den gesamten
  extrahierten Text mit; für Details ist `paperless_documents_get` da. Gemessen an der
  Testinstanz: 351 statt 1844 Zeichen, gut fünfmal kleiner.

`total` ist die Zählung von Paperless über **alle** Sammlungen — auch die hier nicht
gezeigten. Die Summe der Listen kann also kleiner sein.

Alle geplanten Entities sind gebaut.
