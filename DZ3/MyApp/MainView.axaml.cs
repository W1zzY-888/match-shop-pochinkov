using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;

namespace MyApp;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            DbSeed.Seed(db);
        }
        RefreshLists();
    }

    private void RefreshLists()
    {
        using var db = new AppDbContext();
        var clients = db.Clients.AsNoTracking().OrderBy(x => x.FullName).ToList();
        var services = db.Services.AsNoTracking().OrderBy(x => x.Name).ToList();
        var requests = db.RepairRequests.AsNoTracking().Include(x => x.Client).Include(x => x.Service)
            .OrderByDescending(x => x.CreatedAt).ToList();
        ClientsList.SelectedItem = null;
        ServicesList.SelectedItem = null;
        RequestsList.SelectedItem = null;
        ClientsList.ItemsSource = clients;
        ServicesList.ItemsSource = services;
        RequestsList.ItemsSource = requests;
        RepairRequestClient.ItemsSource = clients;
        RepairRequestService.ItemsSource = services;
        RepairRequestClient.SelectedItem = null;
        RepairRequestService.SelectedItem = null;
        ClientRequestsList.ItemsSource = Array.Empty<RepairRequest>();
    }

    private void Persist(AppDbContext db, TextBlock message, string success)
    {
        try
        {
            db.SaveChanges();
            RefreshLists();
            message.Text = success;
        }
        catch (Exception ex)
        {
            message.Text = "Не удалось сохранить: " + ex.GetBaseException().Message;
        }
    }

    private bool ReadClient(Client row)
    {
        var name = ClientFullName.Text?.Trim() ?? "";
        var phone = ClientPhone.Text?.Trim() ?? "";
        if (name.Length == 0 || phone.Length == 0)
        {
            ClientsMessage.Text = "Заполните ФИО и телефон.";
            return false;
        }
        row.FullName = name;
        row.Phone = phone;
        row.Email = string.IsNullOrWhiteSpace(ClientEmail.Text) ? null : ClientEmail.Text.Trim();
        row.IsVip = ClientIsVip.IsChecked == true;
        return true;
    }

    private bool ReadService(Service row)
    {
        var name = ServiceName.Text?.Trim() ?? "";
        if (name.Length == 0)
        {
            ServicesMessage.Text = "Введите название услуги.";
            return false;
        }
        if (!int.TryParse(ServicePrice.Text, out var price) || price < 0)
        {
            ServicesMessage.Text = "Цена должна быть целым числом не меньше нуля.";
            return false;
        }
        if (!int.TryParse(ServiceDurationMinutes.Text, out var duration) || duration <= 0)
        {
            ServicesMessage.Text = "Длительность должна быть положительным целым числом.";
            return false;
        }
        row.Name = name;
        row.Price = price;
        row.DurationMinutes = duration;
        row.IsAvailable = ServiceIsAvailable.IsChecked == true;
        return true;
    }

    private bool ReadRequest(RepairRequest row)
    {
        if (RepairRequestClient.SelectedItem is not Client client ||
            RepairRequestService.SelectedItem is not Service service)
        {
            RequestsMessage.Text = "Выберите клиента и услугу.";
            return false;
        }
        if (RepairRequestCreatedAt.SelectedDate is not DateTime date)
        {
            RequestsMessage.Text = "Выберите дату заявки.";
            return false;
        }
        var description = RepairRequestProblemDescription.Text?.Trim() ?? "";
        if (description.Length == 0)
        {
            RequestsMessage.Text = "Введите описание проблемы.";
            return false;
        }
        if (!int.TryParse(RepairRequestStatusCode.Text, out var status) || status < 1 || status > 3)
        {
            RequestsMessage.Text = "Статус должен быть числом от 1 до 3.";
            return false;
        }
        row.ClientId = client.Id;
        row.ServiceId = service.Id;
        row.CreatedAt = date.Date;
        row.ProblemDescription = description;
        row.StatusCode = status;
        return true;
    }

    private void ClientsSelected(object? sender, SelectionChangedEventArgs e)
    {
        ClientRequestsList.ItemsSource = Array.Empty<RepairRequest>();
        if (ClientsList.SelectedItem is not Client row) return;
        ClientFullName.Text = row.FullName;
        ClientPhone.Text = row.Phone;
        ClientEmail.Text = row.Email;
        ClientIsVip.IsChecked = row.IsVip;
        using var db = new AppDbContext();
        ClientRequestsList.ItemsSource = db.RepairRequests.AsNoTracking()
            .Include(x => x.Client).Include(x => x.Service).Where(x => x.ClientId == row.Id).ToList();
    }

    private void ServicesSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (ServicesList.SelectedItem is not Service row) return;
        ServiceName.Text = row.Name;
        ServicePrice.Text = row.Price.ToString();
        ServiceDurationMinutes.Text = row.DurationMinutes.ToString();
        ServiceIsAvailable.IsChecked = row.IsAvailable;
    }

    private void RequestsSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (RequestsList.SelectedItem is not RepairRequest row) return;
        RepairRequestClient.SelectedItem = RepairRequestClient.Items.OfType<Client>().FirstOrDefault(x => x.Id == row.ClientId);
        RepairRequestService.SelectedItem = RepairRequestService.Items.OfType<Service>().FirstOrDefault(x => x.Id == row.ServiceId);
        RepairRequestCreatedAt.SelectedDate = row.CreatedAt;
        RepairRequestProblemDescription.Text = row.ProblemDescription;
        RepairRequestStatusCode.Text = row.StatusCode.ToString();
    }
    private void ClientsClear(object? sender, RoutedEventArgs e)
    {
        ClientsList.SelectedItem = null;
        ClientFullName.Text = ClientPhone.Text = ClientEmail.Text = "";
        ClientIsVip.IsChecked = false;
        ClientsMessage.Text = "";
    }
    private void ServicesClear(object? sender, RoutedEventArgs e)
    {
        ServicesList.SelectedItem = null;
        ServiceName.Text = ServicePrice.Text = ServiceDurationMinutes.Text = "";
        ServiceIsAvailable.IsChecked = false;
        ServicesMessage.Text = "";
    }
    private void RequestsClear(object? sender, RoutedEventArgs e)
    {
        RequestsList.SelectedItem = null;
        RepairRequestClient.SelectedItem = RepairRequestService.SelectedItem = null;
        RepairRequestCreatedAt.SelectedDate = null;
        RepairRequestProblemDescription.Text = RepairRequestStatusCode.Text = "";
        RequestsMessage.Text = "";
    }

    private void ClientsAdd(object? sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        var row = new Client();
        if (!ReadClient(row)) return;
        db.Clients.Add(row);
        Persist(db, ClientsMessage, "Запись добавлена.");
    }
    private void ClientsSave(object? sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        if (ClientsList.SelectedItem is not Client selected)
        {
            ClientsMessage.Text = "Выберите запись для сохранения.";
            return;
        }
        var row = db.Clients.Find(selected.Id);
        if (row is null)
        {
            ClientsMessage.Text = "Запись уже удалена. Обновите выбор.";
            RefreshLists();
            return;
        }
        if (!ReadClient(row)) return;
        Persist(db, ClientsMessage, "Изменения сохранены.");
    }
    private void ClientsDelete(object? sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        if (ClientsList.SelectedItem is not Client selected)
        {
            ClientsMessage.Text = "Выберите запись для удаления.";
            return;
        }
        var row = db.Clients.Find(selected.Id);
        if (row is null)
        {
            ClientsMessage.Text = "Запись уже удалена.";
            RefreshLists();
            return;
        }
        db.Clients.Remove(row);
        Persist(db, ClientsMessage, "Запись удалена.");
    }

    private void ServicesAdd(object? sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        var row = new Service();
        if (!ReadService(row)) return;
        db.Services.Add(row);
        Persist(db, ServicesMessage, "Запись добавлена.");
    }
    private void ServicesSave(object? sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        if (ServicesList.SelectedItem is not Service selected)
        {
            ServicesMessage.Text = "Выберите запись для сохранения.";
            return;
        }
        var row = db.Services.Find(selected.Id);
        if (row is null)
        {
            ServicesMessage.Text = "Запись уже удалена. Обновите выбор.";
            RefreshLists();
            return;
        }
        if (!ReadService(row)) return;
        Persist(db, ServicesMessage, "Изменения сохранены.");
    }
    private void ServicesDelete(object? sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        if (ServicesList.SelectedItem is not Service selected)
        {
            ServicesMessage.Text = "Выберите запись для удаления.";
            return;
        }
        var row = db.Services.Find(selected.Id);
        if (row is null)
        {
            ServicesMessage.Text = "Запись уже удалена.";
            RefreshLists();
            return;
        }
        db.Services.Remove(row);
        Persist(db, ServicesMessage, "Запись удалена.");
    }

    private void RequestsAdd(object? sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        var row = new RepairRequest();
        if (!ReadRequest(row)) return;
        db.RepairRequests.Add(row);
        Persist(db, RequestsMessage, "Запись добавлена.");
    }
    private void RequestsSave(object? sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        if (RequestsList.SelectedItem is not RepairRequest selected)
        {
            RequestsMessage.Text = "Выберите запись для сохранения.";
            return;
        }
        var row = db.RepairRequests.Find(selected.Id);
        if (row is null)
        {
            RequestsMessage.Text = "Запись уже удалена. Обновите выбор.";
            RefreshLists();
            return;
        }
        if (!ReadRequest(row)) return;
        Persist(db, RequestsMessage, "Изменения сохранены.");
    }
    private void RequestsDelete(object? sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();
        if (RequestsList.SelectedItem is not RepairRequest selected)
        {
            RequestsMessage.Text = "Выберите запись для удаления.";
            return;
        }
        var row = db.RepairRequests.Find(selected.Id);
        if (row is null)
        {
            RequestsMessage.Text = "Запись уже удалена.";
            RefreshLists();
            return;
        }
        db.RepairRequests.Remove(row);
        Persist(db, RequestsMessage, "Запись удалена.");
    }
}
