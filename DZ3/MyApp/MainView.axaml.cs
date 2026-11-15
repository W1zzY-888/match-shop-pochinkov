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

        using var db = new AppDbContext();
        db.Database.EnsureCreated();
        DbSeed.Seed(db);

        ClientsList.ItemsSource = db.Clients.AsNoTracking().ToList();
        ServicesList.ItemsSource = db.Services.AsNoTracking().ToList();
        RequestsList.ItemsSource = db.RepairRequests
            .Include(request => request.Client)
            .Include(request => request.Service)
            .AsNoTracking()
            .OrderByDescending(request => request.CreatedAt)
            .ToList();

        if (ClientsList.ItemCount > 0)
            ClientsList.SelectedIndex = 0;
    }

    private void ClientsList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ClientsList.SelectedItem is not Client client)
        {
            ClientRequestsList.ItemsSource = Array.Empty<RepairRequest>();
            return;
        }

        using var db = new AppDbContext();
        ClientRequestsList.ItemsSource = db.RepairRequests
            .Where(request => request.ClientId == client.Id)
            .Include(request => request.Client)
            .Include(request => request.Service)
            .AsNoTracking()
            .OrderByDescending(request => request.CreatedAt)
            .ToList();
    }
}
