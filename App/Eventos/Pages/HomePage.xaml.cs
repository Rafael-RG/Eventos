
using Eventos.Common.Pages;
using Eventos.ViewModels;
using Microcharts;

namespace Eventos.Pages;

/// <summary>
/// Home UI
/// </summary>
public partial class HomePage
{
    private HomeViewModel viewModel;

    /// <summary>
    /// Receives the depedencies by DI
    /// </summary>
    public HomePage(HomeViewModel viewModel) : base(viewModel, "HomePage")
	{
		InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
        this.viewModel = viewModel;
        this.BindingContext = viewModel;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is HomeViewModel viewModel)
        {
            if (viewModel.User != null)
            {
                this.viewModel.LoadDataCommand.Execute(null);
            }
        }
    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.viewModel.ChangeEventDataAsyncCommand.Execute(null);

        OnPropertyChanged(nameof(this.viewModel.Charts));
    }
}

