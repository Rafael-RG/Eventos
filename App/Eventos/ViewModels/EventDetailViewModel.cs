using CommunityToolkit.Mvvm.ComponentModel;
using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;
using Eventos.Models;
using Eventos.Common;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Logic to show info from an item selected
    /// </summary>
    public partial class EventDetailViewModel : BaseViewModel
    {

        [ObservableProperty]
        private EventItem eventItem;

        [ObservableProperty]
        private EventItem updatedEventItem;

        [ObservableProperty]
        private bool updateView;

        [ObservableProperty]
        private ObservableCollection<TimeZoneInfo> zones;

        [ObservableProperty]
        private TimeZoneInfo selectedZone;

        [ObservableProperty]
        private TimeSpan startTime;

        [ObservableProperty]
        private TimeSpan endTime;

        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public EventDetailViewModel(IServiceProvider provider, EventItem eventItem) : base(provider)
        {
            this.EventItem = eventItem;
            this.UpdatedEventItem = eventItem;
        }

        public override async void OnAppearing()
        {
            IsBusy = true;
            try
            {
                this.UpdateView = false;
            }
            catch (Exception ex)
            {
                await this.NotificationService.NotifyErrorAsync("Error", "Hubo un error al obtener el evento");
            }

            IsBusy = false;
        }


        [RelayCommand]
        private void ActiveUpdateEvent()
        {
            if (this.Zones == null || !this.Zones.Any())
            {
                PopulateTimeZones();
            }
            this.SelectedZone = TimeZoneInfo.FindSystemTimeZoneById(this.EventItem.ZoneId);

            DateTimeOffset dateTimeOffsetStart = DateTimeOffset.Parse(this.EventItem.StartTime);
            DateTimeOffset dateTimeOffsetEnd = DateTimeOffset.Parse(this.EventItem.EndTime);

            TimeSpan startTime = dateTimeOffsetStart.TimeOfDay; 
            TimeSpan endTime = dateTimeOffsetEnd.TimeOfDay;

            // Si necesitas los valores originales en UTC
            //TimeSpan eventStartTimeUTC = eventStart.UtcDateTime.TimeOfDay;
            //TimeSpan eventEndTimeUTC = eventEnd.UtcDateTime.TimeOfDay;


            this.StartTime = startTime;
            this.EndTime = endTime;
            this.UpdateView = true;
        }

        [RelayCommand]
        private void CancelUpdateView()
        {
            this.UpdateView = false;
        }

        /// <summary>
        /// Update Event
        /// </summary>
        [RelayCommand]
        private async void UpdateEvent(string updateOrDelete)
        {
            IsBusy = true;
            try
            {
                var newEvent = new Event();

                if (updateOrDelete == "delete")
                {
                    this.UpdatedEventItem.IsDelete = true;
                    newEvent.IsDelete = this.UpdatedEventItem.IsDelete;
                    newEvent.RowKey = this.UpdatedEventItem.RowKey;
                }
                else 
                {
                    if (this.UpdatedEventItem.StartTime == this.UpdatedEventItem.EndTime)
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "La hora de inicio y fin no pueden ser iguales.", "OK");
                        this.IsBusy = false;
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(this.UpdatedEventItem.Title) || string.IsNullOrWhiteSpace(this.UpdatedEventItem.Description) || this.SelectedZone == null)
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Por favor, complete todos los campos obligatorios.", "OK");
                        this.IsBusy = false;
                        return;
                    }

                    TimeZoneInfo userTimeZone = this.SelectedZone;
                    DateTime startDateTimeLocal = DateTime.SpecifyKind(this.UpdatedEventItem.Date + this.StartTime, DateTimeKind.Unspecified);
                    DateTime endDateTimeLocal = DateTime.SpecifyKind(this.UpdatedEventItem.Date + this.EndTime, DateTimeKind.Unspecified);

                    DateTimeOffset eventStart = TimeZoneInfo.ConvertTimeToUtc(startDateTimeLocal, userTimeZone);
                    DateTimeOffset eventEnd = TimeZoneInfo.ConvertTimeToUtc(endDateTimeLocal, userTimeZone);

                    
                    newEvent.Title = this.UpdatedEventItem.Title;
                    newEvent.Description = this.UpdatedEventItem.Description;
                    newEvent.StartTime = eventStart;
                    newEvent.EndTime = eventEnd;
                    newEvent.Date = this.UpdatedEventItem.Date;
                    newEvent.Zone = this.SelectedZone.DisplayName;
                    newEvent.ZoneId = this.SelectedZone.Id;
                    newEvent.RowKey = this.UpdatedEventItem.RowKey;
                    newEvent.EventURl = this.UpdatedEventItem.EventURl;
                }

                var uri = Constants.SaveEvent;

                var response = await this.HttpService.PostAsync<ResponseData>(newEvent, uri);

                if (response.Success)
                {
                    this.UpdateView = false;
                    this.EventItem = this.UpdatedEventItem;
                    if (updateOrDelete == "delete")
                    {
                        await App.Current.MainPage.DisplayAlert("Eliminado", "Se elimino correctamente el evento.", "OK");
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Actualizado", "Se actualizo correctamente el evento.", "OK");
                    }
                }
                else
                {
                    if (updateOrDelete == "delete")
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Hubo un error al eliminar el evento.", "OK");
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Hubo un error al actualizar el evento.", "OK");
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
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
