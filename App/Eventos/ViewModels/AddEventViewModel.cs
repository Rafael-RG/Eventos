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
        private IHttpService httpService;

        ///// <summary>
        ///// User
        ///// </summary>
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



        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public AddEventViewModel(IServiceProvider provider, IHttpService httpService) : base(provider)
        {
            this.httpService = httpService;
            this.User = AppShell.User;
        }


        public override async void OnAppearing()
        {
            this.SelectedZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
            this.Date = DateTime.Now.ToLocalTime();
            
            var time = new DateTime(DateTime.Now.Ticks, DateTimeKind.Local);
            this.StartTime = time.TimeOfDay;
            this.EndTime = time.AddHours(1).TimeOfDay;

            if (this.Zones == null || this.Zones.Any()) 
            {
                PopulateTimeZones();
            }
            
        }

        /// <summary>
        /// AddEvent command
        ///</summary>
        [RelayCommand]
        private async void AddEvent()
        {
            try 
            {
                if (this.StartTime == this.EndTime)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "La hora de inicio y fin no pueden ser iguales.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(this.Title) || string.IsNullOrWhiteSpace(this.Description) || this.SelectedZone == null)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos obligatorios.", "OK");
                    return;
                }

                TimeZoneInfo userTimeZone = this.SelectedZone;
                DateTime startDateTimeLocal = DateTime.SpecifyKind(this.Date + this.StartTime, DateTimeKind.Unspecified);
                DateTime endDateTimeLocal = DateTime.SpecifyKind(this.Date + this.EndTime, DateTimeKind.Unspecified);

                DateTimeOffset eventStart = TimeZoneInfo.ConvertTimeToUtc(startDateTimeLocal, userTimeZone);
                DateTimeOffset eventEnd = TimeZoneInfo.ConvertTimeToUtc(endDateTimeLocal, userTimeZone);

                string icsContent = GenerateICSContent(this.Title, this.Description, eventStart, eventEnd);

                var newEvent = new Event
                {
                    Email = this.User.Email,
                    Title = this.Title,
                    Description = this.Description,
                    StartTime = eventStart.TimeOfDay,
                    EndTime = eventEnd.TimeOfDay,
                    Zone = this.SelectedZone.DaylightName,
                    ICSContent = icsContent,
                    Date =  DateTime.SpecifyKind(this.Date, DateTimeKind.Utc)
                };

                var result = await this.httpService.PostAsync<ResponseData>(newEvent, Common.Constants.SaveEvent);

                if (result.Success) 
                {
                    await App.Current.MainPage.DisplayAlert("Creado", "Se creo correctamente el evento.", "OK");
                }

            }
            catch(Exception ex)
            {
                await this.NotificationService.NotifyErrorAsync("Error", "Hubo un error al crear al evento");
            }
        }

        private void PopulateTimeZones()
        {
            this.Zones = new ObservableCollection<TimeZoneInfo>(
                 TimeZoneInfo.GetSystemTimeZones()
                     .Where(tz => tz.Id.Contains("America/"))
                     .Select(tz => tz)
             );
        }

        private string GenerateICSContent(string title, string description, DateTimeOffset startDateTime, DateTimeOffset endDateTime)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"SUMMARY:{title}");
            sb.AppendLine($"DESCRIPTION:{description}");
            sb.AppendLine($"DTSTART;TZID={selectedZone}:{startDateTime.ToString("yyyyMMddTHHmmssZ")}");
            sb.AppendLine($"DTEND;TZID={selectedZone}:{endDateTime.ToString("yyyyMMddTHHmmssZ")}");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

    }
}
