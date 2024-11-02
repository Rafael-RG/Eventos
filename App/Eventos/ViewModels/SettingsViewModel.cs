using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common;
using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;
using Eventos.Models;
using Microcharts;
using Newtonsoft.Json;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Synchronization UI logic
    /// </summary>
    public partial class SettingsViewModel : BaseViewModel
    {
        [ObservableProperty]
        private User user;

        [ObservableProperty]
        private string newUserName;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string newPassword;

        [ObservableProperty]
        private string repeatNewPassword;

        [ObservableProperty]
        private bool isEditUserData;

        [ObservableProperty]
        private bool isVisibleSettings;

        [ObservableProperty]
        private bool isVisiblePassword;

        [ObservableProperty]
        private bool isRefreshingList;

        [ObservableProperty]
        private double progres;

        [ObservableProperty]
        private bool isFeedback;

        [ObservableProperty]
        private string feedback;


        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public SettingsViewModel(IServiceProvider provider) : base(provider)
        {

        }

        public override async void OnAppearing()
        {
            IsBusy = true;

            this.IsEditUserData = false;
            this.IsVisibleSettings = true;

            Refresh();

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
        private async void Suscribe()
        {
            //open link 
            await Launcher.OpenAsync("https://www.recuerdame.app/");
        }

        [RelayCommand]
        private async void Refresh()
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
                IsBusy = false;
            }
            catch
            {
                IsBusy = false;
                await App.Current.MainPage.DisplayAlert("Error", "No se ha podido cargar la información", "OK");
            }

            this.IsRefreshingList = false;
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void Logout()
        {
            this.IsBusy = true;
            try
            {
                await this.DataService.DeleteItemAsync(this.User);

                await Shell.Current.GoToAsync("///LoginPage", false);
            }
            catch
            {

            }
            this.IsBusy = false;
        }


        /// <summary>
        /// Change user data active
        /// </summary>
        [RelayCommand]
        private void EditUserData()
        {
            this.IsEditUserData = !this.IsEditUserData;

            this.IsVisibleSettings = !this.IsEditUserData;

            this.IsFeedback = false;

            this.IsVisiblePassword = false;

            if (this.IsEditUserData)
            {
                this.NewUserName = this.User.FullName;
                this.Password = string.Empty;
                this.NewPassword = string.Empty;
                this.RepeatNewPassword = string.Empty;
            }
        }

        /// <summary>
        /// Change user data
        /// </summary>
        [RelayCommand]
        private async void ChangeUserData()
        {
            this.IsBusy = true;
            try
            {
                var newData = new ChangeUserData();

                newData.Email = this.User.Email;
                newData.NewName = this.NewUserName;

                var message = string.Empty;

                if (string.IsNullOrEmpty(this.NewUserName))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Nombre de usuario invalido", "OK");
                    this.IsBusy = false;
                    return;
                }

                if (this.User.FullName != this.NewUserName)
                {
                    message = $"Datos Actualizado!";
                }

                if (!string.IsNullOrEmpty(this.Password))
                {
                    if (!string.IsNullOrEmpty(this.NewPassword) && !string.IsNullOrEmpty(this.RepeatNewPassword) && this.NewPassword != this.RepeatNewPassword)
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                        this.IsBusy = false;
                        return;
                    }
                    else if (!string.IsNullOrEmpty(this.NewPassword) && !string.IsNullOrEmpty(this.RepeatNewPassword) && this.NewPassword == this.RepeatNewPassword)
                    {
                        if (!Regex.IsMatch(this.NewPassword, @"^(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@#$%&*!._+-]{8,}$"))
                        {
                            await App.Current.MainPage.DisplayAlert("Error", "La nueva contraseña debe comenzar por una letra, contener mayúsculas, números y tener al menos 8 caracteres.", "OK");
                            this.IsBusy = false;
                            return;
                        }
                        else
                        {
                            newData.NewPassword = this.NewPassword;
                            newData.Password = this.Password;
                            message = $"Datos actualizados!";
                        }
                    }
                }

                if (string.IsNullOrEmpty(message))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "No se ha realizado ningún cambio", "OK");
                    this.IsBusy = false;
                    return;
                }

                var result = await this.HttpService.PostAsync<ResponseData>(newData, Constants.ChangeUserData);

                if (result.Success)
                {
                    await App.Current.MainPage.DisplayAlert("Exito", message, "OK");

                    this.User = await this.DataService.LoadUserAsync();

                    await this.DataService.DeleteItemAsync(this.User);

                    this.User.FullName = this.NewUserName;

                    var saved = await this.DataService.InsertOrUpdateItemsAsync(this.User);
                    this.IsEditUserData = false;
                    this.IsVisibleSettings = true;
                    this.IsBusy = false;

                }
                else
                {
                    this.IsBusy = false;
                    await App.Current.MainPage.DisplayAlert("Error", result.Data.ToString(), "OK");
                }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "OK");
                this.IsBusy = false;
            }
        }

        [RelayCommand]
        private void ViewPassword()
        {
            this.IsVisiblePassword = !this.IsVisiblePassword;
        }

        [RelayCommand]
        private async void SendFeedbackAsync()
        {
            this.IsBusy = true;
            try
            {
                if (string.IsNullOrEmpty(this.Feedback))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Debe ingresar un mensaje", "OK");
                    this.IsBusy = false;
                    return;
                }

                var feedback = new Feedback
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = this.User.Email,
                    Message = this.Feedback,
                    Created = DateTime.Now
                };


                var result = await this.HttpService.PostAsync<ResponseData>(feedback, Constants.Feedback);

                if (result.Success)
                {
                    await App.Current.MainPage.DisplayAlert("Enviado", "¡Gracias por tomarte el tiempo de compartir tu feedback!", "Cerrar");
                    this.Feedback = string.Empty;

                    this.IsFeedback = false;
                    this.IsVisibleSettings = true;
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "Cerrar");
                }
                this.IsBusy = false;
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "OK");
                this.IsBusy = false;
            }
        }

        [RelayCommand]
        private void CloseFeedback()
        {
            this.IsFeedback = false;
            this.IsEditUserData = false;
            this.IsVisibleSettings = true;
        }

        [RelayCommand]
        private void OpenFeedback()
        {
            this.IsEditUserData = false;
            this.IsVisibleSettings = false;
            this.IsFeedback = true;
            this.Feedback = string.Empty;
        }
    }
}
