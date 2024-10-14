using CommunityToolkit.Mvvm.ComponentModel;
using Eventos.Common.ViewModels;
using System.Collections.ObjectModel;
using Eventos.Common.Interfaces;
using Eventos.Models;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Sample viewmodel to show a collection of items
    /// </summary>
    public partial class HistoryViewModel : BaseViewModel
    {
        ///// <summary>
        ///// User
        ///// </summary>
        [ObservableProperty]
        private User user;

        [ObservableProperty]
        private ObservableCollection<EventItem> events;

        [ObservableProperty]
        private bool isRefreshingList;



        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public HistoryViewModel(IServiceProvider provider) : base(provider)
        {
        }

        public override async void OnAppearing()
        {
            IsBusy = true;
            try
            {
                this.User = await this.DataService.LoadUserAsync();

                this.refreshAsyncCommand.Execute(null);
            }
            catch (Exception ex) 
            {
                await this.NotificationService.NotifyErrorAsync("Error", "Hubo un error al obtener los eventos");
            }

            IsBusy = false;
        }

        /// <summary>
        /// Refresh command
        /// Param EventItem
        ///</summary>s
        [RelayCommand]
        private async void RefreshAsync()
        {
            this.IsRefreshingList = true;
            try
            {
                var uri = string.Format(Common.Constants.GetEventsByUserUri, this.User.Email);

                var response = await this.HttpService.GetJsonObjectAsync(uri);

                var data = JsonConvert.DeserializeObject<ResponseData>(response.ToString());

                var events = JsonConvert.DeserializeObject<Data>(data.Data.ToString());

                var evetsActive = events.Events.Where(x => !x.IsDelete).OrderByDescending(x=>x.Timestamp).ToList();

                foreach (var item in evetsActive)
                {
                    item.URLFile = string.Format(Constants.WebFileUri, item.RowKey);
                }

                this.Events = new ObservableCollection<EventItem>(evetsActive);

            }
            catch (Exception ex)
            {
                await this.NotificationService.NotifyErrorAsync("Error", "Hubo un error al cargar los eventos");
            }

            this.IsRefreshingList = false;
        }

        /// <summary>
        /// CopyLink command
        ///</summary>
        [RelayCommand]
        private async void CopyTextAsync(string uri)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                await Clipboard.Default.SetTextAsync(uri);
                await Application.Current.MainPage.DisplayAlert("Éxito", "Url copiada al portapapeles", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No hay Url para copiar", "OK");
            }
        }
    }
}
