using System.Text.Json.Serialization;
using CornerStore.Models;
using CornerStore.Models.DTOs;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// allows passing datetimes without time zone data
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(
    builder.Configuration["CornerStoreDbConnectionString"] ?? "testing"
);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here
app.MapGet(
    "/cashiers/{id}",
    (CornerStoreDbContext db, int id) =>
    {
        CashierDTO cashier = db
            .Cashiers.Where(c => c.Id == id)
            .Include(c => c.Orders)
            .ThenInclude(o => o.OrderProducts)
            .Select(c => new CashierDTO
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Orders = c
                    .Orders.Select(o => new OrderDTO
                    {
                        Id = o.Id,
                        CashierId = o.CashierId,
                        OrderProducts = o
                            .OrderProducts.Select(op => new OrderProductDTO
                            {
                                Id = op.Id,
                                ProductId = op.ProductId,
                                Product = new ProductDTO
                                {
                                    Id = op.Product.Id,
                                    ProductName = op.Product.ProductName,
                                    Price = op.Product.Price,
                                    Brand = op.Product.Brand,
                                    CategoryId = op.Product.CategoryId,
                                },
                                OrderId = op.OrderId,
                                Quantity = op.Quantity,
                            })
                            .ToList(),
                    })
                    .ToList(),
            })
            .SingleOrDefault();

        return Results.Ok(cashier);
    }
);

app.MapPost(
    "/cashiers",
    (CornerStoreDbContext db, Cashier newCashier) =>
    {
        db.Cashiers.Add(newCashier);
        db.SaveChanges();
        return Results.Created($"/cashiers/{newCashier.Id}", newCashier);
    }
);

//gets all products if no search term is given, if a search term is given return products that include the search in their name or category

app.MapGet(
    "/products",
    (string? search, CornerStoreDbContext db) =>
    {
        if (search != null)
        {
            string LowerCaseSearch = search.ToLower();
            List<ProductDTO> foundProducts = db
                .Products.Where(p =>
                    p.ProductName.ToLower().Contains(LowerCaseSearch)
                    || p.Category.CategoryName.ToLower().Contains(LowerCaseSearch)
                )
                .Include(p => p.Category)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Brand = p.Brand,
                    CategoryId = p.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = p.Category.Id,
                        CategoryName = p.Category.CategoryName,
                    },
                })
                .ToList();
            return Results.Ok(foundProducts);
        }
        List<ProductDTO> products = db
            .Products.Include(p => p.Category)
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Price = p.Price,
                Brand = p.Brand,
                CategoryId = p.CategoryId,
                Category = new CategoryDTO
                {
                    Id = p.Category.Id,
                    CategoryName = p.Category.CategoryName,
                },
            })
            .ToList();
        return Results.Ok(products);
    }
);

// add a product
app.MapPost(
    "/products",
    (CornerStoreDbContext db, Product newProduct) =>
    {
        db.Products.Add(newProduct);
        db.SaveChanges();
        return Results.Created($"/products/{newProduct.Id}", newProduct);
    }
);

//Update a product

app.MapPut(
    "/products/{id}",
    (int id, CornerStoreDbContext db, Product newProduct) =>
    {
        Product productToUpdate = db.Products.SingleOrDefault(p => p.Id == id);
        if (productToUpdate == null)
        {
            return Results.NotFound();
        }
        if (newProduct.ProductName != null)
        {
            productToUpdate.ProductName = newProduct.ProductName;
        }
        if (newProduct.Price != null || newProduct.Price != 0)
        {
            productToUpdate.Price = newProduct.Price;
        }
        if (newProduct.Brand != null)
        {
            productToUpdate.Brand = newProduct.Brand;
        }
        if (newProduct.CategoryId != null || newProduct.CategoryId != 0)
        {
            productToUpdate.CategoryId = newProduct.CategoryId;
        }
        db.SaveChanges();
        return Results.NoContent();
    }
);

//Get an order details, including the cashier, order products, and products on the order with their category.

