using CommunityToolkit.Mvvm.Input;
using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;
using Eventos.GoogleAuth;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Synchronization UI logic
    /// </summary>
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly IGoogleAuthService _googleAuthService;

        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public SettingsViewModel(IServiceProvider provider, IGoogleAuthService googleAuthService) : base(provider)
        {
            _googleAuthService = googleAuthService;
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void Logout()
        {

            await this.DataService.DeleteItemAsync(AppShell.User);

            await Shell.Current.GoToAsync("///LoginPage", false);
        }
    }
}
