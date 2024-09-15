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
        this.BindingContext = viewModel;
        this.viewModel = viewModel;
        this.provider = provider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        var eventItem = (EventItem)((ImageButton)sender).BindingContext;

        await Navigation.PushAsync(new EventDetailPage(new EventDetailViewModel(provider, eventItem), () => { this.viewModel.RefreshAsyncCommand.Execute(null); }));
    }
}

