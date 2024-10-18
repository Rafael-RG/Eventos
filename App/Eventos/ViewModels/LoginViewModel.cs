using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common.ViewModels;
using Eventos.Models;
using Eventos.Common.Interfaces;
using Eventos.Common;
using Microsoft.Maui;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Eventos.ViewModels
{
    /// <summary>
    /// Simple username / password login logic
    /// </summary>
    public partial class LoginViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool isVisibleLogin;

        [ObservableProperty]
        private string userEmailLogin;

        [ObservableProperty]
        private string passwordLogin;

        [ObservableProperty]
        private bool isVisiblePassword;

        [ObservableProperty]
        private bool isVisibleRegistry;

        [ObservableProperty]
        private bool isVisibleActivateAccount;

        [ObservableProperty]
        private bool isVisibleRegistrySuccess;

        [ObservableProperty]
        private string validateCode;

        [ObservableProperty]
        private bool isActiveSendCode;

        [ObservableProperty]
        private IDispatcherTimer timer;

        [ObservableProperty]
        private string timerText;



        [ObservableProperty]
        private bool isVisibleRecoverPassword;

        [ObservableProperty]
        private bool isVisibleValidateRecoverPassword;

        [ObservableProperty]
        private bool isVisibleCreateNewPassword;

        [ObservableProperty]
        private bool isVisibleFinishedRecoverPassword;



        [ObservableProperty]
        private string userEmail;

        [ObservableProperty]
        private string fullName;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string repeatPassword;

        [ObservableProperty]
        private string country;

        [ObservableProperty]
        private ObservableCollection<string> countries;


        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public LoginViewModel(IServiceProvider provider) : base(provider)
        {
            ClearData();
            ChangeView("Login");
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void Login()
        {
            this.IsBusy = true;
            try
            {
                if (this.UserEmailLogin == null || this.PasswordLogin == null || this.UserEmailLogin?.Length == 0 || this.PasswordLogin?.Length == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Todos los campos son requeridos", "OK");
                    this.IsBusy = false;
                    return;
                }

                var user = new RecoveryPassword
                {
                    Email = this.UserEmailLogin.ToLower(),
                    Password = this.PasswordLogin
                };

                var response = await this.HttpService.PostAsync<ResponseData>(user, Constants.Login);
                if (response.Success)
                {
                    var dataUser = JsonConvert.DeserializeObject<LoginUser>(response.Data.ToString());

                    User loggedUser = new User
                    {
                        Email = this.UserEmailLogin.ToLower(),
                        FullName = dataUser.FullName
                    };

                    //save in local storage
                    var result = await this.DataService.InsertOrUpdateItemsAsync(loggedUser);

                    if (result > 0)
                    {
                        AppShell.User = loggedUser;
                        this.UserEmailLogin = string.Empty;
                        this.PasswordLogin = string.Empty;
                        await Shell.Current.GoToAsync("///HomePage", new Dictionary<string, object>
                    {
                        { "User", loggedUser }
                    });
                    }
                    this.IsBusy = false;
                }
                else
                {
                    this.IsBusy = false;
                    await App.Current.MainPage.DisplayAlert("Error", response.Message, "OK");
                }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrion un error al iniciar sesion. Vuelva a intentar", "OK");
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private void RecoverPassword()
        {
            ClearData();
            ChangeView("Recover");
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void FindUserRecoverPassword()
        {
            this.IsBusy=true;
            try{
            if (this.UserEmail.Length == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "El correo electrónico es requerido", "OK");
                return;
            }

            var email = new
            {
                Email = this.UserEmail.ToLower()
            };

            var result = await this.HttpService.PostAsync<ResponseData>(email, Constants.FindUserAndSendCode);

            if (result.Success) 
            {
                this.IsBusy=false;
                this.IsActiveSendCode = true;
                this.ValidateCode = string.Empty;
                ChangeView("ValidateRecoverPassword");
            }
            else
            {
                this.IsBusy=false;
                await App.Current.MainPage.DisplayAlert("Error", "El correo electrónico no está registrado", "OK");
                return;
            }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrion un error buscando el usuario. Vuelva a intentar", "OK");
                this.IsBusy=false;
            }
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void ValidateRecoverPassword()
        {
            this.IsBusy=true;
            try{
            if (this.ValidateCode.Length < 4)
            {
                await App.Current.MainPage.DisplayAlert("Error", "El código de validación debe tener 4 dígitos", "OK");
                this.IsBusy=false;
                return;
            }

            var validateCode = new ValidateRegistry
            {
                Email = this.UserEmail.ToLower(),
                Code = this.ValidateCode
            };

            var result = await this.HttpService.PostAsync<ResponseData>(validateCode, Constants.ValidateCode);

            if (result.Success)
            {
                this.IsBusy=false;
                ChangeView("CreateNewPassword");
            }
            else
            {
                this.IsBusy=false;
                await App.Current.MainPage.DisplayAlert("Error", "El código no es correcto", "OK");
            }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "OK");
                this.IsBusy=false;
            }
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void CreateNewPassword()
        {
            try{
            if(this.Password != this.RepeatPassword)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            if (!Regex.IsMatch(this.Password, @"^(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@#$%&*!._+-]{8,}$"))
            {
                await App.Current.MainPage.DisplayAlert("Error", "La contraseña debe comenzar por una letra, contener mayúsculas, números y tener al menos 8 caracteres.", "OK");
                return;
            }

            var recoveryPassword = new RecoveryPassword
            {
                Email = this.UserEmail.ToLower(),
                Password = this.Password
            };

            var result = await this.HttpService.PostAsync<ResponseData>(recoveryPassword, Constants.RecoverPassword);

            if (result.Success) 
            {
                ChangeView("FinishedRecoverPassword");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "OK");
            }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "OK");
                this.IsBusy=false;
            }
        }

        [RelayCommand]
        private async void ResendCodeRecovery()
        {
            try{
            this.IsActiveSendCode = false;

            // Inicializar el temporizador

            this.Timer = Dispatcher.GetForCurrentThread().CreateTimer();

            TimeSpan maxDuration = TimeSpan.FromSeconds(10);
            this.Timer.Interval = TimeSpan.FromSeconds(1);
            this.Timer.Tick += (s, e) =>
            {
                // Restar 1 segundo a la duración máxima
                maxDuration = maxDuration.Subtract(TimeSpan.FromSeconds(1));

                // Actualizar el texto del temporizador
                this.TimerText = string.Format("{0:ss}", maxDuration);

                // Verificar si el tiempo ha llegado a cero
                if (maxDuration.TotalSeconds <= 0)
                {
                    // Detener el temporizador y ejecutar la acción final
                    this.Timer.Stop();
                    this.IsActiveSendCode = true;
                }
            };

            // Iniciar el temporizador
            this.Timer.Start();

            var userEmail = new
            {
                Email = this.UserEmail.ToLower()
            };

            var result = await this.HttpService.PostAsync<ResponseData>(userEmail, Constants.FindUserAndSendCode);

            if (!result.Success)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuelva a intentar", "OK");
            }
            this.IsBusy=false;
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "OK");
                this.IsBusy=false;
            }
        }

        [RelayCommand]
        private void BackView(string view)
        {
            ChangeView(view);
        }

        [RelayCommand]
        private void RegistryUser()
        {
            ClearData();
            ChangeView("Registry");
        }

        [RelayCommand]
        private async void CreateUser()
        {
            this.IsBusy=true;
            try{
            if (this.Password != this.RepeatPassword)
            {
                this.IsBusy=false;
                await App.Current.MainPage.DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            if (this.FullName.Length == 0 || this.Password.Length == 0 || this.UserEmail.Length == 0 || this.Country.Length == 0)
            {
                this.IsBusy=false;
                await App.Current.MainPage.DisplayAlert("Error", "Todos los campos son requeridos", "OK");
                return;
            }

            if (!Regex.IsMatch(this.Password, @"^(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@#$%&*!._+-]{8,}$"))
            {
                this.IsBusy=false;
                await App.Current.MainPage.DisplayAlert("Error", "La contraseña debe comenzar por una letra, contener mayúsculas, números y tener al menos 8 caracteres.", "OK");
                return;
            }

            var newUser = new NewUser
            {
                FullName = this.FullName,
                Email = this.UserEmail.ToLower(),
                Password = this.Password,
                Country = this.Country,
                RetryValidate = false
            };

            var result = await this.HttpService.PostAsync<ResponseData>(newUser, Constants.CreateUser);

            if (result.Success)
            {
                this.IsBusy=false;
                this.IsActiveSendCode = true;
                this.ValidateCode = string.Empty;
                ChangeView("ActivateAccount");
            }
            else
            {
                this.IsBusy=false;
                await App.Current.MainPage.DisplayAlert("Error", "El usuario ya existe", "OK");
            }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "OK");
                this.IsBusy=false;
            }
        }

        [RelayCommand]
        private async void ResendCode()
        {
            try{
            this.IsActiveSendCode = false;

            // Inicializar el temporizador

            this.Timer = Dispatcher.GetForCurrentThread().CreateTimer();

            TimeSpan maxDuration = TimeSpan.FromSeconds(10);
            this.Timer.Interval = TimeSpan.FromSeconds(1);
            this.Timer.Tick += (s, e) =>
            {
                // Restar 1 segundo a la duración máxima
                maxDuration = maxDuration.Subtract(TimeSpan.FromSeconds(1));

                // Actualizar el texto del temporizador
                this.TimerText = string.Format("{0:ss}", maxDuration);

                // Verificar si el tiempo ha llegado a cero
                if (maxDuration.TotalSeconds <= 0)
                {
                    // Detener el temporizador y ejecutar la acción final
                    this.Timer.Stop();
                    this.IsActiveSendCode = true;
                }
            };

            // Iniciar el temporizador
            this.Timer.Start();

            var newUser = new NewUser
            {
                Email = this.UserEmail.ToLower(),
                RetryValidate = true,
                Password = this.Password,
                Country = this.Country,
                FullName = this.FullName
            };

            var result = await this.HttpService.PostAsync<ResponseData>(newUser, Constants.CreateUser);

            if (!result.Success)
            {
                this.IsBusy=false;
                await App.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
            }
            this.IsBusy=false;
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "OK");
                this.IsBusy=false;
            }

        }

        [RelayCommand]
        private async void ValidateAccount()
        {
            this.IsBusy=true;
            try{
            if (this.ValidateCode.Length < 4)
            {
                this.IsBusy=false;
                await App.Current.MainPage.DisplayAlert("Error", "El código de validación debe tener 4 dígitos", "OK");
                return;
            }

            var validateRegistry = new ValidateRegistry
            {
                Email = this.UserEmail.ToLower(),
                Code = this.ValidateCode
            };

            var result = await this.HttpService.PostAsync<ResponseData>(validateRegistry, Constants.ValidateRegistry);

            if (result.Success)
            {
                this.IsBusy=false;
                ChangeView("RegistrySuccess");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "El código no es correcto", "Reintentar");
            }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Ocurrio un error. Vuleva a intentar", "OK");
                this.IsBusy=false;
            }
        }

        [RelayCommand]
        private void FinishRegistry()
        {
            ClearData();
            ChangeView("Login");
        }

        [RelayCommand]
        private async void Suscribe()
        {
            //open link 
            await Launcher.OpenAsync("https://www.recuerdame.app/");
        }

        /// <summary>
        /// Validates if the user submitted the credentials
        /// </summary>
        /// <returns></returns>
        private bool AreCredentialComplete()
        {
            return this.UserEmail.Length > 0 || this.Password.Length > 0;
        }


        /// <summary>
        /// Login when appears
        /// </summary>
        public override async void OnAppearing()
        {
            this.IsBusy=true;
            try
            {
                var user = await this.DataService.LoadUserAsync();

                if (user != null)
                {
                    AppShell.User = user;
                    this.IsBusy=false;
                    await Shell.Current.GoToAsync("///HomePage", new Dictionary<string, object>
                    {
                        { "User", user }
                    });
                }
                else 
                {
                    ClearData();
                    this.IsBusy=false;
                    if(this.Countries == null || !this.Countries.Any())
                    {
                        PopulateContries();
                        this.Country = this.Countries.FirstOrDefault();
                    }

                    ChangeView("Login");
                }

            }
            catch (Exception ex)
            {
                this.IsBusy = false;
                await NotificationService.NotifyAsync(GetText("Error"), (ex.Message), GetText("Close"));
                await LogExceptionAsync(ex);
            }

        }

        private void ClearData()
        {
            this.FullName = string.Empty;
            this.Password = string.Empty;
            this.RepeatPassword = string.Empty;
            this.UserEmail = string.Empty;
            this.Country = string.Empty;
            this.ValidateCode = string.Empty;
            this.UserEmailLogin = string.Empty;
            this.PasswordLogin = string.Empty;
            this.IsActiveSendCode = true;
            this.IsVisiblePassword = false;
        }

        [RelayCommand]
        private void ViewPassword()
        {
            this.IsVisiblePassword = !this.IsVisiblePassword;
        }

        private void ChangeView(string view)
        {
            switch (view)
            {
                case "Login":
                    this.IsVisibleLogin = true;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = false;
                    this.IsVisibleValidateRecoverPassword = false;
                    this.IsVisibleCreateNewPassword = false;
                    this.IsVisibleFinishedRecoverPassword = false;
                    break;
                case "Registry":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = true;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = false;
                    this.IsVisibleValidateRecoverPassword = false;
                    this.IsVisibleCreateNewPassword = false;
                    this.IsVisibleFinishedRecoverPassword = false;
                    break;
                case "Recover":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = true;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = false;
                    this.IsVisibleValidateRecoverPassword = false;
                    this.IsVisibleCreateNewPassword = false;
                    this.IsVisibleFinishedRecoverPassword = false;
                    break;
                case "ActivateAccount":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = true;
                    this.IsVisibleRegistrySuccess = false;
                    this.IsVisibleValidateRecoverPassword = false;
                    this.IsVisibleCreateNewPassword = false;
                    this.IsVisibleFinishedRecoverPassword = false;
                    break;
                case "RegistrySuccess":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = true;
                    this.IsVisibleValidateRecoverPassword = false;
                    this.IsVisibleCreateNewPassword = false;
                    this.IsVisibleFinishedRecoverPassword = false;
                    break;
                case "ValidateRecoverPassword":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = false;
                    this.IsVisibleValidateRecoverPassword = true;
                    this.IsVisibleCreateNewPassword = false;
                    this.IsVisibleFinishedRecoverPassword = false;
                    break;
                case "CreateNewPassword":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = false;
                    this.IsVisibleValidateRecoverPassword = false;
                    this.IsVisibleCreateNewPassword = true;
                    this.IsVisibleFinishedRecoverPassword = false;
                    break;
                case "FinishedRecoverPassword":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = false;
                    this.IsVisibleValidateRecoverPassword = false;
                    this.IsVisibleCreateNewPassword = false;
                    this.IsVisibleFinishedRecoverPassword = true;
                    break;

            }
        }

        private void PopulateContries() 
        {
            var countriesOfAmerica = new[]
            {
                "Argentina",
                "Bolivia",
                "Brasil",
                "Canadá",
                "Chile",
                "Colombia",
                "Costa Rica",
                "Cuba",
                "Ecuador",
                "El Salvador",
                "Estados Unidos",
                "Granada",
                "Guatemala",
                "Guyana",
                "Haití",
                "Honduras",
                "Jamaica",
                "México",
                "Nicaragua",
                "Panamá",
                "Paraguay",
                "Perú",
                "Puerto Rico",
                "República Dominicana",
                "Surinam",
                "Trinidad y Tobago",
                "Uruguay",
                "Venezuela"
            };


            this.Countries = new ObservableCollection<string>(countriesOfAmerica);
        }
    }
}