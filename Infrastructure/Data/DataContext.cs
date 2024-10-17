using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class DataContext : IdentityDbContext<ApplicationUser>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    public DbSet<FuelLog> FuelLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseIdentityColumns();
        
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.FuelLogs)
            .WithOne()
            .HasForeignKey(fl => fl.ApplicationUserId)
            .IsRequired();
            
        base.OnModelCreating(builder);
    }
}