namespace Veimen_API.Models.Dtos;

public class MailComparisonDto
{
    public string Date { get; set; } = string.Empty;
    public int Mails { get; set; }
    public int Requests { get; set; }
}