app.MapGet(
    "/orders/{id}",
    (int id, CornerStoreDbContext db) =>
    {
        OrderDTO order = db
            .Orders.Where(o => o.Id == id)
            .Include(o => o.Cashier)
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .Select(o => new OrderDTO
            {
                Id = o.Id,
                CashierId = o.CashierId,
                Cashier = new CashierDTO
                {
                    Id = o.Cashier.Id,
                    FirstName = o.Cashier.FirstName,
                    LastName = o.Cashier.LastName,
                },
                OrderProducts = o
                    .OrderProducts.Select(op => new OrderProductDTO
                    {
                        Id = op.Id,
                        ProductId = op.ProductId,
                        Product = new ProductDTO
                        {
                            Id = op.Product.Id,
                            ProductName = op.Product.ProductName,
                            Price = op.Product.Price,
                            Brand = op.Product.Brand,
                            CategoryId = op.Product.CategoryId,
                        },
                        Quantity = op.Quantity,
                    })
                    .ToList(),
                PaidOnDate = o.PaidOnDate,
            })
            .SingleOrDefault();
        if (order == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(order);
    }
);

//Get all orders. Check for a query string param orderDate that only returns orders from a particular day. If it is not present, return all orders.
app.MapGet(
    "/orders",
    (CornerStoreDbContext db, DateTime? orderDate) =>
    {
        if (orderDate != null)
        {
            List<OrderDTO> foundOrders = db
                .Orders.Where(o => o.PaidOnDate == orderDate)
                .Include(o => o.Cashier)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Select(o => new OrderDTO
                {
                    Id = o.Id,
                    CashierId = o.CashierId,
                    Cashier = new CashierDTO
                    {
                        Id = o.Cashier.Id,
                        FirstName = o.Cashier.FirstName,
                        LastName = o.Cashier.LastName,
                    },
                    OrderProducts = o
                        .OrderProducts.Select(op => new OrderProductDTO
                        {
                            Id = op.Id,
                            ProductId = op.ProductId,
                            Product = new ProductDTO
                            {
                                Id = op.Product.Id,
                                ProductName = op.Product.ProductName,
                                Price = op.Product.Price,
                                Brand = op.Product.Brand,
                                CategoryId = op.Product.CategoryId,
                            },
                        })
                        .ToList(),
                })
                .ToList();
            return Results.Ok(foundOrders);
        }
        List<OrderDTO> orders = db
            .Orders.Include(o => o.Cashier)
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .Select(o => new OrderDTO
            {
                Id = o.Id,
                CashierId = o.CashierId,
                Cashier = new CashierDTO
                {
                    Id = o.Cashier.Id,
                    FirstName = o.Cashier.FirstName,
                    LastName = o.Cashier.LastName,
                },
                OrderProducts = o
                    .OrderProducts.Select(op => new OrderProductDTO
                    {
                        Id = op.Id,
                        ProductId = op.ProductId,
                        Product = new ProductDTO
                        {
                            Id = op.Product.Id,
                            ProductName = op.Product.ProductName,
                            Price = op.Product.Price,
                            Brand = op.Product.Brand,
                            CategoryId = op.Product.CategoryId,
                        },
                    })
                    .ToList(),
            })
            .ToList();
        return Results.Ok(orders);
    }
);

//Delete an order
app.MapDelete(
    "/orders/{id}",
    (int id, CornerStoreDbContext db) =>
    {
        var foundOrder = db.Orders.Where(o => o.Id == id).SingleOrDefault();
        db.Remove(foundOrder);
        db.SaveChanges();
        return Results.NoContent();
    }
);

// Create an Order (with products!)
app.MapPost(
    "/orders",
    (CornerStoreDbContext db, Order newOrder) =>
    {
        Order order = new Order
        {
            Id = newOrder.Id,
            CashierId = newOrder.CashierId,
            OrderProducts = newOrder
                .OrderProducts.Select(op =>
                {
                    var product = db
                        .Products.Include(p => p.Category)
                        .SingleOrDefault(p => p.Id == op.ProductId);
                    return new OrderProduct
                    {
                        Id = op.Id,
                        ProductId = op.ProductId,
                        Product = product,
                        OrderId = op.OrderId,
                        Quantity = op.Quantity,
                    };
                })
                .ToList(),
            PaidOnDate = newOrder.PaidOnDate,
        };
        db.Orders.Add(order);
        db.SaveChanges();
        return Results.Created($"/orders/{order.Id}", order);
    }
);

// /products/popular - Get the most popular products, determined by which products have been ordered the most times (HINT: this requires using GroupBy to group the OrderProducts by ProductId, then using Sum to add up all the Quantities of the OrderProducts in each group). Check for a query string param called amount that says how many products to return. Return five by default.

app.MapGet(
    "/products/popular",
    (CornerStoreDbContext db, int? amount) =>
    {
        List<ProductWithTotalDTO> foundProducts = db
            .OrderProducts.Include(op => op.Product)
            .GroupBy(op => op.ProductId)
            .Select(group => new ProductWithTotalDTO
            {
                Id = group.First().Product.Id,
                ProductName = group.First().Product.ProductName,
                Price = group.First().Product.Price,
                Brand = group.First().Product.Brand,
                CategoryId = group.First().Product.CategoryId,
                TotalQuantity = group.Sum(op => op.Quantity),
            })
            .OrderByDescending(products => products.TotalQuantity)
            .Take(amount.GetValueOrDefault(5))
            .ToList();
        return Results.Ok(foundProducts);
    }
);

app.Run();

//don't move or change this!
public partial class Program { }
