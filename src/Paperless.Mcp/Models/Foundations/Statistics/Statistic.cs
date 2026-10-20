namespace Paperless.Mcp.Models.Foundations.Statistics
{
    // Primary Model: Ueberblick ueber die Paperless-Instanz. Felder, die nicht jede
    // Paperless-Version liefert, sind nullable statt erfunden.
    public class Statistic
    {
        public int DocumentsTotal { get; set; }

        // null, wenn in Paperless kein Posteingangs-Tag eingerichtet ist.
        public int? DocumentsInbox { get; set; }

        // Aeltere Versionen kennen nur einen Posteingangs-Tag, neuere eine Liste.
        public int? InboxTag { get; set; }

        public List<int>? InboxTags { get; set; }

        public List<StatisticFileTypeCount> DocumentFileTypeCounts { get; set; } = [];

        public long? CharacterCount { get; set; }

        public int? TagCount { get; set; }

        public int? CorrespondentCount { get; set; }

        public int? DocumentTypeCount { get; set; }

        public int? StoragePathCount { get; set; }

        public int? CurrentAsn { get; set; }
    }
}
