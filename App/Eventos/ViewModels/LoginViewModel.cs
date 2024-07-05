using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common.ViewModels;
using Eventos.GoogleAuth;
using Eventos.Models;

namespace Eventos.ViewModels
{
    /// <summary>
    /// Simple username / password login logic
    /// </summary>
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IGoogleAuthService _googleAuthService;


        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;


        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public LoginViewModel(IServiceProvider provider, IGoogleAuthService googleAuthService) : base(provider)
        {
            _googleAuthService = googleAuthService;
        }

        /// <summary>
        /// Login with active directory
        /// </summary>
        [RelayCommand]
        private async void LoginWithGoogle()
        {
            var loggedUser = await _googleAuthService.GetCurrrentUserAsync();

            if (loggedUser == null)
            {
                loggedUser = await _googleAuthService.AuthenticateAsync();
            }

            if (loggedUser != null)
            {
                AppShell.User = new User
                {
                    Email = loggedUser.Email,
                    FullName = loggedUser.FullName,
                    UserName = loggedUser.UserName
                };

                await Shell.Current.GoToAsync("///HomePage",false);
            }
            
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
                var loggedUser = await _googleAuthService.GetCurrrentUserAsync();

                if (loggedUser == null)
                {
                    loggedUser = await _googleAuthService.AuthenticateAsync();
                }

                if (loggedUser != null)
                {
                    AppShell.User = new User
                    {
                        Email = loggedUser.Email,
                        FullName = loggedUser.FullName,
                        UserName = loggedUser.UserName
                    };

                    await Shell.Current.GoToAsync("///HomePage",false);
                }
            }
            catch (Exception ex)
            {
                this.IsBusy = false;
                await NotificationService.NotifyAsync(GetText("Error"), (ex.Message), GetText("Close"));
                await LogExceptionAsync(ex);
            }
            
        }
       

    }
}
