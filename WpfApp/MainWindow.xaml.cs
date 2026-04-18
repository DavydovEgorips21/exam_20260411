using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using pgDataAccess;
using pgDataAccess.Models;
using UserModel = pgDataAccess.Models.User;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private ExamDbContext? _db;
        private ObservableCollection<Order> _orders = new();
        private ICollectionView? _ordersView;
        private List<UserModel> _users = new();
        private List<PickupPoint> _pickupPoints = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            _db?.Dispose();
        }

        private async Task LoadDataAsync()
        {
            SetStatus("Loading data from PostgreSQL...");

            try
            {
                _db?.Dispose();
                _db = new ExamDbContext();

                if (!await _db.Database.CanConnectAsync())
                {
                    SetStatus("PostgreSQL database is unavailable.");
                    return;
                }

                var roles = await _db.Roles.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
                _users = await _db.Users.AsNoTracking()
                    .Include(x => x.Role)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
                var suppliers = await _db.Suppliers.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
                _pickupPoints = await _db.PickupPoints.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
                var categories = await _db.Categories.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
                var manufacturers = await _db.Manufacturers.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
                var products = await _db.Products.AsNoTracking()
                    .Include(x => x.Category)
                    .Include(x => x.Manufacturer)
                    .Include(x => x.Supplier)
                    .OrderBy(x => x.Article)
                    .ToListAsync();
                var orderItems = await _db.OrderItems.AsNoTracking()
                    .Include(x => x.Product)
                    .OrderBy(x => x.OrderId)
                    .ThenBy(x => x.ProductArticle)
                    .ToListAsync();

                _orders = new ObservableCollection<Order>(
                    await _db.Orders.OrderBy(x => x.Id).ToListAsync());

                BuildTabs(roles, suppliers, categories, manufacturers, products, orderItems);
                SetStatus($"Loaded: {_orders.Count} orders, {products.Count} products, {_users.Count} users.");
            }
            catch (Exception ex)
            {
                SetStatus("Loading failed.");
                MessageBox.Show(ex.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuildTabs(
            List<Role> roles,
            List<Supplier> suppliers,
            List<Category> categories,
            List<Manufacturer> manufacturers,
            List<Product> products,
            List<OrderItem> orderItems)
        {
            DataTabs.Items.Clear();
            DataTabs.Items.Add(CreateOrdersTab());
            DataTabs.Items.Add(CreateReadOnlyTab("Users", _users.Select(x => new
            {
                x.Id,
                x.Login,
                x.FullName,
                Role = x.Role?.Name ?? string.Empty
            }).ToList()));
            DataTabs.Items.Add(CreateReadOnlyTab("Roles", roles.Select(x => new { x.Id, x.Name }).ToList()));
            DataTabs.Items.Add(CreateReadOnlyTab("Suppliers", suppliers.Select(x => new { x.Id, x.Name }).ToList()));
            DataTabs.Items.Add(CreateReadOnlyTab("Pickup points", _pickupPoints.Select(x => new { x.Id, x.Address }).ToList()));
            DataTabs.Items.Add(CreateReadOnlyTab("Categories", categories.Select(x => new { x.Id, x.Name }).ToList()));
            DataTabs.Items.Add(CreateReadOnlyTab("Manufacturers", manufacturers.Select(x => new { x.Id, x.Name }).ToList()));
            DataTabs.Items.Add(CreateReadOnlyTab("Products", products.Select(x => new
            {
                x.Article,
                x.Name,
                x.Unit,
                x.Price,
                Category = x.Category?.Name ?? string.Empty,
                Manufacturer = x.Manufacturer?.Name ?? string.Empty,
                Supplier = x.Supplier?.Name ?? string.Empty,
                x.DiscountPercent,
                x.StockQuantity,
                x.Description,
                x.PhotoPath
            }).ToList()));
            DataTabs.Items.Add(CreateReadOnlyTab("Order items", orderItems.Select(x => new
            {
                x.OrderId,
                x.ProductArticle,
                Product = x.Product?.Name ?? string.Empty,
                x.Quantity
            }).ToList()));
        }

        private TabItem CreateOrdersTab()
        {
            var root = new DockPanel();

            var toolbar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };
            DockPanel.SetDock(toolbar, Dock.Top);

            var filterBox = new TextBox
            {
                Width = 280,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = "Filter orders"
            };

            toolbar.Children.Add(new TextBlock
            {
                Text = "Filter:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0)
            });
            toolbar.Children.Add(filterBox);
            toolbar.Children.Add(CreateButton("Add order", AddOrder_Click));
            toolbar.Children.Add(CreateButton("Save", SaveOrders_Click));
            toolbar.Children.Add(CreateButton("Delete", DeleteOrder_Click));
            toolbar.Children.Add(CreateButton("Reload", Reload_Click));

            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                Margin = new Thickness(0),
                SelectionMode = DataGridSelectionMode.Single,
                IsReadOnly = false
            };

            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Id",
                Binding = new Binding(nameof(Order.Id)),
                IsReadOnly = true,
                Width = 70
            });
            grid.Columns.Add(new DataGridComboBoxColumn
            {
                Header = "Client",
                ItemsSource = _users,
                DisplayMemberPath = nameof(UserModel.FullName),
                SelectedValuePath = nameof(UserModel.Id),
                SelectedValueBinding = new Binding(nameof(Order.ClientId)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
                Width = 220
            });
            grid.Columns.Add(new DataGridComboBoxColumn
            {
                Header = "Pickup point",
                ItemsSource = _pickupPoints,
                DisplayMemberPath = nameof(PickupPoint.Address),
                SelectedValuePath = nameof(PickupPoint.Id),
                SelectedValueBinding = new Binding(nameof(Order.PickupPointId)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
                Width = 260
            });
            grid.Columns.Add(CreateDateColumn("Order date", nameof(Order.OrderDate)));
            grid.Columns.Add(CreateDateColumn("Delivery date", nameof(Order.DeliveryDate)));
            grid.Columns.Add(new DataGridComboBoxColumn
            {
                Header = "Status",
                ItemsSource = GetStatuses(),
                SelectedItemBinding = new Binding(nameof(Order.Status)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
                Width = 130
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Pickup code",
                Binding = new Binding(nameof(Order.PickupCode)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
                Width = 120
            });

            _ordersView = CollectionViewSource.GetDefaultView(_orders);
            _ordersView.Filter = item => MatchesFilter(item, filterBox.Text);
            filterBox.TextChanged += (_, _) => _ordersView.Refresh();
            grid.ItemsSource = _ordersView;

            root.Children.Add(toolbar);
            root.Children.Add(grid);

            return new TabItem { Header = "Orders CRUD", Content = root };
        }

        private static DataGridTemplateColumn CreateDateColumn(string header, string path)
        {
            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetBinding(TextBlock.TextProperty, new Binding(path) { StringFormat = "yyyy-MM-dd" });

            var datePicker = new FrameworkElementFactory(typeof(DatePicker));
            datePicker.SetBinding(DatePicker.SelectedDateProperty, new Binding(path)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            return new DataGridTemplateColumn
            {
                Header = header,
                CellTemplate = new DataTemplate { VisualTree = text },
                CellEditingTemplate = new DataTemplate { VisualTree = datePicker },
                Width = 130
            };
        }

        private TabItem CreateReadOnlyTab(string title, IEnumerable rows)
        {
            var root = new DockPanel();

            var toolbar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };
            DockPanel.SetDock(toolbar, Dock.Top);

            var filterBox = new TextBox
            {
                Width = 280,
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = $"Filter {title}"
            };

            toolbar.Children.Add(new TextBlock
            {
                Text = "Filter:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0)
            });
            toolbar.Children.Add(filterBox);

            var grid = new DataGrid
            {
                AutoGenerateColumns = true,
                IsReadOnly = true,
                CanUserAddRows = false,
                CanUserDeleteRows = false
            };

            var view = CollectionViewSource.GetDefaultView(rows);
            view.Filter = item => MatchesFilter(item, filterBox.Text);
            filterBox.TextChanged += (_, _) => view.Refresh();
            grid.ItemsSource = view;

            root.Children.Add(toolbar);
            root.Children.Add(grid);

            return new TabItem { Header = title, Content = root };
        }

        private static Button CreateButton(string text, RoutedEventHandler handler)
        {
            var button = new Button
            {
                Content = text,
                MinWidth = 90,
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(8, 4, 8, 4)
            };

            button.Click += handler;
            return button;
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_db is null || _users.Count == 0 || _pickupPoints.Count == 0)
            {
                return;
            }

            var order = new Order
            {
                Id = _orders.Count == 0 ? 1 : _orders.Max(x => x.Id) + 1,
                ClientId = _users[0].Id,
                PickupPointId = _pickupPoints[0].Id,
                OrderDate = DateTime.Today,
                DeliveryDate = DateTime.Today.AddDays(7),
                Status = GetStatuses().FirstOrDefault() ?? "New",
                PickupCode = GeneratePickupCode()
            };

            _db.Orders.Add(order);
            _orders.Add(order);
            _ordersView?.Refresh();
            SetStatus("New order added. Press Save to write it to PostgreSQL.");
        }

        private async void SaveOrders_Click(object sender, RoutedEventArgs e)
        {
            if (_db is null)
            {
                return;
            }

            try
            {
                await _db.SaveChangesAsync();
                SetStatus("Orders saved.");
            }
            catch (Exception ex)
            {
                SetStatus("Save failed.");
                MessageBox.Show(ex.Message, "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_db is null || DataTabs.SelectedItem is not TabItem { Content: DockPanel root })
            {
                return;
            }

            var grid = root.Children.OfType<DataGrid>().FirstOrDefault();
            if (grid?.SelectedItem is not Order order)
            {
                return;
            }

            if (MessageBox.Show($"Delete order #{order.Id}?", "Delete order", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _db.Orders.Remove(order);
                _orders.Remove(order);
                await _db.SaveChangesAsync();
                _ordersView?.Refresh();
                SetStatus("Order deleted.");
            }
            catch (Exception ex)
            {
                SetStatus("Delete failed.");
                MessageBox.Show(ex.Message, "Delete error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Reload_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private List<string> GetStatuses()
        {
            return _orders
                .Select(x => x.Status)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }

        private string GeneratePickupCode()
        {
            var usedCodes = _orders.Select(x => x.PickupCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            for (var code = 900; code <= 999; code++)
            {
                var value = code.ToString(CultureInfo.InvariantCulture);
                if (!usedCodes.Contains(value))
                {
                    return value;
                }
            }

            return DateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture);
        }

        private static bool MatchesFilter(object? item, string filter)
        {
            if (item is null || string.IsNullOrWhiteSpace(filter))
            {
                return true;
            }

            return item.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.GetIndexParameters().Length == 0 && IsSimpleType(x.PropertyType))
                .Select(x => x.GetValue(item)?.ToString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Any(x => x!.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsSimpleType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(Guid);
        }

        private void SetStatus(string text)
        {
            StatusText.Text = text;
        }
    }
}
