using Microsoft.EntityFrameworkCore;
using Database.Tables;

namespace Offers.Database;

public class OffersContext : DbContext
{
    public OffersContext(DbContextOptions<OffersContext> options) : base(options) { }
    public OffersContext() { }
    public DbSet<Trip> Trips { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trip>().ToTable("Trip"); // table name overwrite (removing the plural "s")
    }
}