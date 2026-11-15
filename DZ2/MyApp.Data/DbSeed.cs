using Microsoft.EntityFrameworkCore;

namespace MyApp.Data;

public static class DbSeed
{
    public static void Seed(AppDbContext db)
    {
        if (db.Clients.Any() || db.Services.Any() || db.RepairRequests.Any())
            return;

        var maxim = new Client { FullName = "Максим Починков", Phone = "+7 900 000-00-01", Email = "maxim@example.com", IsVip = true };
        var anna = new Client { FullName = "Анна Белова", Phone = "+7 900 000-00-02", Email = "anna@example.com", IsVip = false };
        var sergey = new Client { FullName = "Сергей Волков", Phone = "+7 900 000-00-03", IsVip = false };
        var screen = new Service { Name = "Замена экрана", Price = 8500, DurationMinutes = 180, IsAvailable = true };
        var cleaning = new Service { Name = "Чистка ноутбука", Price = 2500, DurationMinutes = 90, IsAvailable = true };
        var diagnostics = new Service { Name = "Диагностика", Price = 1000, DurationMinutes = 45, IsAvailable = true };

        db.Clients.AddRange(maxim, anna, sergey);
        db.Services.AddRange(screen, cleaning, diagnostics);
        db.SaveChanges();

        db.RepairRequests.AddRange(
            new RepairRequest { Client = maxim, Service = screen, CreatedAt = new DateTime(2026, 11, 10), ProblemDescription = "Треснул экран смартфона", StatusCode = 2 },
            new RepairRequest { Client = maxim, Service = diagnostics, CreatedAt = new DateTime(2026, 11, 12), ProblemDescription = "Телефон быстро разряжается", StatusCode = 1 },
            new RepairRequest { Client = anna, Service = cleaning, CreatedAt = new DateTime(2026, 11, 13), ProblemDescription = "Ноутбук перегревается", StatusCode = 3 });
        db.SaveChanges();
    }
}
