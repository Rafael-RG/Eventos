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
        private bool isVisibleListEvent;

        [ObservableProperty]
        private ObservableCollection<Chart> charts;

        [ObservableProperty]
        private EventItem nextEvent;

        [ObservableProperty]
        private double progres;

        [ObservableProperty]
        private ObservableCollection<EventItem> events;

        [ObservableProperty]
        private EventItem selectedEvent;

        [ObservableProperty]
        private double nextEventWidth;

        [ObservableProperty]
        private double selectedEventWidth;

        [ObservableProperty]
        private bool isVisibleGraphic1;

        [ObservableProperty]
        private bool isVisibleGraphic2;

        //[ObservableProperty]
        //private double totalEventWidth;


        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public HomeViewModel(IServiceProvider provider) :  base(provider)
        {
            this.Charts =
            new ObservableCollection<Chart>() {
                new LineChart { Entries = new List<ChartEntry>() { new ChartEntry(0) } },
                new LineChart { Entries = new List<ChartEntry>() { new ChartEntry(0) } },
                new LineChart { Entries = new List<ChartEntry>() { new ChartEntry(0) } }};
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
        private async Task LoadData()
        {
            this.IsRefreshingList = true;
            IsBusy = true;
            try
            {
                this.User = await this.DataService.LoadUserAsync();

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

                this.IsVisibleListEvent = false;

                if (this.Events.Any()) 
                {
                    this.IsVisibleListEvent = true;
                    this.SelectedEvent= this.Events.OrderBy(x => x.StartTime).FirstOrDefault(x => !x.IsDelete);

                    this.ChangeEventDataAsync();
                }

                this.NextEvent = this.Events.OrderBy(x => x.StartTime).FirstOrDefault(x => !x.IsDelete && DateTimeOffset.Parse(x.StartTime, null, System.Globalization.DateTimeStyles.AssumeUniversal) > DateTime.UtcNow);

                this.IsVisibleNextEvent = this.NextEvent != null ? true : false;


                if (this.NextEvent?.ClickedInfo != null && this.NextEvent.ClickedInfo.Any())
                {
                    var clicksGroupByDay = this.NextEvent.ClickedInfo.GroupBy(x => x.ClickedDateTime.LocalDateTime.Date).Select(x => new { Date = x.Key, Day = x.FirstOrDefault().ClickedDateTime, Count = x.Count() });

                    if (clicksGroupByDay.Any())
                    {
                        var entries = new List<ChartEntry>();

                        foreach (var item in clicksGroupByDay)
                        {
                            entries.Add(new ChartEntry(item.Count)
                            {
                                Label = item.Day.ToString("dd/MM/yyyy"),
                                ValueLabel = item.Count.ToString(),
                                TextColor = SKColor.Parse("#184159"),
                                Color = SKColor.Parse("#61a60e"),
                                OtherColor = SKColor.Parse("#61a60e"),
                                ValueLabelColor = SKColor.Parse("#184159"),
                            });
                        }

                        this.Charts[0] = new LineChart { Entries = entries, LabelTextSize = 30, LineMode = LineMode.Straight, LineSize = 6, ShowYAxisLines = true };

                        this.IsVisibleGraphic1 = true;

                        this.NextEventWidth = ((LineChart)(this.Charts[1])).Entries.Count() > 15 ? ((LineChart)(this.Charts[1])).Entries.Count() * 30 : 400;
                    }
                    else
                    {
                        this.Charts[0] = new LineChart
                        {
                            Entries = new List<ChartEntry>() { new ChartEntry(0) }
                        };

                        this.IsVisibleGraphic1 = false;
                    }

                    OnPropertyChanged(nameof(this.Charts));
                }
                else
                {
                    this.Charts[0] = new LineChart
                    {
                        Entries = new List<ChartEntry>() { new ChartEntry(0) }
                    };

                    this.IsVisibleGraphic1 = false;
                }

                OnPropertyChanged(nameof(this.Charts));
                IsBusy = false;
            }
            catch
            {
                IsBusy = false;
                await App.Current.MainPage.DisplayAlert("Error", "No se ha podido cargar la información", "OK");
            }

            this.IsRefreshingList = false;

        }

        [RelayCommand]
        private void ChangeEventDataAsync()
        {
            if (this.SelectedEvent == null) 
            {
                this.Charts[1] = new LineChart
                {
                    Entries = new List<ChartEntry>() { new ChartEntry(0) }
                };

                this.IsVisibleGraphic2 = false;
                OnPropertyChanged(nameof(this.Charts));

                return;
            }

            var clicksGroupByDay = this.SelectedEvent.ClickedInfo.GroupBy(x => x.ClickedDateTime.LocalDateTime.Date).Select(x => new { Date = x.Key, Day = x.FirstOrDefault().ClickedDateTime, Count = x.Count() });

            var entries = new List<ChartEntry>();

            if (clicksGroupByDay.Any())
            {
                foreach (var item in clicksGroupByDay)
                {
                    entries.Add(new ChartEntry(item.Count)
                    {
                        Label = item.Day.ToString("dd/MM/yyyy"),
                        ValueLabel = item.Count.ToString(),
                        TextColor = SKColor.Parse("#184159"),
                        Color = SKColor.Parse("#61a60e"),
                        OtherColor = SKColor.Parse("#61a60e"),
                        ValueLabelColor = SKColor.Parse("#184159"),
                    });
                }

                var line = new LineChart { Entries = entries, LabelTextSize = 30, LineMode = LineMode.Straight, LineSize = 6, ShowYAxisLines = true };

                this.Charts[1] = line;

                this.SelectedEventWidth = ((LineChart)(this.Charts[1])).Entries.Count() > 15 ? ((LineChart)(this.Charts[1])).Entries.Count() * 30 : 400;

                this.IsVisibleGraphic2 = true;
            }
            else
            {
                this.Charts[1] = new LineChart
                {
                    Entries = new List<ChartEntry>() { new ChartEntry(0) }
                };

                this.IsVisibleGraphic2 = false;
            }

            OnPropertyChanged(nameof(this.Charts));
        }

    }
}
