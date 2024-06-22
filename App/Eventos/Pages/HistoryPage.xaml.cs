using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Entry form UI
/// </summary>
public partial class HistoryPage
{

	/// <summary>
	/// Receives the depedencies by DI
	/// </summary>
	public HistoryPage(HistoryViewModel viewModel) : base(viewModel, "History")
	{
		InitializeComponent();
	}
}

