using Microsoft.EntityFrameworkCore;
using System.Globalization;

Console.OutputEncoding = System.Text.Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
Console.WriteLine("Магазин спичек «Чирк»: проект запущен!");
Console.WriteLine("=== Шаг 1. Покупатели и товары ===");
using var db = new ShopContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();
