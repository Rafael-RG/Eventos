using CommunityToolkit.Mvvm.ComponentModel;
using Eventos.Models;
using Eventos.Common.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common;

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

        [ObservableProperty]
        private bool isRefreshingList;


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

        [RelayCommand]
        private async void LoadData()
        {
            this.IsRefreshingList = true;

            var userInfo = await this.HttpService.PostAsync<SuscriberUserInfo>(new ValidateSubscriptionRequest { UserEmail= this.User.Email}, Constants.ValidateSuscription);

            this.User.IsSubscribed = userInfo.IsSubscribed;

            if(this.User.IsSubscribed)
            {
                this.User.Plan = userInfo.Plan;
                this.User.PlanFinishDate = userInfo.PlanFinishDate;
                this.User.ClickCount = userInfo.ClickCount;
            }

            var userStorage = await this.HttpService.PostAsync<LoginUser>(new ValidateSubscriptionRequest { UserEmail = this.User.Email }, Constants.GetUser);

            this.User.TotalClicks = userStorage.TotalClicks;
            this.User.TotalClicksCurrentPeriod = userStorage.TotalClicksCurrentPeriod;
            this.User.LastPeriod = userStorage.LastPeriod;

            await this.DataService.InsertOrUpdateItemsAsync(this.User);
            
            this.IsRefreshingList = false;
        }   



    }
}
