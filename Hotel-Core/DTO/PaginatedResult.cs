namespace Hotel_Core.DTO;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; }
    
    public int TotalCount { get; set; }
    
    public int CurrentPage { get; set; }
    
    public int PageSize { get; set; }
    
    public bool HasNextPage => CurrentPage * PageSize < TotalCount;
    
    public bool HasPreviousPage => CurrentPage > 1;
}
