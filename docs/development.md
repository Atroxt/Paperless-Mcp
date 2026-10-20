# Entwicklung

[Zurück zur Übersicht](../README.md)


```bash
dotnet build Paperless-Mcp.slnx -c Release
dotnet test Paperless-Mcp.slnx -c Release
dotnet test Paperless-Mcp.slnx --filter "FullyQualifiedName~TagServiceTests"
```

Tests laufen auf **Microsoft.Testing.Platform** (konfiguriert über `global.json`),
nicht VSTest. Argumente für die Testplattform stehen hinter `--`.

Zwei Stolpersteine, die beim Entwickeln Zeit kosten:

- **XML-Dateien vertragen kein `--` in Kommentaren.** Betrifft `Directory.Build.props`,
  `Directory.Packages.props`, die csproj-Dateien und vor allem `coverage.runsettings`:
  ein Kommentar mit `--coverage` darin macht die Datei ungültig, und die Fehlermeldung
  sagt nur „nicht vorhanden oder ungültig".
- **`dotnet run` lädt `Properties/launchSettings.json`**, und die dort gesetzten
  Umgebungsvariablen überschreiben die, die man dem Befehl mitgibt. `MCP_PORT=5019
  dotnet run …` landet deshalb trotzdem auf 5005. Für Versuche mit abweichenden Werten
  `--no-launch-profile` mitgeben, sonst misst man am falschen Prozess.

### Testkonventionen

MSTest statt xUnit, sonst Moq, AwesomeAssertions, DeepCloner, Tynamix.ObjectFiller und
Xeption. `[TestClass]` gehört genau einmal an das Root-Partial, Theorien laufen über
`[DataRow]` beziehungsweise `[DynamicData]`.

Pro Service wird aufgeteilt in `{Entity}ServiceTests.cs` (Setup, Mocks, `CreateRandom*`)
sowie `.Logic.{Methode}.cs`, `.Validations.{Methode}.cs` und `.Exceptions.{Methode}.cs`.

Jeder Test trägt GWT-Kommentare, vergleicht mit `Should().BeEquivalentTo()`, verifiziert
Broker-Aufrufe mit `Times.Once` oder `Times.Never` und schließt mit
`VerifyNoOtherCalls()` ab.

**Der häufigste Fehler dabei:** `VerifyNoOtherCalls()` ohne vorheriges `Verify` des
erwarteten Aufrufs. Moq meldet dann „failed verification due to the following unverified
invocations", und der Test ist rot, obwohl der Code stimmt. Jeder Aufruf, der passieren
**soll**, braucht sein eigenes `Verify` davor.

Die Reihenfolge der Tests je Methode folgt dem Weg durch die Schichten: Happy Path →
strukturell (null) → logisch (`[DataRow]`) → Abhängigkeitsvalidierung (BadRequest,
Conflict) → kritische Abhängigkeit (Unauthorized, Forbidden, NotFound, UrlNotFound) →
nicht-kritische Abhängigkeit (InternalServerError, ServiceUnavailable) → Transport
(`HttpRequestException`) → Catch-All (`Exception`).

### Testabdeckung

```bash
dotnet test Paperless-Mcp.slnx -c Release -- --coverage --coverage-settings coverage.runsettings --coverage-output-format cobertura
```

Die Berichte landen unter `TestResults/<guid>.cobertura.xml`, **einer je Testprojekt**.
Einen festen Dateinamen über `--coverage-output` zu setzen wäre falsch: beide Projekte
schrieben dann in dieselbe Datei, der zweite Lauf überschriebe den ersten, und die Hälfte
des Codes sähe ungetestet aus.

Die Einstellungen stehen in [coverage.runsettings](../coverage.runsettings); ausgeschlossen
sind dort nur die Testprojekte selbst. `Brokers/` und `Program.cs` bleiben bewusst in der
Auswertung, obwohl sie nicht unit-getestet werden — die Quote soll ehrlich bleiben.
Die Unit-Tests decken derzeit 67,5 Prozent der Zeilen und 49,3 Prozent der Zweige ab.

### Integrationstests

