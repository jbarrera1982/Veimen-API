using Dapper;
using Veimen_API.Data;

namespace Veimen_API.Repositories;

public class MailScanStateRepository : IMailScanStateRepository
{
    private readonly DapperContext _context;

    public MailScanStateRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<DateTime?> GetLastScannedAtAsync(string mailbox)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT last_scanned_at FROM mail_scan_state WHERE mailbox = @Mailbox";
        var result = await connection.QueryFirstOrDefaultAsync<DateTime?>(sql, new { Mailbox = mailbox });
        return result;
    }

    public async Task UpsertLastScannedAtAsync(string mailbox, DateTime scannedAtUtc)
    {
        using var connection = _context.CreateConnection();
        var sql = @"INSERT INTO mail_scan_state (mailbox, last_scanned_at)
                    VALUES (@Mailbox, @ScannedAtUtc)
                    ON DUPLICATE KEY UPDATE last_scanned_at = @ScannedAtUtc";
        await connection.ExecuteAsync(sql, new { Mailbox = mailbox, ScannedAtUtc = scannedAtUtc });
    }
}
