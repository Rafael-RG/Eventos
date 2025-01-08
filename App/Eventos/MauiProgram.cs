using CommunityToolkit.Maui;
using Eventos.Common.Extensions;
using Eventos.Common.Interfaces;
using Eventos.DataAccess;
using Eventos.Services;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
#if IOS
using StoreKit;
#endif

namespace Eventos;

/// <summary>
/// MAUI entry point 
/// </summary>
public static class MauiProgram
{
	/// <summary>
	/// Creates the app.
	/// Declares the font, depedenc
	/// y injection components
	/// </summary>
	/// <returns></returns>
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMicrocharts()
            .RegisterViewModelsAndServices()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureLifecycleEvents(AppLifecycle =>
             {
#if IOS
                 AppLifecycle.AddiOS(ios =>
                     ios.FinishedLaunching((del, b) =>
                    {
                        var current = Plugin.InAppBilling.CrossInAppBilling.Current;
                        return true;
                    }));
#endif
             });

#if IOS
        bool OnShouldAddStorePayment(SKPaymentQueue queue, SKPayment payment, SKProduct product)
        {
            //Process and check purchases
            return true;
        }
#endif


        builder.Services.AddDbContext<DatabaseContext>();
        builder.Services.AddSingleton<IDataService, DataService>();
        builder.Services.AddLocalization();
		builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
		builder.Services.AddSingleton<IReceiptValidationService, ReceiptValidationService>();

        builder.UseMauiApp<App>().UseMauiCommunityToolkit();
		builder.ConfigureMauiHandlers(h =>
		{
#if IOS
            h.AddHandler<Shell, Eventos.Platforms.iOS.CustomShellHandler>();
#endif
		});
        return builder.Build();
	}
}
