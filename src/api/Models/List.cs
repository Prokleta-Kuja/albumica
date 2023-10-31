using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace albumica.Models;

public class FilterQuery
{
    public FilterQuery()
    {
        size = 25;
        page = 1;
    }
    private int size;
    private int page;
    public int? Size { get => size; set => size = value.HasValue ? value.Value > 100 ? 100 : value.Value < 1 ? 10 : value.Value : 25; }
    public int? Page { get => page; set => page = value.HasValue ? value.Value <= 0 ? 1 : value.Value : 1; }
    public bool? Ascending { get; set; }
    public string? SortBy { get; set; }
    public string? SearchTerm { get; set; }
}

public record ListResponse<T>
{
    public ListResponse(FilterQuery req, int total, List<T> items)
    {
        Size = req.Size!.Value;
        Page = req.Page!.Value;
        Total = total;
        Items = items;
        Ascending = req.Ascending ?? false;
        SortBy = req.SortBy;
    }

    [Required] public List<T> Items { get; init; }
    [Required] public int Size { get; init; }
    [Required] public int Page { get; init; }
    [Required] public int Total { get; init; }
    [Required] public bool Ascending { get; init; }
    public string? SortBy { get; init; }
}

public static class EFilterQueryExtensions
{
    internal static IQueryable<T> Paginate<T>(this IQueryable<T> query, FilterQuery request)
    {
        if (request.Page.HasValue && request.Size.HasValue)
            return query.Skip((request.Page.Value - 1) * request.Size.Value).Take(request.Size.Value);

        return query;
    }
    internal static IOrderedQueryable<T> Order<T, TKey>(this IQueryable<T> source, Expression<Func<T, TKey>> selector, bool? ascending)
    {
        return ascending.HasValue && ascending.Value ? source.OrderBy(selector) : source.OrderByDescending(selector);
    }
}