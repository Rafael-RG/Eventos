using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Entry form UI
/// </summary>
public partial class EventDetailPage
{

	/// <summary>
	/// Receives the depedencies by DI
	/// </summary>
	public EventDetailPage(EventDetailViewModel viewModel) : base(viewModel, "EventDetailPage")
	{
		InitializeComponent();
        this.BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}

