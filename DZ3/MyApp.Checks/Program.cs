using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MyApp;
using MyApp.Data;

AppBuilder.Configure<App>().UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions
{ UseHeadlessDrawing = false }).SetupWithoutStarting();
var view = new MainView();
var window = new Window { Content = view, Width = 360, Height = 1000 };
window.Show();
var output = args.Length == 0 ? "screens" : args[0];
Directory.CreateDirectory(output);
T C<T>(string name) where T : Control => view.FindControl<T>(name) ?? throw new Exception(name);
void Click(string name) => C<Button>(name).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
void Input(string name, string value) => C<TextBox>(name).Text = value;
void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
void Shot(string name)
{
 Dispatcher.UIThread.RunJobs();
 using var bitmap = window.CaptureRenderedFrame() ?? throw new Exception("No frame");
 bitmap.Save(Path.Combine(output, name + ".png"));
}
int Count<T>() where T : class { using var db = new AppDbContext(); return db.Set<T>().Count(); }
foreach (var tab in new[] { 0, 1, 2 })
{
 C<TabControl>("Tabs").SelectedIndex = tab;
 Dispatcher.UIThread.RunJobs();
 var fields = view.GetVisualDescendants().OfType<Control>()
  .Where(x => x.IsEffectivelyVisible && !string.IsNullOrEmpty(x.Name) && !x.Name.StartsWith("PART_") &&
    (x is TextBox || x is ComboBox || x is Button || x is CalendarDatePicker || x is CheckBox)).ToList();
 Check(fields.Count > 0 && fields.All(x =>
 {
  var origin = x.TranslatePoint(new Point(0, 0), window);
  return origin.HasValue && origin.Value.X >= 0 && origin.Value.X + x.Bounds.Width <= 360 && x.Bounds.Height >= 40;
 }), "inputs fit narrow viewport and have touch height: tab " + tab);
 Shot("narrow-tab-" + tab);
}
C<TabControl>("Tabs").SelectedIndex = 0;
var tag = "Проверка " + Guid.NewGuid().ToString("N")[..8];
try
{
 var before = Count<Client>();
 Click("ClientsAddButton");
 Check(Count<Client>() == before && C<TextBlock>("ClientsMessage").Text!.Contains("ФИО"), "empty client rejected");
 Shot("validation-error");
 Click("ClientsSaveButton"); Click("ClientsDeleteButton");
 Check(Count<Client>() == before, "no-selection save/delete safe");
 Input("ClientFullName", tag); Input("ClientPhone", "+7 900 123-45-67");
 Click("ClientsAddButton");
 Check(Count<Client>() == before + 1, "client inserted");
 C<ListBox>("ClientsList").SelectedItem = C<ListBox>("ClientsList").Items.OfType<Client>().Single(x => x.FullName == tag);
 Shot("new-client");
 Input("ClientFullName", tag + " изменён"); Click("ClientsSaveButton");
 using (var db = new AppDbContext()) Check(db.Clients.Any(x => x.FullName == tag + " изменён"), "client update persisted in new context");
 Check(C<ComboBox>("RepairRequestClient").Items.OfType<Client>().Any(x => x.FullName == tag + " изменён"), "client combo refreshed");
 C<TabControl>("Tabs").SelectedIndex = 1;
 var servicesBefore = Count<Service>();
 Input("ServiceName", tag); Input("ServicePrice", "буквы"); Input("ServiceDurationMinutes", "45");
 Click("ServicesAddButton"); Check(Count<Service>() == servicesBefore, "invalid price rejected");
 Input("ServicePrice", "1000"); Input("ServiceDurationMinutes", "0"); Click("ServicesAddButton");
 Check(Count<Service>() == servicesBefore, "invalid duration rejected");
 Input("ServiceDurationMinutes", "45"); Click("ServicesAddButton");
 Check(Count<Service>() == servicesBefore + 1, "service inserted"); Shot("services");
 C<TabControl>("Tabs").SelectedIndex = 2;
 var requestsBefore = Count<RepairRequest>();
 Click("RequestsAddButton"); Check(Count<RepairRequest>() == requestsBefore, "missing related selections rejected");
 C<ComboBox>("RepairRequestClient").SelectedItem = C<ComboBox>("RepairRequestClient").Items.OfType<Client>().Single(x => x.FullName == tag + " изменён");
 C<ComboBox>("RepairRequestService").SelectedItem = C<ComboBox>("RepairRequestService").Items.OfType<Service>().Single(x => x.Name == tag);
 Click("RequestsAddButton"); Check(Count<RepairRequest>() == requestsBefore, "missing date rejected");
 C<CalendarDatePicker>("RepairRequestCreatedAt").SelectedDate = DateTime.Today;
 Input("RepairRequestProblemDescription", "Проверка сохранения заявки"); Input("RepairRequestStatusCode", "99");
 Click("RequestsAddButton"); Check(Count<RepairRequest>() == requestsBefore, "invalid status rejected");
 Input("RepairRequestStatusCode", "1"); Click("RequestsAddButton");
 Check(Count<RepairRequest>() == requestsBefore + 1 && Count<Client>() == before + 1 && Count<Service>() == servicesBefore + 1, "request inserted without duplicating parents");
 Shot("requests");
 C<ListBox>("RequestsList").SelectedItem = C<ListBox>("RequestsList").Items.OfType<RepairRequest>().Single(x => x.Client.FullName == tag + " изменён");
 Input("RepairRequestStatusCode", "2"); Click("RequestsSaveButton");
 using (var db = new AppDbContext()) Check(db.RepairRequests.Any(x => x.Client.FullName == tag + " изменён" && x.StatusCode == 2), "request update persisted");
 C<ListBox>("RequestsList").SelectedItem = C<ListBox>("RequestsList").Items.OfType<RepairRequest>().Single(x => x.Client.FullName == tag + " изменён");
 Click("RequestsDeleteButton"); Check(Count<RepairRequest>() == requestsBefore && Count<Client>() == before + 1, "request deleted without deleting client");
 C<ComboBox>("RepairRequestClient").SelectedItem = C<ComboBox>("RepairRequestClient").Items.OfType<Client>().Single(x => x.FullName == tag + " изменён");
 C<ComboBox>("RepairRequestService").SelectedItem = C<ComboBox>("RepairRequestService").Items.OfType<Service>().Single(x => x.Name == tag);
 Click("RequestsAddButton");
 window.Content = view = new MainView();
 Check(C<ListBox>("RequestsList").Items.OfType<RepairRequest>().Any(x => x.Client.FullName == tag + " изменён"), "data survives reopening view and contexts");
 C<ListBox>("ClientsList").SelectedItem = C<ListBox>("ClientsList").Items.OfType<Client>().Single(x => x.FullName == tag + " изменён");
 Click("ClientsDeleteButton");
 Check(Count<Client>() == before && Count<RepairRequest>() == requestsBefore, "client deletion cascades to requests");
 Input("ClientFullName", tag); Input("ClientPhone", "+7 900 123-45-67"); Click("ClientsAddButton");
 C<ComboBox>("RepairRequestClient").SelectedItem = C<ComboBox>("RepairRequestClient").Items.OfType<Client>().Single(x => x.FullName == tag);
 C<ComboBox>("RepairRequestService").SelectedItem = C<ComboBox>("RepairRequestService").Items.OfType<Service>().Single(x => x.Name == tag);
 C<CalendarDatePicker>("RepairRequestCreatedAt").SelectedDate = DateTime.Today;
 Input("RepairRequestProblemDescription", "Проверка удаления услуги"); Input("RepairRequestStatusCode", "1"); Click("RequestsAddButton");
 C<TabControl>("Tabs").SelectedIndex = 1;
 C<ListBox>("ServicesList").SelectedItem = C<ListBox>("ServicesList").Items.OfType<Service>().Single(x => x.Name == tag);
 Click("ServicesDeleteButton"); Check(Count<Service>() == servicesBefore, "service deleted");
 Check(Count<RepairRequest>() == requestsBefore && Count<Client>() == before + 1, "service deletion cascades to requests but keeps client");
 Console.WriteLine("ALL CHECKS PASSED");
}
finally
{
 using var db = new AppDbContext();
 db.Clients.RemoveRange(db.Clients.Where(x => x.FullName.StartsWith(tag)));
 db.Services.RemoveRange(db.Services.Where(x => x.Name == tag));
 db.SaveChanges();
 window.Close();
}
