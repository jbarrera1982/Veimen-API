namespace Veimen_API.Models;

public class MailScanState
{
    public int Id { get; set; }
    public string Mailbox { get; set; } = "INBOX";
    public DateTime LastScannedAt { get; set; }
}
