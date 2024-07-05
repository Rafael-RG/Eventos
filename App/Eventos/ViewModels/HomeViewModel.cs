using CommunityToolkit.Mvvm.ComponentModel;
using Eventos.Models;
using Eventos.Common.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace Eventos.ViewModels
{
    /// <summary>
    /// Home logic
    /// </summary>
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
            this.User = AppShell.User;
        }


        public override async void OnAppearing()
        {
            if (this.User == null)
            {
                await Shell.Current.GoToAsync("///LoginPage", false);
            }
            else 
            {
                this.User = AppShell.User;
            }
        }



    }
}
