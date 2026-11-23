
using Microsoft.EntityFrameworkCore;

namespace Roshetta.API.Models.DTOs
{
    public class PagedList<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }


        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber * PageSize < TotalCount;
        public PagedList(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        

    }
}
