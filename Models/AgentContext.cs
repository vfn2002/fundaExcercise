using Microsoft.EntityFrameworkCore;

namespace Core.Models
{
    public class AgentContext : DbContext
    {
        public AgentContext(DbContextOptions<AgentContext> options)
            : base(options)
        {
            
        }
        
        public DbSet<Agent> Agents { get; set; }
    }
}