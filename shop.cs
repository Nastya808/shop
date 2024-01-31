using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Price { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string Fio { get; set; }
    public string Address { get; set; }
    public List<OrderLine> OrderLines { get; set; }
}

public class OrderLine
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public Product Product { get; set; }
}

public class ApplicationContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=(Localdb)\\mssqllocaldb;Database=testdb;Trusted_Connection=True;MultipleActiveResultSets=true");
        base.OnConfiguring(optionsBuilder);
    }
}

public class ApplicationContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=(Localdb)\\mssqllocaldb;Database=testdb;Trusted_Connection=True;MultipleActiveResultSets=true");
        base.OnConfiguring(optionsBuilder);
    }
}

public class OrderService
{
    public void EnsurePopulated()
    {
        using (var db = new ApplicationContext())
        {
            db.Database.EnsureCreated();

            if (!db.Products.Any())
            {
                db.Products.AddRange(Product.TestData);
                db.SaveChanges();
            }
        }
    }

    public Product GetProduct(int id)
    {
        using (var db = new ApplicationContext())
        {
            return db.Products.FirstOrDefault(p => p.Id == id);
        }
    }

    public void AddOrder(Order order)
    {
        using (var db = new ApplicationContext())
        {
            db.Orders.Add(order);
            db.SaveChanges();
        }
    }

    public Order GetOrder(int id)
    {
        using (var db = new ApplicationContext())
        {
            return db.Orders.Include(o => o.OrderLines).ThenInclude(ol => ol.Product).FirstOrDefault(o => o.Id == id);
        }
    }
}

class Program
{
    static void Main()
    {
        var orderService = new OrderService();
        orderService.EnsurePopulated();

        var order = new Order
        {
            Fio = "Alex Smith",
            Address = "Kiev, Pelev 12, 44",
            OrderLines = new List<OrderLine>
            {
                new OrderLine { ProductId = 1, Quantity = 2 },
                new OrderLine { ProductId = 2, Quantity = 1 }
            }
        };
        orderService.AddOrder(order);

        var retrievedOrder = orderService.GetOrder(1);
        Console.WriteLine($"Order ID: {retrievedOrder.Id}, Fio: {retrievedOrder.Fio}, Address: {retrievedOrder.Address}");
        foreach (var orderLine in retrievedOrder.OrderLines)
        {
            Console.WriteLine($"Product ID: {orderLine.Product.Id}, Product Name: {orderLine.Product.Name}, Quantity: {orderLine.Quantity}");
        }
    }
}
