# exam_20260411

Учебное WPF-приложение

## Что сделано

- Спроектирована PostgreSQL-схема для CSV из `Docs/imports`.
- Минимальные таблицы из чеклиста: `users`, `roles`, `orders`, `suppliers`, `pickup_points`.
- Дополнительные таблицы для целостного импорта: `products`, `order_items`, `categories`, `manufacturers`.
- Импорт CSV выполняется через `psql \copy`, без ручного ввода данных.
- В `pgDataAccess` добавлены EF Core модели и `ExamDbContext`.
- В WPF добавлен простой `TabControl` + `DataGrid`: просмотр всех сущностей, фильтрация, CRUD для заказов.

## Требования

- .NET SDK 9
- PostgreSQL 16
- Visual Studio 2022 или `dotnet` CLI
- DbSchema для визуального просмотра схемы

## База данных

Строка подключения по умолчанию:

```text
Host=localhost;Port=5432;Database=davydov;Username=app;Password=123456789
```

Ее можно переопределить переменной окружения:

```powershell
$env:EXAM_DB_CONNECTION="Host=localhost;Port=5432;Database=davydov;Username=app;Password=123456789"
```

Создание базы и импорт CSV из корня репозитория:

```powershell
$env:PGPASSWORD="admin"
& "C:\Program Files\PostgreSQL\16\bin\psql.exe" -h 127.0.0.1 -U postgres -d postgres -f Database\00_create_database.sql
& "C:\Program Files\PostgreSQL\16\bin\psql.exe" -h 127.0.0.1 -U postgres -d davydov -f Database\01_schema.sql
& "C:\Program Files\PostgreSQL\16\bin\psql.exe" -h 127.0.0.1 -U postgres -d davydov -f Database\02_import.sql
& "C:\Program Files\PostgreSQL\16\bin\psql.exe" -h 127.0.0.1 -U postgres -d davydov -f Database\03_verify_counts.sql
```

Если пароль пользователя `postgres` другой, замените значение `PGPASSWORD`.

## Запуск

```powershell
dotnet build WpfApp\WpfApp.csproj
dotnet run --project WpfApp\WpfApp.csproj
```

## DbSchema

Для DbSchema используйте подключение к базе `davydov` и reverse engineer схемы `public`.
Также можно открыть `Database/01_schema.sql` через `Start a New Project -> Open SQL File`.
