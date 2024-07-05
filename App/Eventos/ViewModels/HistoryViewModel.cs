using CommunityToolkit.Mvvm.ComponentModel;
using Eventos.Common.ViewModels;
using System.Collections.ObjectModel;
using Eventos.Common.Interfaces;
using Eventos.Models;
using Newtonsoft.Json;


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
            this.User = AppShell.User;
        }

        public override async void OnAppearing()
        {
            try
            {
                var uri = string.Format(Common.Constants.GetEventsByUserUri, this.User.Email);

                var response = await this.httpService.GetJsonObjectAsync(uri);

                var data = JsonConvert.DeserializeObject<ResponseData>(response.ToString());

                var events = JsonConvert.DeserializeObject<Data>(data.Data.ToString());

                

                this.Events = new ObservableCollection<EventItem>(events.Events);
            }
            catch (Exception ex) 
            {
                await this.NotificationService.NotifyErrorAsync("Error", "Hubo un error al obtener los eventos");
            }

        }
    }
}
