using Eventos.ViewModels;

namespace Eventos.Pages;

/// <summary>
/// Entry form UI
/// </summary>
public partial class EntryFormPage
{

	/// <summary>
	/// Receives the depedencies by DI
	/// </summary>
	public EntryFormPage(EntryFormViewModel viewModel) : base(viewModel, "Entry")
	{
		InitializeComponent();
	}
}

