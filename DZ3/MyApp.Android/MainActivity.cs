using Android.App;
using Android.Content.PM;
using Android.Views;
using Avalonia.Android;

namespace MyApp.Android;

[Activity(Label = "FixPoint", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon",
 MainLauncher = true, Exported = true, WindowSoftInputMode = SoftInput.AdjustResize,
 ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
}
