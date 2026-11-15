using Microsoft.EntityFrameworkCore;
using MyApp.Data;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("FixPoint: консольный просмотр базы данных");
Console.WriteLine($"Файл базы: {AppDbContext.DbPath}");

using var db = new AppDbContext();
db.Database.EnsureCreated();
DbSeed.Seed(db);

Console.WriteLine("\nКлиенты:");
foreach (var client in db.Clients.AsNoTracking().OrderBy(c => c.Id))
    Console.WriteLine($"  {client.Id}. {client}");

Console.WriteLine("\nУслуги:");
foreach (var service in db.Services.AsNoTracking().OrderBy(s => s.Id))
    Console.WriteLine($"  {service.Id}. {service}");

Console.WriteLine("\nЗаявки на ремонт:");
var requests = db.RepairRequests
    .Include(r => r.Client)
    .Include(r => r.Service)
    .AsNoTracking()
    .OrderBy(r => r.Id)
    .ToList();
foreach (var request in requests)
    Console.WriteLine($"  {request.Id}. {request}");

Console.WriteLine($"\nВсего: клиентов {db.Clients.Count()}, услуг {db.Services.Count()}, заявок {db.RepairRequests.Count()}");
