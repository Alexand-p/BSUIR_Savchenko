using BSUIR_Savchenko.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BSUIR_Savchenko.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasIndex(category => category.NormalizedName)
            .IsUnique();

        modelBuilder.Entity<Book>()
            .HasOne(book => book.Category)
            .WithMany(category => category.Books)
            .HasForeignKey(book => book.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Book>()
            .Property(book => book.Price)
            .HasPrecision(10, 2);
    }
}
