using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common.ViewModels;
using Eventos.GoogleAuth;
using Eventos.Models;
using Eventos.Common.Interfaces;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Text;

namespace Eventos.ViewModels
{
    /// <summary>
    /// Simple username / password login logic
    /// </summary>
    public partial class LoginViewModel : BaseViewModel
    {
        //private readonly IGoogleAuthService _googleAuthService;

        private IHttpService httpService;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string fullName;

        [ObservableProperty]
        private string repeatPassword;

        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public LoginViewModel(IServiceProvider provider, IGoogleAuthService googleAuthService, IHttpService httpService) : base(provider)
        {
            this.httpService = httpService;
            //_googleAuthService = googleAuthService;
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
        private async void RegistryUser()
        {
            await SendEmail();

            await Shell.Current.GoToAsync("///HomePage", false);

            //await Shell.Current.GoToAsync("///RegistryPage", false);
        }


        /// <summary>
        /// Validates if the user submitted the credentials
        /// </summary>
        /// <returns></returns>
        private bool AreCredentialComplete()
        {
            return this.Username.Length > 0 || this.Password.Length > 0;
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

                //await Shell.Current.GoToAsync("///HomePage", false);
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

                    var response = await this.httpService.PostAsync<ResponseData>(emailData, url,headers);

                    if (response.Success)
                    {
                        var responseBody = response.Data;
                        Console.WriteLine("Email sent successfully: " + responseBody);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.Message);
                        var responseBody =  response.Data;
                        Console.WriteLine("Details: " + responseBody);
                    }
                }
            }
            catch (Exception ex)
            {
                // Otro error
            }
        }
    }
}