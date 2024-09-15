using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Entry form UI
/// </summary>
public partial class AddEventPage
{

	/// <summary>
	/// Receives the depedencies by DI
	/// </summary>
	public AddEventPage(AddEventViewModel viewModel) : base(viewModel, "AddEventPage")
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

