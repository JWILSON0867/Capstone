using Microsoft.EntityFrameworkCore;

namespace VideoGameCompanyStockPrices
{
    // Step 1: Inherit from DbContext
    public class ProductContext : DbContext
    {
        // Step 2: Define DbSet for your ProductEntity
        public DbSet<ProductEntity> Products { get; set; }

        // Step 3: Override OnConfiguring to use SQLite
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Specify the SQLite database path
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "products.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
