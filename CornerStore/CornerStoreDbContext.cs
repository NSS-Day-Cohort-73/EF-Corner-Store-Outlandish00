using CornerStore.Models;
using Microsoft.EntityFrameworkCore;

public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<Product> Products { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context)
        : base(context) { }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Cashier>()
            .HasData(
                new Cashier[]
                {
                    new Cashier
                    {
                        Id = 1,
                        FirstName = "Tyler",
                        LastName = "Parker",
                    },
                    new Cashier
                    {
                        Id = 2,
                        FirstName = "Peter",
                        LastName = "Parker",
                    },
                    new Cashier
                    {
                        Id = 3,
                        FirstName = "Severus",
                        LastName = "Snake",
                    },
                    new Cashier
                    {
                        Id = 4,
                        FirstName = "Balynda",
                        LastName = " Myars",
                    },
                }
            );
        modelBuilder
            .Entity<Category>()
            .HasData(
                new Category[]
                {
                    new Category { Id = 1, CategoryName = "Soda" },
                    new Category { Id = 2, CategoryName = "Energy" },
                    new Category { Id = 3, CategoryName = "Candy" },
                    new Category { Id = 4, CategoryName = "Snacks" },
                    new Category { Id = 5, CategoryName = "Home" },
                }
            );
        modelBuilder
            .Entity<Order>()
            .HasData(
                new Order[]
                {
                    new Order
                    {
                        Id = 1,
                        CashierId = 1,
                        PaidOnDate = null,
                    },
                    new Order
                    {
                        Id = 2,
                        CashierId = 1,
                        PaidOnDate = new DateTime(2025, 1, 1),
                    },
                    new Order
                    {
                        Id = 3,
                        CashierId = 2,
                        PaidOnDate = new DateTime(2025, 1, 2),
                    },
                    new Order
                    {
                        Id = 4,
                        CashierId = 3,
                        PaidOnDate = null,
                    },
                    new Order
                    {
                        Id = 5,
                        CashierId = 4,
                        PaidOnDate = null,
                    },
                }
            );
        modelBuilder
            .Entity<Product>()
            .HasData(
                new Product[]
                {
                    new Product
                    {
                        Id = 1,
                        ProductName = "Coca-Cola",
                        Price = 2.00M,
                        Brand = "Coke",
                        CategoryId = 1,
                    },
                    new Product
                    {
                        Id = 2,
                        ProductName = "Gummy Worms",
                        Price = 3.50M,
                        Brand = "Haribo",
                        CategoryId = 3,
                    },
                    new Product
                    {
                        Id = 3,
                        ProductName = "Monster",
                        Price = 3.00M,
                        Brand = "Monster",
                        CategoryId = 2,
                    },
                    new Product
                    {
                        Id = 4,
                        ProductName = "USB Car Charger",
                        Price = 7.50M,
                        Brand = "ZapIT",
                        CategoryId = 5,
                    },
                    new Product
                    {
                        Id = 5,
                        ProductName = "M&Ms",
                        Price = 4.00M,
                        Brand = "Nestle",
                        CategoryId = 3,
                    },
                    new Product
                    {
                        Id = 6,
                        ProductName = "ChexMix",
                        Price = 3.50M,
                        Brand = "Chex",
                        CategoryId = 4,
                    },
                }
            );
        modelBuilder
            .Entity<OrderProduct>()
            .HasData(
                new OrderProduct[]
                {
                    new OrderProduct
                    {
                        Id = 1,
                        ProductId = 6,
                        OrderId = 1,
                        Quantity = 2,
                    },
                    new OrderProduct
                    {
                        Id = 2,
                        ProductId = 5,
                        OrderId = 1,
                        Quantity = 1,
                    },
                    new OrderProduct
                    {
                        Id = 3,
                        ProductId = 3,
                        OrderId = 2,
                        Quantity = 2,
                    },
                    new OrderProduct
                    {
                        Id = 4,
                        ProductId = 1,
                        OrderId = 3,
                        Quantity = 4,
                    },
                    new OrderProduct
                    {
                        Id = 5,
                        ProductId = 2,
                        OrderId = 4,
                        Quantity = 2,
                    },
                    new OrderProduct
                    {
                        Id = 6,
                        ProductId = 4,
                        OrderId = 4,
                        Quantity = 1,
                    },
                    new OrderProduct
                    {
                        Id = 7,
                        ProductId = 5,
                        OrderId = 5,
                        Quantity = 4,
                    },
                    new OrderProduct
                    {
                        Id = 8,
                        ProductId = 2,
                        OrderId = 5,
                        Quantity = 2,
                    },
                }
            );
    }
}
