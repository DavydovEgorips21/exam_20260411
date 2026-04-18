\set ON_ERROR_STOP on
\encoding UTF8

TRUNCATE TABLE order_items, orders, products, users, roles, suppliers, pickup_points, manufacturers, categories
RESTART IDENTITY CASCADE;

\copy roles(id, name) FROM 'Docs/imports/roles.csv' WITH (FORMAT csv, HEADER true)
\copy suppliers(id, name) FROM 'Docs/imports/suppliers.csv' WITH (FORMAT csv, HEADER true)
\copy categories(id, name) FROM 'Docs/imports/categories.csv' WITH (FORMAT csv, HEADER true)
\copy manufacturers(id, name) FROM 'Docs/imports/manufacturers.csv' WITH (FORMAT csv, HEADER true)
\copy pickup_points(id, address) FROM 'Docs/imports/pickup_points.csv' WITH (FORMAT csv, HEADER true)
\copy users(id, login, password_hash, full_name, role_id) FROM 'Docs/imports/users.csv' WITH (FORMAT csv, HEADER true)
\copy products(article, name, unit, price, category_id, manufacturer_id, supplier_id, discount_percent, stock_quantity, description, photo_path) FROM 'Docs/imports/products.csv' WITH (FORMAT csv, HEADER true)
\copy orders(id, client_id, pickup_point_id, order_date, delivery_date, status, pickup_code) FROM 'Docs/imports/orders.csv' WITH (FORMAT csv, HEADER true)
\copy order_items(order_id, product_article, quantity) FROM 'Docs/imports/order_items.csv' WITH (FORMAT csv, HEADER true)

SELECT setval(pg_get_serial_sequence('roles', 'id'), COALESCE((SELECT MAX(id) FROM roles), 1), true);
SELECT setval(pg_get_serial_sequence('suppliers', 'id'), COALESCE((SELECT MAX(id) FROM suppliers), 1), true);
SELECT setval(pg_get_serial_sequence('categories', 'id'), COALESCE((SELECT MAX(id) FROM categories), 1), true);
SELECT setval(pg_get_serial_sequence('manufacturers', 'id'), COALESCE((SELECT MAX(id) FROM manufacturers), 1), true);
SELECT setval(pg_get_serial_sequence('pickup_points', 'id'), COALESCE((SELECT MAX(id) FROM pickup_points), 1), true);
SELECT setval(pg_get_serial_sequence('users', 'id'), COALESCE((SELECT MAX(id) FROM users), 1), true);
SELECT setval(pg_get_serial_sequence('orders', 'id'), COALESCE((SELECT MAX(id) FROM orders), 1), true);
