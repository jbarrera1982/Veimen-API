namespace Veimen_API.Models.Dtos;

// Fila del dashboard: total de service requests por día (receipt_date) y status.
public class ServiceRequestDashboardRow
{
    public DateTime Date { get; set; }

    public string Status { get; set; } = string.Empty;

    public long Total { get; set; }
}
