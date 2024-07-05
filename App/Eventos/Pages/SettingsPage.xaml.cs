using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Settings UI
/// </summary>
public partial class SettingsPage
{

	/// <summary>
	/// Receives the depedencies by DI
	/// </summary>
	public SettingsPage(SettingsViewModel viewModel) : base(viewModel, "SettingsPage")
	{
		InitializeComponent();
        this.BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}