`src/Paperless.Mcp.Tests.Integrations` prüft gegen eine **echte** Paperless-Instanz, was
Unit-Tests nicht können: ob die JSON-Form stimmt — snake_case-Zuordnung, Enum-Werte,
Pagination und die Übersetzung echter HTTP-Fehler in unsere Exception-Kategorien.

Die Tests laufen nur, wenn beide Variablen gesetzt sind, und melden sich sonst als
*inconclusive* ab, damit CI ohne Instanz grün bleibt:

```bash
PAPERLESS_TEST_BASE_URL=http://localhost:8000 PAPERLESS_TEST_API_TOKEN=<token> dotnet test Paperless-Mcp.slnx -c Release
```

Bequemer, besonders aus der IDE heraus: die Vorlage
[integration.runsettings.example](../src/Paperless.Mcp.Tests.Integrations/integration.runsettings.example)
nach `integration.runsettings` kopieren, Werte eintragen und mitgeben:

```bash
dotnet test src/Paperless.Mcp.Tests.Integrations/Paperless.Mcp.Tests.Integrations.csproj -c Release -- --settings src/Paperless.Mcp.Tests.Integrations/integration.runsettings
```

Die Kopie steht in `.gitignore` — sie enthält ein gültiges Token und gehört nicht ins
Repository. Die Vorlage daneben ist versioniert, damit ein frischer Clone weiß, was zu
tun ist.

Bewusst eigene Variablennamen statt `PAPERLESS_BASE_URL` und `PAPERLESS_API_TOKEN`: diese
Tests **legen Tags an und löschen sie wieder**. Wer die Produktivvariablen gesetzt hat,
soll nicht versehentlich Testdaten in seiner echten Ablage anlegen — das Opt-in muss
ausdrücklich sein. Zeigt `PAPERLESS_BASE_URL` auf etwas anderes als
`PAPERLESS_TEST_BASE_URL`, brechen die Tests mit einer Meldung ab, statt die falsche
Instanz zu treffen.

Angelegte Testdaten heißen `zz-integrationstest-<guid>` und werden im `finally`-Zweig
wieder entfernt. Bleibt nach einem Abbruch etwas liegen, ist es am Präfix erkennbar.

### Paketversionen

Alle NuGet-Versionen stehen zentral in
[Directory.Packages.props](../Directory.Packages.props) (Central Package Management).
Die `.csproj`-Dateien nennen nur Paketnamen, keine Versionen.

### Docker

Fertiges Setup zum Übernehmen liegt in [docker/](../docker/) — `compose.yaml`,
`.env.example` und eine Anleitung:

```bash
cd docker
cp .env.example .env     # PAPERLESS_BASE_URL eintragen
docker compose up -d
```

In die `.env` gehört **kein** API-Token: im HTTP-Betrieb bringt jede Anfrage ihr eigenes
mit. Siehe [docker/README.md](../docker/README.md), dort auch der häufigste Stolperstein —
wie der Container an eine Paperless-Instanz kommt, die in einem anderen Compose-Projekt
läuft.

Selbst bauen geht aus der Repository-Wurzel, nicht aus `docker/`: der Build braucht
`Directory.Build.props`, `Directory.Packages.props` und `NuGet.config`, und die liegen
dort.

```bash
docker build -f src/Paperless.Mcp/Dockerfile -t paperless-mcp .
```

CI veröffentlicht nach `ghcr.io/atroxt/paperless-mcp` — aber nur von `main` und von
Tags. Ein Push auf einen Feature-Branch läuft durch Build und Test und schiebt kein
Image.


## Versionierung

Single source of truth ist `<Version>` in `src/Paperless.Mcp/Paperless.Mcp.csproj`.
`Directory.Build.props` leitet daraus `AssemblyVersion`, `FileVersion` und
`InformationalVersion` ab; lokale Builds ohne gesetzte Version laufen als
`0.0.0-local`. Ein Git-Tag `vX.Y.Z` muss zur csproj-Version passen, sonst schlägt CI
fehl.

Commits folgen [Conventional Commits](https://www.conventionalcommits.org/),
Entwicklung ist trunk-based auf `main`.
