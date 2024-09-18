using CommunityToolkit.Maui;
using Eventos.Common.Extensions;
using Eventos.Common.Interfaces;
using Eventos.DataAccess;
using Microcharts.Maui;

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
			});
        builder.Services.AddDbContext<DatabaseContext>();
        builder.Services.AddSingleton<IDataService, DataService>();
        builder.Services.AddLocalization();

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
