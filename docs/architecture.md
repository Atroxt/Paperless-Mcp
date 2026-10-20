# Architektur

[Zurück zur Übersicht](../README.md)


Flow: **Exposer → Service → Broker → Paperless-ngx.** Niemals rückwärts, niemals eine
Schicht überspringen.

```
src/Paperless.Mcp/
├── Brokers/            Abhängigkeiten: nur durchreichen, keine Logik, kein try/catch
│   ├── Paperlesses/    API-Broker, partiell je Entity (RESTFulSense, snake_case)
│   ├── DownloadLinks/  Vorrat der Einmal-Links, im Arbeitsspeicher
│   ├── Loggings/       LoggingBroker
│   ├── DateTimes/      DateTimeBroker
│   ├── Delays/         DelayBroker: Warten, im Test ersetzbar
│   └── Files/          FileBroker, nur im stdio-Betrieb genutzt
├── Models/             Entities und Exception-Hierarchien je Entity
├── Services/
│   ├── Foundations/    Validierung, Mapping, Exception-Lokalisierung
│   ├── Processings/    Höherwertige Logik einer Entity
│   └── Orchestrations/ Mehrere Entities zusammen
├── Exposers/
│   ├── Tools/          MCP-Tools: reines Mapping
│   └── Middlewares/    Accept-Header-Kompatibilität
└── Program.cs          Komposition: Transportwahl, DI
```

Harte Regeln: Brokers verzweigen nicht und fangen nichts ab. Eine Foundation Service hat
genau einen Entity-Broker, eine Processing Service genau eine Foundation Service, ein Tool
genau eine Service und nie einen Broker. Es gibt kein `Utils/`, `Common/` oder `Helpers/`
und keine über Flows hinweg geteilten Modelle oder Exceptions — Duplizierung ist dort
erwünscht, wo sie Autonomie erhält. Jede öffentlich exponierte Methode gibt
`ValueTask`/`ValueTask<T>` zurück, auch wenn intern nichts awaited wird.

### Exceptions

Jede Entity besitzt ihre eigene Exception-Hierarchie unter
`Models/Foundations/{Plural}/Exceptions/`. Native Exceptions werden in der Foundation
Service lokalisiert: Data-Dictionary übertragen, native Exception als `InnerException`,
dann in die kategoriale Exception wrappen und loggen. `TryCatch<T>` ist generisch und
gilt für alle Methoden einer Entity — die Zuordnung wird pro Entity einmal geschrieben,
nicht pro Methode.

Verbindlich ist diese Tabelle; sie entspricht `TagService.Exceptions.cs`, an dem sich
alle weiteren Entities orientieren. Die Reihenfolge der `catch`-Blöcke ist festgelegt:
spezifisch vor Basis.

```
Native Exception                          → Inner (lokal)                 → Outer (kategorial)                Log
───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
(null input)                              → Null{E}Exception              → {E}ValidationException            Error
(Validierungsregel verletzt)              → Invalid{E}Exception           → {E}ValidationException            Error
HttpResponseBadRequestException           → Invalid{E}Exception           → {E}DependencyValidationException  Error
HttpResponseConflictException             → AlreadyExists{E}Exception     → {E}DependencyValidationException  Error
HttpResponseNotFoundException             → NotFound{E}Exception          → {E}DependencyValidationException  Error
HttpResponseUnauthorizedException         → Unauthorized{E}Exception      → {E}DependencyValidationException  Error
HttpResponseForbiddenException            → Unauthorized{E}Exception      → {E}DependencyValidationException  Error
HttpResponseUrlNotFoundException          → Failed{E}DependencyException  → {E}DependencyException            Critical
HttpRequestException                      → Failed{E}DependencyException  → {E}DependencyException            Critical
HttpResponseInternalServerErrorException  → Failed{E}DependencyException  → {E}DependencyException            Error
HttpResponseServiceUnavailableException   → Failed{E}DependencyException  → {E}DependencyException            Error
HttpResponseException (Basisfall)         → Failed{E}DependencyException  → {E}DependencyException            Error
Exception (Catch-All)                     → Failed{E}ServiceException     → {E}ServiceException               Error
```

Drei Zeilen weichen von dem ab, was man erwarten würde, und jede hat ihren Grund:

- **404 ist zweigeteilt.** RESTFulSense unterscheidet `HttpResponseNotFoundException`
  (die Ressource fehlt) von `HttpResponseUrlNotFoundException` (der Endpunkt ist falsch).
  „Diesen Tag gibt es nicht" ist ein Eingabefehler des Aufrufers, ein falscher Endpunkt
  ein Fehler des Servers. Nur der zweite ist kritisch.
- **401 und 403 sind Abhängigkeitsvalidierung, nicht kritisch.** Mit Per-Request-Token
  ist ein abgelehntes Token der Fehler des Aufrufers und von ihm behebbar.
- **`HttpResponseException` fängt den Rest ab.** Jede sonstige Antwort außerhalb des
  Erfolgsbereichs landet dort — sie muss nach den spezifischen Fällen stehen.

Das Tool mappt die kategoriale Exception auf eine MCP-Fehlerantwort:
`{E}ValidationException` und `{E}DependencyValidationException` als Anwenderfehler,
`{E}DependencyException` und `{E}ServiceException` als Serverfehler.
