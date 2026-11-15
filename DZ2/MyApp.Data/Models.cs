namespace MyApp.Data;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public bool IsVip { get; set; }
    public List<RepairRequest> RepairRequests { get; set; } = new();

    public override string ToString() => $"{FullName}, тел. {Phone}, VIP: {IsVip}";
}

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsAvailable { get; set; }
    public List<RepairRequest> RepairRequests { get; set; } = new();

    public override string ToString() => $"{Name}, {Price} руб., {DurationMinutes} мин., доступна: {IsAvailable}";
}

public class RepairRequest
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string ProblemDescription { get; set; } = "";
    public int StatusCode { get; set; }

    public override string ToString() => $"{Client.FullName} — {Service.Name}, статус {StatusCode}, {ProblemDescription}";
}
