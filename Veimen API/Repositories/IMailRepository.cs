using Veimen_API.Models.Dtos;

namespace Veimen_API.Repositories;

public interface IMailRepository
{
    Task<bool> MessageExistsAsync(string mailbox, ulong uid);
    Task<int> InsertMessageAsync(Models.MailMessage message);
    Task DeleteMessagesByDateRangeAsync(string mailbox, DateTime startDate, DateTime endDate);
    Task<IEnumerable<MailDailySummaryDto>> RecalculateSummaryAsync(string mailbox, DateTime startDate, DateTime endDate);
    Task<IEnumerable<MailDailySummaryDto>> GetSummaryAsync(DateOnly? startDate, DateOnly? endDate);
    Task<IEnumerable<MailComparisonDto>> GetComparisonAsync(DateOnly? startDate, DateOnly? endDate);
    Task<IEnumerable<DateOnly>> GetSummaryDatesInRangeAsync(string mailbox, DateTime startDate, DateTime endDate);
    Task DeleteSummariesByDatesAsync(string mailbox, IEnumerable<DateOnly> dates);
}
