# ДЗ 2 — слой данных и консоль FixPoint

Решение состоит из двух проектов:

- `MyApp.Data` — модели `Client`, `Service`, `RepairRequest`, контекст `AppDbContext` и идемпотентное заполнение `DbSeed`;
- `MyApp.ConsoleApp` — консольная программа, которая создаёт SQLite-базу, заполняет её и выводит все три таблицы.

Запуск из каталога `DZ2`:

```bash
dotnet run --project MyApp.ConsoleApp
dotnet run --project MyApp.ConsoleApp
```

Повторный запуск не добавляет дубликаты. Заявки выводятся через `Include` с именами клиента и услуги.
