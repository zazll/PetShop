using Microsoft.EntityFrameworkCore;
using PetShopApp.Models;

namespace PetShopApp.Data;

public class PetShopContext : DbContext
{
    public PetShopContext()
    {
    }

    public PetShopContext(DbContextOptions<PetShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; } = null!;
    public virtual DbSet<AppUser> AppUsers { get; set; } = null!;
    public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;
    public virtual DbSet<AnimalType> AnimalTypes { get; set; } = null!;
    public virtual DbSet<Manufacturer> Manufacturers { get; set; } = null!;
    public virtual DbSet<Supplier> Suppliers { get; set; } = null!;
    public virtual DbSet<Product> Products { get; set; } = null!;
    public virtual DbSet<PickupPoint> PickupPoints { get; set; } = null!;
    public virtual DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
    public virtual DbSet<OrderHeader> OrderHeaders { get; set; } = null!;
    public virtual DbSet<OrderProduct> OrderProducts { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=94.156.170.23,1433;Database=PetShopDB;User Id=sa;Password=DimpYTYT98!;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderProduct>(entity =>
        {
            entity.HasKey(e => new { e.OrderID, e.ProductID });
        });
        
        // Additional configuration if needed to match SQL exactly
        modelBuilder.Entity<AppUser>().ToTable("AppUser");
        modelBuilder.Entity<Role>().ToTable("Role");
        modelBuilder.Entity<Product>().ToTable("Product");
        modelBuilder.Entity<ProductCategory>().ToTable("ProductCategory");
        modelBuilder.Entity<AnimalType>().ToTable("AnimalType");
        modelBuilder.Entity<Manufacturer>().ToTable("Manufacturer");
        modelBuilder.Entity<Supplier>().ToTable("Supplier");
        modelBuilder.Entity<PickupPoint>().ToTable("PickupPoint");
        modelBuilder.Entity<OrderStatus>().ToTable("OrderStatus");
        modelBuilder.Entity<OrderHeader>().ToTable("OrderHeader");
        modelBuilder.Entity<OrderProduct>().ToTable("OrderProduct");
    }
}
