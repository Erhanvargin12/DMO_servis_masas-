namespace WebUI.Models
{
    /// <summary>
    /// Generic sayfalı sonuç modeli. Tüm listeleme controller'ları için kullanılır.
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>Mevcut sayfanın kayıtları.</summary>
        public List<T> Items { get; set; } = new();

        /// <summary>Filtreleme sonrası toplam kayıt sayısı.</summary>
        public int TotalCount { get; set; }

        /// <summary>Mevcut sayfa numarası (1'den başlar).</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>Sayfa başına kayıt sayısı.</summary>
        public int PageSize { get; set; } = 10;

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Bellekteki bir koleksiyondan sayfalı sonuç oluşturur.
        /// Tüm kayıtlar controller'da API'den çekilir, sayfalama burada yapılır.
        /// </summary>
        public static PagedResult<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var list = source.ToList();
            var page = Math.Max(1, pageNumber);

            return new PagedResult<T>
            {
                Items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = list.Count,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }

    /// <summary>
    /// _Pagination.cshtml partial view'ına iletilen sayfalama render modeli.
    /// Her listeleme view model'ı bu modeli içerebilir ya da ViewData üzerinden aktarabilir.
    /// </summary>
    public class PaginationModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>Pagination linklerinde kullanılacak action adı.</summary>
        public string Action { get; set; } = "Index";

        /// <summary>Pagination linklerinde kullanılacak controller adı.</summary>
        public string Controller { get; set; } = "";

        /// <summary>Aktif arama terimi — pagination linklerine eklenir.</summary>
        public string? Search { get; set; }

        /// <summary>List view model'dan doğrudan oluşturur.</summary>
        public static PaginationModel From(
            int pageNumber, int pageSize, int totalCount,
            string controller, string action = "Index", string? search = null)
        {
            return new PaginationModel
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Controller = controller,
                Action = action,
                Search = search
            };
        }
    }
}

