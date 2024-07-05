using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Entry form UI
/// </summary>
public partial class HistoryPage
{
    private HistoryViewModel viewModel;

    /// <summary>
    /// Receives the depedencies by DI
    /// </summary>
    public HistoryPage(HistoryViewModel viewModel) : base(viewModel, "HistoryPage")
	{
        InitializeComponent();
        this.BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}

