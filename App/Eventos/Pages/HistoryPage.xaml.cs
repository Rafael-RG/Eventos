using Eventos.Models;
using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Entry form UI
/// </summary>
public partial class HistoryPage
{
    private HistoryViewModel viewModel;
    private readonly IServiceProvider provider;

    public HistoryPage(HistoryViewModel viewModel, IServiceProvider provider) : base(viewModel, "HistoryPage")
    {
        InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
        this.BindingContext = viewModel;
        this.viewModel = viewModel;
        this.provider = provider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is HistoryViewModel viewModel)
        {
            this.viewModel.RefreshAsyncCommand.Execute(null);
        }
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {

        this.viewModel.IsBusy = true;
        var eventItem = (EventItem)((ImageButton)sender).BindingContext;

        await Navigation.PushAsync(new EventDetailPage(new EventDetailViewModel(provider, eventItem), () => { this.viewModel.RefreshAsyncCommand.Execute(null); }));
    }
}

