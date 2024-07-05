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

    }
}
