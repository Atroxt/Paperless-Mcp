namespace Paperless.Mcp.Models.Processings.StoragePaths
{
    // Ergebnis eines Loeschvorgangs -- geplant oder ausgefuehrt.
    //
    // Destruktive Operationen laufen per Default als Trockenlauf: ohne confirm wird
    // nichts entfernt, sondern berichtet, was entfernt WUERDE. Der Bericht nennt die
    // Tragweite, damit eine Bestaetigung ueberhaupt etwas wert ist -- "haengt an 143
    // Dokumenten" ist eine andere Entscheidung als "haengt an keinem".
    public class StoragePathRemovalPlan
    {
        public int StoragePathId { get; set; }

        public string Name { get; set; } = string.Empty;

        // Wie viele Dokumente diesen Ablagepfad nutzen. Sie bleiben erhalten und fallen
        // auf den Standardort zurueck.
        public int DocumentCount { get; set; }

        // false bei einem Trockenlauf, true wenn tatsaechlich entfernt wurde.
        public bool Removed { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
