namespace Paperless.Mcp.Models.Foundations.Correspondents
{
    // Inhaltlich gleich zu TagMatchingAlgorithm und trotzdem bewusst ein eigener Typ:
    // geteilte Modelle ueber unabhaengige Flows hinweg gibt es hier nicht.
    // Duplizierung erhaelt die Autonomie, ein gemeinsamer Enum koppelt die Entities.
    public enum CorrespondentMatchingAlgorithm
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
