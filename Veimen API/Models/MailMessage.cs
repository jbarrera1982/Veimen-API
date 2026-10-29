namespace Veimen_API.Models;

public class MailMessage
{
    public long Id { get; set; }
    public long ClientId { get; set; } = 0;
    public string Mailbox { get; set; } = "INBOX";
    public ulong Uid { get; set; }
    public string? MessageId { get; set; }
    public string? FromAddress { get; set; }
    public string? Subject { get; set; }
    public DateTime MessageDate { get; set; }
    public DateTime ReceivedAt { get; set; }
}
