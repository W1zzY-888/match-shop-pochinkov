public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
    public List<Order> Orders { get; set; } = new();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int MatchesPerBox { get; set; }
    public int Stock { get; set; }
    // string обязательна: Name имеет NOT NULL. string? допускает NULL.
    public string? Description { get; set; }
    public List<Order> Orders { get; set; } = new();
}

public class Order
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    // DateTime с Kind=Utc хранится как timestamp with time zone.
    // DateTime.Now имеет местное время; DateTime.UtcNow подходит для timestamptz.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
