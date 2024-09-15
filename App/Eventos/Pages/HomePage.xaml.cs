
using Eventos.Common.Pages;
using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Home UI
/// </summary>
public partial class HomePage
{
	/// <summary>
	/// Receives the depedencies by DI
	/// </summary>
	public HomePage(HomeViewModel viewModel) : base(viewModel, "HomePage")
	{
		InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
        this.BindingContext = viewModel;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

}

