using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;
using Eventos.Models;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Synchronization UI logic
    /// </summary>
    public partial class SettingsViewModel : BaseViewModel
    {
        [ObservableProperty]
        private User user;


        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public SettingsViewModel(IServiceProvider provider) : base(provider)
        {
            
        }

        public override async void OnAppearing()
        {
            IsBusy = true;

            this.User = await this.DataService.LoadUserAsync();

            if (string.IsNullOrEmpty(this.User?.FullName))
            {
                await Shell.Current.GoToAsync("///LoginPage", false);
            }
            else
            {
                AppShell.User = this.User;
            }
            IsBusy = false;
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void Logout()
        {
            this.IsBusy = true;
            await this.DataService.DeleteItemAsync(AppShell.User);

            await Shell.Current.GoToAsync("///LoginPage", false);
            this.IsBusy = false;
        }
    }
}
