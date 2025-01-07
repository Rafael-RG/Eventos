using Eventos.Models;
using Eventos.Pages;

namespace Eventos;

public partial class AppShell : Shell
{

	public static User User;

	/// <summary>
	/// App shell
	/// </summary>
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));

		Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));

		Routing.RegisterRoute(nameof(SubscriptionPage), typeof(SubscriptionPage));

		Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

		Routing.RegisterRoute(nameof(AddEventPage), typeof(AddEventPage));

		Routing.RegisterRoute(nameof(EventDetailPage), typeof(EventDetailPage));

    }
}
