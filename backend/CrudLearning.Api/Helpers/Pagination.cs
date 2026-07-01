namespace CrudLearning.Api.Helpers;

public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public static class Pagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static int NormalizePage(int page) => Math.Max(DefaultPage, page);

    public static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
        {
            return DefaultPageSize;
        }

        return Math.Min(pageSize, MaxPageSize);
    }

    public static int CountTotalPages(int totalItems, int pageSize)
    {
        if (totalItems == 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(totalItems / (double)pageSize);
    }
}
