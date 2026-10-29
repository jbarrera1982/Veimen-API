namespace Veimen_API.Repositories;

public interface IMailScanStateRepository
{
    Task<DateTime?> GetLastScannedAtAsync(string mailbox);
    Task UpsertLastScannedAtAsync(string mailbox, DateTime scannedAtUtc);
}
