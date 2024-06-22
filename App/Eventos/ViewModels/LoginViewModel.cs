using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Common.ViewModels;
using Eventos.GoogleAuth;

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
                await this.NavigationService.Close(this);
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
                await this.AuthenticationService.LoadCredentialsAsync();
                if (this.AuthenticationService.IsAuthenticated())
                {
                    // shows the last username and 
                    // add a small delay to allow the UI to update itself before continuing
                    this.IsBusy = true;
                    this.Username = this.AuthenticationService.User.Name;
                    this.Password = "*******";
                    await Task.Delay(250);
                    await this.NavigationService.Close(this);
                }
            }
            catch (Exception ex)
            {
                this.IsBusy = false;
                //await NotificationService.NotifyAsync(GetText("Error"), (ex.Message), GetText("Close"));
                await LogExceptionAsync(ex);
            }
            
        }
       

    }
}
