namespace Paperless.Mcp.Models.Foundations.StoragePaths
{
    // Inhaltlich gleich zu den Matching-Enums der anderen Entities und trotzdem bewusst
    // ein eigener Typ: geteilte Modelle ueber unabhaengige Flows hinweg gibt es nicht.
    // Duplizierung erhaelt die Autonomie, ein gemeinsamer Enum koppelt die Entities.
    public enum StoragePathMatchingAlgorithm
    {
        None = 0,
        Any = 1,
        All = 2,
        Literal = 3,
        RegularExpression = 4,
        Fuzzy = 5,
        Auto = 6,
    }
}
