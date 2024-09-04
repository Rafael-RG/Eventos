using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common.ViewModels;
using Eventos.GoogleAuth;
using Eventos.Models;
using Eventos.Common.Interfaces;
using Eventos.Common;

namespace Eventos.ViewModels
{
    /// <summary>
    /// Simple username / password login logic
    /// </summary>
    public partial class LoginViewModel : BaseViewModel
    {
        private IHttpService httpService;

        [ObservableProperty]
        private bool isVisibleLogin;


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
        private string userEmail;

        [ObservableProperty]
        private string fullName;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string repeatPassword;

        [ObservableProperty]
        private string country;

        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public LoginViewModel(IServiceProvider provider, IGoogleAuthService googleAuthService, IHttpService httpService) : base(provider)
        {
            this.httpService = httpService;
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void Login()
        {
            User loggedUser = null;

            //if (loggedUser == null)
            //{
            //    loggedUser = new User();
            //}

            if (loggedUser != null)
            {
                AppShell.User = new User
                {
                    Email = loggedUser.Email,
                    FullName = loggedUser.FullName,
                    UserName = loggedUser.UserName
                };

                await Shell.Current.GoToAsync("///HomePage", false);
            }

            //await Shell.Current.GoToAsync("///HomePage", false);

        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void RecoverPassword()
        {
            await Shell.Current.GoToAsync("///RecoverPage", false);
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
                Email = this.UserEmail,
                Password = this.Password,
                Country = this.Country,
                RetryValidate = false
            };

            var result = await this.httpService.PostAsync<ResponseData>(newUser, Constants.CreateUser);

            if (result.Success)
            {
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
            var validateRegistry = new NewUser
            {
                Email = this.UserEmail,
                RetryValidate = true,
                Password = this.Password,
                Country = this.Country,
                FullName = this.FullName
            };

            var result = await this.httpService.PostAsync<ResponseData>(validateRegistry, Constants.ValidateRegistry);

            if (result.Success)
            {
                TimeSpan maxDuration = TimeSpan.FromSeconds(30);
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
            }
            else
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
                Email = this.UserEmail,
                Code = this.ValidateCode
            };

            var result = await this.httpService.PostAsync<ResponseData>(validateRegistry, Constants.ValidateRegistry);

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
                User loggedUser = null;

                //if (loggedUser == null)
                //{
                //    loggedUser = new User();
                //}

                if (loggedUser != null)
                {
                    AppShell.User = new User
                    {
                        Email = loggedUser.Email,
                        FullName = loggedUser.FullName,
                        UserName = loggedUser.UserName
                    };

                    await Shell.Current.GoToAsync("///HomePage", false);
                }

                ChangeView("Login");

                await Shell.Current.GoToAsync("///LoginPage", false);
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

                    var response = await this.httpService.PostAsync<ResponseData>(emailData, url, headers);

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
                    break;
                case "Registry":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = true;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = false;
                    break;
                case "Recover":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = true;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = false;
                    break;
                case "ActivateAccount":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = true;
                    this.IsVisibleRegistrySuccess = false;
                    break;
                case "RegistrySuccess":
                    this.IsVisibleLogin = false;
                    this.IsVisibleRegistry = false;
                    this.IsVisibleRecoverPassword = false;
                    this.IsVisibleActivateAccount = false;
                    this.IsVisibleRegistrySuccess = true;
                    break;
            }
        }
    }
}