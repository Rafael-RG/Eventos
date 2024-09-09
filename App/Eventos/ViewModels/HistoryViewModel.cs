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
        private IHttpService httpService;

        ///// <summary>
        ///// User
        ///// </summary>
        [ObservableProperty]
        private User user;

        [ObservableProperty]
        private ObservableCollection<EventItem> events;



        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public HistoryViewModel(IServiceProvider provider, IHttpService httpService) : base(provider)
        {
            this.httpService = httpService;
            //this.User = AppShell;
            this.User = new User() {Email="rafa_rg11@hotmail.com",FullName="Rafael" };
        }

        public override async void OnAppearing()
        {
            IsBusy = true;
            try
            {
                var uri = string.Format(Common.Constants.GetEventsByUserUri, this.User.Email);

                var response = await this.httpService.GetJsonObjectAsync(uri);

                var data = JsonConvert.DeserializeObject<ResponseData>(response.ToString());

                var events = JsonConvert.DeserializeObject<Data>(data.Data.ToString());

                foreach (var item in events.Events)
                {
                    item.URLFile = string.Format(Constants.WebFileUri, item.RowKey);
                }

                this.Events = new ObservableCollection<EventItem>(events.Events);
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
            IsBusy = true;
            try
            {
                var uri = string.Format(Common.Constants.GetEventsByUserUri, this.User.Email);

                var response = await this.httpService.GetJsonObjectAsync(uri);

                var data = JsonConvert.DeserializeObject<ResponseData>(response.ToString());

                var events = JsonConvert.DeserializeObject<Data>(data.Data.ToString());

                foreach (var item in events.Events)
                {
                    item.URLFile = string.Format(Constants.WebFileUri, item.RowKey);
                }

                this.Events = new ObservableCollection<EventItem>(events.Events);

                if (events.Events.Count()>0)
                {
                    await App.Current.MainPage.DisplayAlert("Completado", "Se recargo la lista de eventos.", "OK");
                }

            }
            catch (Exception ex)
            {
                await this.NotificationService.NotifyErrorAsync("Error", "Hubo un error al cargar los eventos");
            }

            IsBusy = false;
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
