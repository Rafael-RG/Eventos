using CommunityToolkit.Mvvm.ComponentModel;
using Eventos.Models;
using Eventos.Common.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace Eventos.ViewModels
{
    /// <summary>
    /// Home logic
    /// </summary>
    [QueryProperty("User", "User")]
    public partial class HomeViewModel : BaseViewModel
    {
        ///// <summary>
        ///// User
        ///// </summary>
        [ObservableProperty]
        private User user;


        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public HomeViewModel(IServiceProvider provider) :  base(provider)
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



    }
}
