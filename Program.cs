using Microsoft.EntityFrameworkCore;
using System.Globalization;

Console.OutputEncoding = System.Text.Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
Console.WriteLine("Магазин спичек «Чирк»: проект запущен!");
Console.WriteLine("=== Шаг 1. Покупатели и товары ===");
using var db = new ShopContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

Console.WriteLine("=== Шаг 2. Заказы ===");
Console.WriteLine("У Orders два внешних ключа: CustomerId и ProductId.");

Console.WriteLine("=== Шаг 3. Заполнение ===");
// Учебные покупатели и вымышленные контактные данные.
var ivan = new Customer { Name = "Иван Петров", Phone = "+7 900 111-22-33", City = "Омск" };
var olga = new Customer { Name = "Ольга Смирнова", Phone = "+7 900 444-55-66", City = "Томск" };
var maxim = new Customer { Name = "Максим Починков", Phone = "+7 000 000-00-01", City = "Владивосток" };
var anna = new Customer { Name = "Анна Белова", Phone = "+7 000 000-00-02", City = "Омск" };
var sergey = new Customer { Name = "Сергей Волков", Phone = "+7 000 000-00-03", City = "Хабаровск" };
db.Customers.AddRange(ivan, olga, maxim, anna, sergey);
db.SaveChanges();

var household = new Product { Name = "Бытовые", Price = 15m, MatchesPerBox = 40, Stock = 100 };
var kitchen = new Product { Name = "Кухонные", Price = 60m, MatchesPerBox = 60, Stock = 50 };
var fireplace = new Product { Name = "Каминные", Price = 150m, MatchesPerBox = 30, Stock = 20, Description = "Длинные спички для камина" };
var storm = new Product { Name = "Штормовые", Price = 240m, MatchesPerBox = 20, Stock = 15, Description = "Горят даже под дождём" };
var souvenir = new Product { Name = "Сувенирные", Price = 350m, MatchesPerBox = 50, Stock = 0 };
db.Products.AddRange(household, kitchen, fireplace, storm, souvenir);
db.SaveChanges();

// Передаём объекты через навигационные свойства: EF сам заполнит внешние ключи.
// Это импорт прошлых заказов; Stock выше уже отражает текущий остаток после них.
var lastWeek = DateTime.UtcNow.AddDays(-7);
db.Orders.AddRange(
    new Order { Customer = ivan, Product = fireplace, Quantity = 3, CreatedAt = lastWeek },
    new Order { Customer = ivan, Product = household, Quantity = 10, CreatedAt = lastWeek.AddHours(1) },
    new Order { Customer = maxim, Product = storm, Quantity = 2, CreatedAt = lastWeek.AddDays(1) },
    new Order { Customer = anna, Product = kitchen, Quantity = 4, CreatedAt = lastWeek.AddDays(2) });
db.SaveChanges();
Console.WriteLine($"Покупателей: {db.Customers.Count()}, товаров: {db.Products.Count()}, заказов: {db.Orders.Count()}");
Console.WriteLine($"Заказов Ивана: {db.Orders.Count(o => o.CustomerId == ivan.Id)}");
Console.WriteLine($"Заказов Сергея: {db.Orders.Count(o => o.CustomerId == sergey.Id)}");
db.ChangeTracker.Clear();
