using Microsoft.EntityFrameworkCore;

namespace Core.Models
{
    public class SourceResultContext : DbContext
    {
        public SourceResultContext(DbContextOptions<SourceResultContext> options)
            : base(options)
        {
            
        }
        
        public DbSet<SourceResult> SourceResults { get; set; }
    }
}