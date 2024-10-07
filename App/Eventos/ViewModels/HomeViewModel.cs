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
        private bool isVisibleNextEvent;

        [ObservableProperty]
        private ObservableCollection<Chart> charts = 
            new ObservableCollection<Chart>() { 
                new LineChart { Entries = new List<ChartEntry>() { new ChartEntry(0) } },
                new LineChart { Entries = new List<ChartEntry>() { new ChartEntry(0) } },
                new LineChart { Entries = new List<ChartEntry>() { new ChartEntry(0) } }};

        [ObservableProperty]
        private EventItem nextEvent;

        [ObservableProperty]
        private double progres;

        [ObservableProperty]
        private ObservableCollection<EventItem> events;

        [ObservableProperty]
        private EventItem selectedEvent;


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

            try
            {
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

                    if (this.User.ClickCount > 0)
                    {
                        this.Progres = (double)this.User.TotalClicksCurrentPeriod / this.User.ClickCount;
                    }
                    else
                    {
                        this.Progres = 0;
                    }

                    await this.DataService.InsertOrUpdateItemsAsync(this.User);
                }

                var uri = string.Format(Constants.GetEventsByUserUri, this.User.Email);

                var response = await this.HttpService.GetJsonObjectAsync(uri);

                var data = JsonConvert.DeserializeObject<ResponseData>(response.ToString());

                var eventObject = JsonConvert.DeserializeObject<Data>(data.Data.ToString());

                this.Events = new ObservableCollection<EventItem>(eventObject.Events);


                this.NextEvent = this.Events.OrderBy(x => x.StartTime).FirstOrDefault(x => !x.IsDelete);

                this.IsVisibleNextEvent = this.NextEvent != null ? true : false;


                if (this.NextEvent?.ClickedInfo != null && this.NextEvent.ClickedInfo.Any())
                {
                    var clicksGroupByDay = this.NextEvent.ClickedInfo.GroupBy(x => x.ClickedDateTime.Second).Select(x => new { Date = x.Key, Day = x.FirstOrDefault().ClickedDateTime, Count = x.Count() }).ToList();

                    var entries = new List<ChartEntry>();

                    foreach (var item in clicksGroupByDay)
                    {
                        entries.Add(new ChartEntry(item.Count)
                        {
                            Label = item.Day.ToString("dd/MM/yyyy"),
                            ValueLabel = item.Count.ToString(),
                            TextColor = SKColor.Parse("#184159"),
                            Color = SKColor.Parse("#184159"),
                            ValueLabelColor = SKColor.Parse("#184159"),
                        });
                    }

                    this.Charts[0] = new LineChart { Entries = entries, LabelTextSize = 30, LineMode = LineMode.Straight, LineSize = 6, ShowYAxisLines = true };

                    this.SelectedEvent = this.Events.OrderBy(x => x.StartTime).FirstOrDefault(x => !x.IsDelete);
                }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "No se ha podido cargar la información", "OK");
            }

            this.IsRefreshingList = false;

        }

        [RelayCommand]
        private async void ChangeEventDataAsync()
        {
            var clicksGroupByDay = this.SelectedEvent.ClickedInfo.GroupBy(x => x.ClickedDateTime.Second).Select(x => new { Date = x.Key, Day = x.FirstOrDefault().ClickedDateTime, Count = x.Count() }).ToList();

            var entries = new List<ChartEntry>();

            foreach (var item in clicksGroupByDay)
            {
                entries.Add(new ChartEntry(item.Count)
                {
                    Label = item.Day.ToString("dd/MM/yyyy"),
                    ValueLabel = item.Count.ToString(),
                    TextColor = SKColor.Parse("#184159"),
                    Color = SKColor.Parse("#184159"),
                    ValueLabelColor = SKColor.Parse("#184159"),
                });
            }

            var line = new LineChart { Entries = entries, LabelTextSize = 30, LineMode = LineMode.Straight, LineSize = 6, ShowYAxisLines = true };

            this.Charts[1] = line;
        }

    }
}
