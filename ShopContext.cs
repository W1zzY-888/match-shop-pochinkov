using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ShopContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Отдельная учебная база: Program.cs пересоздаёт её при каждом запуске.
        options.UseNpgsql("Host=localhost;Database=match_shop;Username=postgres;Password=postgres");
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
}
