using Microsoft.EntityFrameworkCore;
using pgDataAccess.Models;

namespace pgDataAccess;

public class ExamDbContext : DbContext
{
    public const string ConnectionStringEnvironmentVariable = "EXAM_DB_CONNECTION";

    public static string DefaultConnectionString =>
        Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
        ?? "Host=localhost;Port=5432;Database=davydov;Username=app;Password=123456789";

    public ExamDbContext()
    {
    }

    public ExamDbContext(DbContextOptions<ExamDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<PickupPoint> PickupPoints => Set<PickupPoint>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(DefaultConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login).HasColumnName("login").HasMaxLength(150).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(250).IsRequired();
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.HasIndex(e => e.Login).IsUnique();
            entity.HasOne(e => e.Role)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<PickupPoint>(entity =>
        {
            entity.ToTable("pickup_points");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(300).IsRequired();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.ToTable("manufacturers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Article);
            entity.Property(e => e.Article).HasColumnName("article").HasMaxLength(20);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(12, 2);
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PhotoPath).HasColumnName("photo_path").HasMaxLength(260);
            entity.HasOne(e => e.Category)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Manufacturer)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.PickupPointId).HasColumnName("pickup_point_id");
            entity.Property(e => e.OrderDate).HasColumnName("order_date").HasColumnType("date");
            entity.Property(e => e.DeliveryDate).HasColumnName("delivery_date").HasColumnType("date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PickupCode).HasColumnName("pickup_code").HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.PickupCode).IsUnique();
            entity.HasOne(e => e.Client)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PickupPoint)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.PickupPointId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => new { e.OrderId, e.ProductArticle });
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductArticle).HasColumnName("product_article").HasMaxLength(20);
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.HasOne(e => e.Order)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.ProductArticle)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
