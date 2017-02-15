using Microsoft.EntityFrameworkCore;

namespace FlipDotServer.Models
{
    public class ScoreboardContext : DbContext
    {
        public ScoreboardContext(DbContextOptions<ScoreboardContext> options)
            : base(options)
        {
        }

        public DbSet<ScoreboardValue> ScoreboardValues { get; set; }
    }
}
