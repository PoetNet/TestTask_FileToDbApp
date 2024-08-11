using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class FileToDbAppDbContext : DbContext
{
    public FileToDbAppDbContext(DbContextOptions<FileToDbAppDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Word> Words => Set<Word>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        modelBuilder.Entity<Word>()
            .Property(w => w.Text)
            .HasColumnName("Text");

        modelBuilder.Entity<Word>()
            .Property(w => w.Occurrences)
            .HasColumnName("Occurrences");

        modelBuilder.Entity<Word>()
            .HasKey(w => w.Id);

        base.OnModelCreating(modelBuilder);
    }
}
