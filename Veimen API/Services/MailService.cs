using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using Veimen_API.Helpers;
using Veimen_API.Models.Dtos;
using Veimen_API.Repositories;

namespace Veimen_API.Services;

public class MailService : IMailService
{
    private readonly IConfiguration _configuration;
    private readonly IMailRepository _mailRepository;
    private readonly IMailScanStateRepository _mailScanStateRepository;
    private const string MailboxName = "INBOX";

    public MailService(IConfiguration configuration, IMailRepository mailRepository, IMailScanStateRepository mailScanStateRepository)
    {
        _configuration = configuration;
        _mailRepository = mailRepository;
        _mailScanStateRepository = mailScanStateRepository;
    }

    public async Task<IEnumerable<MailDailySummaryDto>> ScanAsync(MailScanRequest request)
    {
        var host = _configuration["Imap:Host"] ?? "vda3700.is.cc";
        var port = int.Parse(_configuration["Imap:Port"] ?? "993");
        var username = _configuration["Imap:Username"] ?? string.Empty;
        var password = _configuration["Imap:Password"] ?? string.Empty;
        var useSsl = bool.Parse(_configuration["Imap:UseSsl"] ?? "true");

        using var client = new ImapClient();
        await client.ConnectAsync(host, port, useSsl);
        await client.AuthenticateAsync(username, password);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly);

        var lastScannedAt = await _mailScanStateRepository.GetLastScannedAtAsync(MailboxName);
        var scanStartTime = DateTime.UtcNow;

        // Determinar rango a escanear
        List<MimeMessage> messagesToProcess = new List<MimeMessage>();
        List<UniqueId> uidsToProcess = new List<UniqueId>();

        if (request.Force || lastScannedAt == null)
        {
            // Escanear todo el histórico
            var allUids = await inbox.SearchAsync(SearchQuery.All);
            foreach (var uid in allUids)
            {
                var message = await inbox.GetMessageAsync(uid);
                messagesToProcess.Add(message);
                uidsToProcess.Add(uid);
            }
        }
        else
        {
            // Escanear desde último escaneo - buscar mensajes con fecha de cabecera posterior o procesar todos y filtrar
            // Buscamos todos y filtramos por fecha de cabecera > lastScannedAt
            var allUids = await inbox.SearchAsync(SearchQuery.All);
            foreach (var uid in allUids)
            {
                var message = await inbox.GetMessageAsync(uid);
                var messageDate = message.Date.UtcDateTime;
                if (messageDate > lastScannedAt.Value)
                {
                    messagesToProcess.Add(message);
                    uidsToProcess.Add(uid);
                }
            }
        }

        // Para recalcular: si hay force o es primera vez, podríamos recalcular todo; 
        // según requisitos "recalcular (borrar los registros del rango y volver a contar)"
        // Determinar fechas afectadas
        var affectedDates = new HashSet<DateOnly>();
        foreach (var message in messagesToProcess)
        {
            var msgDate = DateOnly.FromDateTime(message.Date.UtcDateTime);
            affectedDates.Add(msgDate);
        }

        // Si force o primera vez sin lastScannedAt, recalculamos todo el rango que cubren los mensajes
        if (request.Force || lastScannedAt == null)
        {
            // Borrar todos los mensajes y resúmenes para recalcular desde cero
            // Obtener rango completo
            if (messagesToProcess.Any())
            {
                var minDate = messagesToProcess.Min(m => m.Date.UtcDateTime.Date);
                var maxDate = messagesToProcess.Max(m => m.Date.UtcDateTime.Date).AddDays(1);
                await _mailRepository.DeleteMessagesByDateRangeAsync(MailboxName, minDate, maxDate);
                var datesInRange = await _mailRepository.GetSummaryDatesInRangeAsync(MailboxName, minDate, maxDate);
                await _mailRepository.DeleteSummariesByDatesAsync(MailboxName, datesInRange);
            }
        }
        else if (affectedDates.Any())
        {
            // Borrar mensajes y resúmenes solo del rango afectado
            var affectedDateList = affectedDates.ToList();
            var minAffected = affectedDateList.Min().ToDateTime(TimeOnly.MinValue);
            var maxAffected = affectedDateList.Max().ToDateTime(TimeOnly.MinValue).AddDays(1);
            await _mailRepository.DeleteMessagesByDateRangeAsync(MailboxName, minAffected, maxAffected);
            await _mailRepository.DeleteSummariesByDatesAsync(MailboxName, affectedDates);
        }

        // Guardar mensajes
        for (int i = 0; i < messagesToProcess.Count; i++)
        {
            var message = messagesToProcess[i];
            var uid = uidsToProcess[i].Id;
            
            var mailMessage = new Models.MailMessage
            {
                ClientId = 1,
                Mailbox = MailboxName,
                Uid = uid,
                MessageId = message.MessageId,
                FromAddress = message.From.Mailboxes.FirstOrDefault()?.Address,
                Subject = message.Subject,
                MessageDate = message.Date.UtcDateTime,
                ReceivedAt = DateTime.UtcNow
            };
            
            await _mailRepository.InsertMessageAsync(mailMessage);
        }

        // Recalcular resúmenes para fechas afectadas
        IEnumerable<MailDailySummaryDto> result = new List<MailDailySummaryDto>();
        if (affectedDates.Any())
        {
            var affectedDateList = affectedDates.ToList();
            var minAffected = affectedDateList.Min().ToDateTime(TimeOnly.MinValue);
            var maxAffected = affectedDateList.Max().ToDateTime(TimeOnly.MinValue).AddDays(1);
            result = await _mailRepository.RecalculateSummaryAsync(MailboxName, minAffected, maxAffected);
        }

        // Actualizar último escaneo
        await _mailScanStateRepository.UpsertLastScannedAtAsync(MailboxName, scanStartTime);

        await client.DisconnectAsync(true);
        return result.OrderBy(r => r.Date);
    }

    public async Task<IEnumerable<MailDailySummaryDto>> GetSummaryAsync(string? startDate, string? endDate)
    {
        DateOnly? start = null;
        DateOnly? end = null;

        if (!string.IsNullOrWhiteSpace(startDate) && DateQueryParser.TryParse(startDate, out var parsedStart))
        {
            start = DateOnly.FromDateTime(parsedStart.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(endDate) && DateQueryParser.TryParse(endDate, out var parsedEnd))
        {
            end = DateOnly.FromDateTime(parsedEnd.Value.Date);
        }

        return await _mailRepository.GetSummaryAsync(start, end);
    }

    public async Task<IEnumerable<MailComparisonDto>> GetComparisonAsync(string? startDate, string? endDate)
    {
        DateOnly? start = null;
        DateOnly? end = null;

        if (!string.IsNullOrWhiteSpace(startDate) && DateQueryParser.TryParse(startDate, out var parsedStart))
        {
            start = DateOnly.FromDateTime(parsedStart.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(endDate) && DateQueryParser.TryParse(endDate, out var parsedEnd))
        {
            end = DateOnly.FromDateTime(parsedEnd.Value.Date);
        }

        return await _mailRepository.GetComparisonAsync(start, end);
    }
}
