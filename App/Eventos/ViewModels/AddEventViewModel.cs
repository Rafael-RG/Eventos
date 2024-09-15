using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;
using System.Collections.ObjectModel;
using Eventos.Models;
using System.Text;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Sample view model to show a collection of items
    /// </summary>
    public partial class AddEventViewModel : BaseViewModel
    {
        [ObservableProperty]
        private User user;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private DateTime date;
        
        [ObservableProperty]
        private ObservableCollection<TimeZoneInfo> zones;

        [ObservableProperty]
        private TimeZoneInfo selectedZone;

        [ObservableProperty]
        private string selectedZoneId;

        [ObservableProperty]
        private TimeSpan startTime;

        [ObservableProperty]
        private TimeSpan endTime;

        [ObservableProperty]
        private string url;



        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public AddEventViewModel(IServiceProvider provider) : base(provider)
        {
            this.User = AppShell.User;
        }

        public override async void OnAppearing()
        {
            IsBusy = true;
            this.SelectedZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
            this.Date = DateTime.Now.ToLocalTime();
            
            var time = new DateTime(DateTime.Now.Ticks, DateTimeKind.Local);
            this.StartTime = time.TimeOfDay;
            this.EndTime = time.AddHours(1).TimeOfDay;

            if (this.Zones == null || !this.Zones.Any()) 
            {
                PopulateTimeZones();
            }
            IsBusy = false;
        }

        /// <summary>
        /// AddEvent command
        ///</summary>
        [RelayCommand]
        private async void AddEvent()
        {
            this.IsBusy = true;
            try 
            {
                if (this.StartTime == this.EndTime)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "La hora de inicio y fin no pueden ser iguales.", "OK");
                    this.IsBusy = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(this.Title) || string.IsNullOrWhiteSpace(this.Description) || this.SelectedZone == null)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos obligatorios.", "OK");
                    this.IsBusy = false;
                    return;
                }

                TimeZoneInfo userTimeZone = this.SelectedZone;
                DateTime startDateTimeLocal = DateTime.SpecifyKind(this.Date + this.StartTime, DateTimeKind.Unspecified);
                DateTime endDateTimeLocal = DateTime.SpecifyKind(this.Date + this.EndTime, DateTimeKind.Unspecified);

                DateTimeOffset eventStart = TimeZoneInfo.ConvertTimeToUtc(startDateTimeLocal, userTimeZone);
                DateTimeOffset eventEnd = TimeZoneInfo.ConvertTimeToUtc(endDateTimeLocal, userTimeZone);

                var newEvent = new Event
                {
                    Email = this.User.Email,
                    Title = this.Title,
                    Description = this.Description,
                    StartTime = eventStart,
                    EndTime = eventEnd,
                    Zone = this.SelectedZone.DisplayName,
                    ZoneId= this.SelectedZone.Id,
                    Date =  DateTime.SpecifyKind(this.Date, DateTimeKind.Utc),
                    Count = 0,
                    EventURl = this.Url
                };

                var result = await this.HttpService.PostAsync<ResponseData>(newEvent, Common.Constants.SaveEvent);

                if (result.Success) 
                {
                    await App.Current.MainPage.DisplayAlert("Creado", "Se creo correctamente el evento.", "OK");
                }

            }
            catch(Exception ex)
            {
                this.IsBusy = false;
                await this.NotificationService.NotifyErrorAsync("Error", "Hubo un error al crear al evento");
            }

            this.IsBusy = false;
        }

        private void PopulateTimeZones()
        {
            var mainTimeZones = new[]
            {
                "America/New_York",         // Eastern Time (ET)
                "America/Chicago",          // Central Time (CT)
                "America/Denver",           // Mountain Time (MT)
                "America/Los_Angeles",      // Pacific Time (PT)
                "America/Sao_Paulo",        // São Paulo Time (SPT)
                "America/Toronto",          // Toronto Time (ET)
                "America/Mexico_City",      // Mexico City Time (CST)
                "America/Buenos_Aires",     // Buenos Aires Time (ART)
                "America/Caracas",          // Caracas Time (VET)
                "America/Montevideo",        // Montevideo Time (UYT)
                "America/Guayaquil",        // Guayaquil Time (ECT)
                "America/Lima",             // Lima Time (PET)
                "America/Panama",           // Panama Time (EST)
                "America/Costa_Rica",       // Costa Rica Time (CST)
                "America/Honduras",         // Honduras Time (CST)
                "America/La_Paz",           // La Paz Time (BOT)
                "America/Asuncion",         // Asuncion Time (PYT)
                "America/Santo_Domingo",    // Santo Domingo Time (AST)
                "America/Port-au-Prince",   // Haiti Time (ET)
                "America/Barbados",         // Barbados Time (AST)
                "America/Kingston",         // Kingston Time (ET)
                "America/Belize"            // Belize Time (CST)
            };

            this.Zones = new ObservableCollection<TimeZoneInfo>(
                TimeZoneInfo.GetSystemTimeZones()
                    .Where(tz => mainTimeZones.Contains(tz.Id))
            );
        }

    }
}
