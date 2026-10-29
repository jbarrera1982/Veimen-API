using Dapper;
using MySqlConnector;
using Veimen_API.Data;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Repositories;

public class MailRepository : IMailRepository
{
    private readonly DapperContext _context;

    public MailRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<bool> MessageExistsAsync(string mailbox, ulong uid)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT COUNT(1) FROM mail_message WHERE mailbox = @Mailbox AND uid = @Uid";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Mailbox = mailbox, Uid = uid });
        return count > 0;
    }

    public async Task<int> InsertMessageAsync(Models.MailMessage message)
    {
        using var connection = _context.CreateConnection();
        var sql = @"INSERT IGNORE INTO mail_message (client_id, mailbox, uid, message_id, from_address, subject, message_date, received_at)
                    VALUES (@ClientId, @Mailbox, @Uid, @MessageId, @FromAddress, @Subject, @MessageDate, @ReceivedAt)";
        return await connection.ExecuteAsync(sql, message);
    }

    public async Task DeleteMessagesByDateRangeAsync(string mailbox, DateTime startDate, DateTime endDate)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM mail_message WHERE mailbox = @Mailbox AND message_date >= @StartDate AND message_date < @EndDate";
        await connection.ExecuteAsync(sql, new { Mailbox = mailbox, StartDate = startDate, EndDate = endDate });
    }

    public async Task<IEnumerable<DateOnly>> GetSummaryDatesInRangeAsync(string mailbox, DateTime startDate, DateTime endDate)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT DISTINCT DATE(message_date) FROM mail_message WHERE mailbox = @Mailbox AND message_date >= @StartDate AND message_date < @EndDate";
        var dates = await connection.QueryAsync<DateTime>(sql, new { Mailbox = mailbox, StartDate = startDate, EndDate = endDate });
        return dates.Select(d => DateOnly.FromDateTime(d));
    }

    public async Task DeleteSummariesByDatesAsync(string mailbox, IEnumerable<DateOnly> dates)
    {
        var dateList = dates.ToList();
        if (!dateList.Any()) return;
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM mail_daily_summary WHERE mailbox = @Mailbox AND summary_date IN @Dates";
        await connection.ExecuteAsync(sql, new { Mailbox = mailbox, Dates = dateList.Select(d => d.ToDateTime(TimeOnly.MinValue)) });
    }

    public async Task<IEnumerable<MailDailySummaryDto>> RecalculateSummaryAsync(string mailbox, DateTime startDate, DateTime endDate)
    {
        using var connection = _context.CreateConnection();

        // Recalcular totales por día desde mail_message
        var sql = @"SELECT DATE_FORMAT(message_date, '%Y%m%d') AS Date, COUNT(*) AS Count
                    FROM mail_message
                    WHERE mailbox = @Mailbox 
                      AND message_date >= @StartDate 
                      AND message_date < @EndDate
                    GROUP BY DATE_FORMAT(message_date, '%Y%m%d')
                    ORDER BY Date;";

        var results = await connection.QueryAsync<MailDailySummaryDto>(sql, new { Mailbox = mailbox, StartDate = startDate, EndDate = endDate });

        // Guardar/actualizar resúmenes
        var upsertSql = @"INSERT INTO mail_daily_summary (mailbox, summary_date, count, last_recalculated_at)
                          VALUES (@Mailbox, STR_TO_DATE(@Date, '%Y%m%d'), @Count, @LastRecalculatedAt)
                          ON DUPLICATE KEY UPDATE count = @Count, last_recalculated_at = @LastRecalculatedAt";

        foreach (var result in results)
        {
            var summaryDate = DateTime.ParseExact(result.Date, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            await connection.ExecuteAsync(upsertSql, new
            {
                Mailbox = mailbox,
                Date = result.Date,
                Count = result.Count,
                LastRecalculatedAt = DateTime.UtcNow
            });
        }

        return results;
    }

    public async Task<IEnumerable<MailDailySummaryDto>> GetSummaryAsync(DateOnly? startDate, DateOnly? endDate)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT DATE_FORMAT(summary_date, '%Y%m%d') AS Date, count AS Count FROM mail_daily_summary WHERE 1=1";
        
        if (startDate.HasValue)
        {
            sql += " AND summary_date >= @StartDate";
        }
        if (endDate.HasValue)
        {
            sql += " AND summary_date <= @EndDate";
        }
        sql += " ORDER BY summary_date";
        
        return await connection.QueryAsync<MailDailySummaryDto>(sql, new { StartDate = startDate?.ToDateTime(TimeOnly.MinValue), EndDate = endDate?.ToDateTime(TimeOnly.MinValue) });
    }

    public async Task<IEnumerable<MailComparisonDto>> GetComparisonAsync(DateOnly? startDate, DateOnly? endDate)
    {
        using var connection = _context.CreateConnection();
        var sql = @"SELECT 
                        DATE_FORMAT(m.mail_date, '%Y%m%d') AS Date,
                        m.mails AS Mails,
                        r.requests AS Requests
                    FROM (	
                        SELECT 
                            DATE(message_date) AS mail_date,
                            COUNT(1) AS mails
                        FROM 
                            mail_message
                        GROUP BY 
                            DATE(message_date)
                    ) m
                    INNER JOIN (	
                        SELECT 
                            DATE(receipt_date) AS request_date,
                            COUNT(1) AS requests	
                        FROM 
                            service_request
                        GROUP BY 
                            DATE(receipt_date)
                    ) r ON m.mail_date = r.request_date
                    WHERE 1=1";
        
        if (startDate.HasValue)
        {
            sql += " AND m.mail_date >= @StartDate";
        }
        if (endDate.HasValue)
        {
            sql += " AND m.mail_date <= @EndDate";
        }
        sql += " ORDER BY m.mail_date";
        
        return await connection.QueryAsync<MailComparisonDto>(sql, new { StartDate = startDate?.ToDateTime(TimeOnly.MinValue), EndDate = endDate?.ToDateTime(TimeOnly.MinValue) });
    }
}
