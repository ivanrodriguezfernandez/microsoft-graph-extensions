using System.Collections.Generic;

namespace microsoft_graph_extensions.Pagination
{
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Total { get; set; }
    }
}