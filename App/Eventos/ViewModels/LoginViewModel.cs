using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common.ViewModels;
using Eventos.GoogleAuth;
using Eventos.Models;
using Eventos.Common.Interfaces;
using Eventos.Common;
using Microsoft.Maui;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

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
        public LoginViewModel(IServiceProvider provider, IGoogleAuthService googleAuthService) : base(provider)
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
            if (this.UserEmailLogin == null || this.PasswordLogin == null || this.UserEmailLogin?.Length == 0 || this.PasswordLogin?.Length == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Todos los campos son requeridos", "OK");
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
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", response.Message, "OK");
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
                this.IsActiveSendCode = true;
                this.ValidateCode = string.Empty;
                ChangeView("ValidateRecoverPassword");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "El correo electrónico no está registrado", "OK");
                return;
            }
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void ValidateRecoverPassword()
        {
            if (this.ValidateCode.Length < 4)
            {
                await App.Current.MainPage.DisplayAlert("Error", "El código de validación debe tener 4 dígitos", "OK");
                return;
            }

            var validateCode = new ValidateRegistry
            {
                Email = this.UserEmail,
                Code = this.ValidateCode
            };

            var result = await this.HttpService.PostAsync<ResponseData>(validateCode, Constants.ValidateCode);

            if (result.Success)
            {
                ChangeView("CreateNewPassword");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
            }
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void CreateNewPassword()
        {
            if(this.Password != this.RepeatPassword)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(this.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$"))
            {
                await App.Current.MainPage.DisplayAlert("Error", "La contraseña debe comenzar por una letra, contener mayúsculas, números y tener al menos 8 caracteres.", "OK");
                return;
            }

            var recoveryPassword = new RecoveryPassword
            {
                Email = this.UserEmail,
                Password = this.Password
            };

            var result = await this.HttpService.PostAsync<ResponseData>(recoveryPassword, Constants.RecoverPassword);

            if (result.Success) 
            {
                ChangeView("FinishedRecoverPassword");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
            }
        }

        [RelayCommand]
        private async void ResendCodeRecovery()
        {
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
                await App.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
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
            if (this.Password != this.RepeatPassword)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            if (this.FullName.Length == 0 || this.Password.Length == 0 || this.UserEmail.Length == 0 || this.Country.Length == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Todos los campos son requeridos", "OK");
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(this.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$"))
            {
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
                this.IsActiveSendCode = true;
                this.ValidateCode = string.Empty;
                ChangeView("ActivateAccount");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
            }
        }

        [RelayCommand]
        private async void ResendCode()
        {
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
                Email = this.UserEmail,
                RetryValidate = true,
                Password = this.Password,
                Country = this.Country,
                FullName = this.FullName
            };

            var result = await this.HttpService.PostAsync<ResponseData>(newUser, Constants.CreateUser);

            if (!result.Success)
            {
                await App.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
            }
        }

        [RelayCommand]
        private async void ValidateAccount()
        {
            if (this.ValidateCode.Length < 4)
            {
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
                ChangeView("RegistrySuccess");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
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
            try
            {
                var user = await this.DataService.LoadUserAsync();

                if (user != null)
                {
                    AppShell.User = user;
                    await Shell.Current.GoToAsync("///HomePage", new Dictionary<string, object>
                    {
                        { "User", user }
                    });
                }
                else 
                {
                    ClearData();

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

        public async Task SendEmail()
        {
            try
            {
                var apiKey = "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";
                var url = "https://api.brevo.com/v3/emailCampaigns";

                var emailData = new
                {
                    sender = new
                    {
                        name = "Recuerdame soporte",
                        email = "recuerdame.soporte@outlook.com"
                    },
                    to = new[]
                    {
                new { email = "rafargg11@gmail.com.com", name = "Rafael" }
            },
                    subject = "Hello world",
                    htmlContent = "<html><head></head><body><p>Hello,</p>This is my first transactional email sent from Brevo.</p></body></html>"
                };


                using (var client = new HttpClient())
                {
                    var headers = new Dictionary<string, string>
                    {
                        { "api-key", apiKey },
                    };

                    var response = await this.HttpService.PostAsync<ResponseData>(emailData, url, headers);

                    if (response.Success)
                    {
                        var responseBody = response.Data;
                        Console.WriteLine("Email sent successfully: " + responseBody);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.Message);
                        var responseBody = response.Data;
                        Console.WriteLine("Details: " + responseBody);
                    }
                }
            }
            catch (Exception ex)
            {
                // Otro error
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