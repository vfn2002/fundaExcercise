namespace Core.Models
{
    public class SourceResult
    {
        public SourceResultObject[] Objects { get; set; }
        public PagingInformation Paging { get; set; }
        public int TotaalAantalObjecten { get; set; }
    }

    public class SourceResultObject {
        public int MakelaarId { get; set; }
        public string MakelaarNaam { get; set; }
    }

    public class PagingInformation {
        public int AantalPaginas { get; set; }
        public int HuidigePagina { get; set; }
    }
}