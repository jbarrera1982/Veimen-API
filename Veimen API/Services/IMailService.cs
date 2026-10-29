using Veimen_API.Models.Dtos;

namespace Veimen_API.Services;

public interface IMailService
{
    Task<IEnumerable<MailDailySummaryDto>> ScanAsync(MailScanRequest request);
    Task<IEnumerable<MailDailySummaryDto>> GetSummaryAsync(string? startDate, string? endDate);
    Task<IEnumerable<MailComparisonDto>> GetComparisonAsync(string? startDate, string? endDate);
}
