using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Login UI
/// </summary>
public partial class LoginPage
{

	/// <summary>
	/// Receives the depedencies by DI
	/// </summary>
	public LoginPage(LoginViewModel viewModel) : base(viewModel, "LoginPage")
	{
        this.BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}

