using Microsoft.Extensions.Logging;
#if ANDROID
using Android.OS;
using Android.Views;
using Microsoft.Maui.Platform;
#endif

namespace OKKT25
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if ANDROID
            // 🔹 Android státuszsáv testreszabása az egész alkalmazásra
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping("StatusBarColor", (handler, view) =>
            {
                var activity = Platform.CurrentActivity;
                if (activity?.Window != null && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    var window = activity.Window;

                    // Háttérszín sötétszürkére
                    window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#121212"));

                    // Világos ikonok, hogy látszódjanak sötét háttéren
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                    {
                        // Android 11+ API
                        var controller = window.InsetsController;
                        controller?.SetSystemBarsAppearance(0, (int)WindowInsetsControllerAppearance.LightStatusBars);
                    }
                    else
                    {
#pragma warning disable CA1422
                        window.DecorView.SystemUiVisibility = 0; // 0 = világos ikonok
#pragma warning restore CA1422
                    }
                }
            });
#endif

            return builder.Build();
        }
    }
}