using CommunityToolkit.Mvvm.ComponentModel;
using Eventos.Models;
using Eventos.Common.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common;
using Newtonsoft.Json;

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

            var datasuscription = await this.HttpService.PostAsync<ResponseData>(new ValidateSubscriptionRequest { UserEmail= this.User.Email}, Constants.ValidateSuscription);

            if (datasuscription.Success) 
            {
                var userInfo = JsonConvert.DeserializeObject<SuscriberUserInfo>(datasuscription.Data.ToString());

                this.User.IsSubscribed = userInfo.IsSubscribed;

                if (this.User.IsSubscribed)
                {
                    this.User.Plan = userInfo.Plan;
                    this.User.PlanFinishDate = userInfo.PlanFinishDate;

                }

              this.User.ClickCount = userInfo.ClickCount;
            }

            var userData = await this.HttpService.PostAsync<ResponseData>(new ValidateSubscriptionRequest { UserEmail = this.User.Email }, Constants.GetUser);

            if (userData.Success)
            {
                var userStorage = JsonConvert.DeserializeObject<LoginUser>(userData.Data.ToString());

                this.User.TotalClicks = userStorage.TotalClicks;
                this.User.TotalClicksCurrentPeriod = userStorage.TotalClicksCurrentPeriod;
                this.User.LastPeriod = userStorage.LastPeriod;
            }

            await this.DataService.InsertOrUpdateItemsAsync(this.User);
            
            this.IsRefreshingList = false;
        }   



    }
}
