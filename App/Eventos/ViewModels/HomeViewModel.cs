using CommunityToolkit.Mvvm.ComponentModel;
using Eventos.Models;
using Eventos.Common.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common;
using Newtonsoft.Json;
using Microcharts;
using System.Collections.ObjectModel;
using SkiaSharp;

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

        [ObservableProperty]
        private ObservableCollection<Chart> charts;


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
            if (this.User != null)
            {
                var datasuscription = await this.HttpService.PostAsync<ResponseData>(new ValidateSubscriptionRequest { UserEmail = this.User.Email }, Constants.ValidateSuscription);

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
            }

            var entries = new[]
            {
                new ChartEntry(200)
                {
                    Label = "January",
                    ValueLabel = "200",
                    Color = SKColor.Parse("#184159")
                },
                new ChartEntry(400)
                {
                    Label = "February",
                    ValueLabel = "400",
                    Color = SKColor.Parse("#D2E898")
                },
                new ChartEntry(100)
                {
                    Label = "March",
                    ValueLabel = "100",
                    Color = SKColor.Parse("#184159")
                },
                new ChartEntry(300)
                {
                    Label = "April",
                    ValueLabel = "300",
                    Color = SKColor.Parse("#D2E898")
                }
            };

            this.Charts = new ObservableCollection<Chart>
            {
                new BarChart { Entries = entries, LabelTextSize = 40 },
                new LineChart { Entries = entries, LabelTextSize = 40 },
                new PointChart { Entries = entries, LabelTextSize = 40 },
                new DonutChart { Entries = entries, LabelTextSize = 40 },
                new RadialGaugeChart { Entries = entries, LabelTextSize = 40 }
            };
            this.IsRefreshingList = false;

        }   



    }
}
