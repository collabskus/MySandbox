using Microsoft.EntityFrameworkCore;

namespace MySandbox.ClassLibrary;

// DbContext
public class StuffDbContext(DbContextOptions<StuffDbContext> options) : DbContext(options)
{
    public DbSet<StuffItem> StuffItems => Set<StuffItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StuffItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500); // Prevent unbounded string attacks
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.HasIndex(e => e.CreatedAt); // Index for performance
        });
    }
}

