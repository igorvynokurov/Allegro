using Microsoft.EntityFrameworkCore;
using AllegroSearchService.Data.Entity;

namespace AllegroSearchService.Data.Config
{
    public class SSDbContext : DbContext
    {
        public DbSet<TokenInfo> Tokens { get; set; }
        public DbSet<TranslateItem> Translations { get; set; }

        public SSDbContext(DbContextOptions<SSDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.ConfigureIntegrationEvents();

            base.OnModelCreating(modelBuilder);
        }
    }
}
