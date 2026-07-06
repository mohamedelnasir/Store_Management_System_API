using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Models;

namespace StoreManagementSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- User ----
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
        });

        // ---- Category ----
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });

        // ---- Product ----
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.HasIndex(p => p.Barcode);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Sku).HasMaxLength(50).IsRequired();
            entity.Property(p => p.BuyPrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.SellPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- InventoryTransaction ----
        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasOne(t => t.Product)
                  .WithMany(p => p.InventoryTransactions)
                  .HasForeignKey(t => t.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.RelatedSale)
                  .WithMany()
                  .HasForeignKey(t => t.RelatedSaleId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(t => t.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Sale ----
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(s => s.InvoiceNumber).IsUnique();
            entity.Property(s => s.Total).HasColumnType("decimal(18,2)");

            entity.HasOne(s => s.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(s => s.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- SaleItem ----
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.Property(si => si.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(si => si.LineTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(si => si.Sale)
                  .WithMany(s => s.SaleItems)
                  .HasForeignKey(si => si.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(si => si.Product)
                  .WithMany(p => p.SaleItems)
                  .HasForeignKey(si => si.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Expense ----
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Employee ----
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
        });

        // ---- Payroll ----
        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.Property(p => p.BaseSalary).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Bonus).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Deduction).HasColumnType("decimal(18,2)");
            entity.Property(p => p.NetSalary).HasColumnType("decimal(18,2)");

            entity.HasOne(p => p.Employee)
                  .WithMany(e => e.Payrolls)
                  .HasForeignKey(p => p.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
