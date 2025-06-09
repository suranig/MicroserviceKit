namespace MyApp.UserService.Api.Models;

/// <summary>
/// Generic paged response model
/// </summary>
/// <typeparam name="T">Type of items</typeparam>
public class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}