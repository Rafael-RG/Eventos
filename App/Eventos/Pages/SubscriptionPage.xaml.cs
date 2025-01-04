using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Login UI
/// </summary>
public partial class SubscriptionPage
{
    /// <summary>
    /// Receives the depedencies by DI
    /// </summary>
    public SubscriptionPage(SubscriptionViewModel viewModel) : base(viewModel, "SubscriptionPage")
	{
        this.BindingContext = viewModel;
        Application.Current.UserAppTheme = AppTheme.Light;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}

