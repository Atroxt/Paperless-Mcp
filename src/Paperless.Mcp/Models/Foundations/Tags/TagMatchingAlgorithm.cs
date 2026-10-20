namespace Paperless.Mcp.Models.Foundations.Tags
{
    // Werte entsprechen den Zahlen, die die Paperless-API erwartet und liefert.
    public enum TagMatchingAlgorithm
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
