using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common;
using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;
using Eventos.Models;
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

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void Logout()
        {
            this.IsBusy = true;
            await this.DataService.DeleteItemAsync(AppShell.User);

            await Shell.Current.GoToAsync("///LoginPage", false);
            this.IsBusy = false;
        }


        /// <summary>
        /// Change user data active
        /// </summary>
        [RelayCommand]
        private void EditUserData()
        {
            this.IsEditUserData = !this.IsEditUserData;

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
                    if(!Regex.IsMatch(this.NewPassword, @"^[a-zA-Z](?=.*[A-Z])(?=.*\d).{7,}$"))
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "La nueva contraseña debe comenzar por una letra, contener mayúsculas, números y tener al menos 8 caracteres.", "OK");
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

                this.User.FullName = this.NewUserName;

                await this.DataService.DeleteItemAsync(AppShell.User);

                var saved = await this.DataService.InsertOrUpdateItemsAsync(this.User);
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", result.Data.ToString(), "OK");
            }

            this.IsBusy = false;
        }
    }
}
