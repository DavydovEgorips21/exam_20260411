# DbSchema

Схема спроектирована для PostgreSQL в `Database/01_schema.sql`.

Как открыть в DbSchema:

1. Создать базу и таблицы скриптами из папки `Database`.
2. В DbSchema выбрать PostgreSQL и подключиться к базе `davydov`.
3. Выполнить reverse engineer схемы `public` или открыть файл `Database/01_schema.sql` через `Start a New Project -> Open SQL File`.

Минимальные сущности из чеклиста присутствуют: `users`, `roles`, `orders`, `suppliers`, `pickup_points`.
Дополнительно добавлены таблицы для всех CSV: `products`, `order_items`, `categories`, `manufacturers`.
