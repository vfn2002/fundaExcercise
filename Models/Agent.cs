using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class Agent
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Listings { get; set; }
        public ListingType ListingType { get; set; }
    }

    public enum ListingType
    {
        Garden,
        All
    }
}