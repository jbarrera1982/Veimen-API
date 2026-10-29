namespace Veimen_API.Models;

public class MailDailySummary
{
    public long SummaryId { get; set; }
    public string Mailbox { get; set; } = "INBOX";
    public DateOnly SummaryDate { get; set; }
    public int Count { get; set; }
    public DateTime LastRecalculatedAt { get; set; }
}
