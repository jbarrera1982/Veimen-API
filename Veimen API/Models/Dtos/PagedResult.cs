namespace Veimen_API.Models.Dtos;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];

    public long TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
