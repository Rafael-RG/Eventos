using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Entry form UI
/// </summary>
public partial class EventDetailPage
{

    private readonly Action onPopCallback;

    /// <summary>
    /// Receives the depedencies by DI
    /// </summary>
    public EventDetailPage(EventDetailViewModel viewModel, Action onPop) : base(viewModel, "EventDetailPage")
	{
		InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Light;
        onPopCallback = onPop;
        this.BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        this.Navigation.PopAsync();
    }

    private void Button_Clicked_Update(object sender, EventArgs e)
    {
        this.ViewModel.UpdateEventCommand.Execute(null);
    }

    private async void Button_Clicked_Delete(object sender, EventArgs e)
    {
        this.ViewModel.UpdateEventCommand.Execute("delete");
        await Task.Delay(2000);
        await this.Navigation.PopAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        // Llamar al callback al desaparecer la página
        onPopCallback?.Invoke();
    }
}

