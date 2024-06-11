using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Synchronization UI logic
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
      
        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public SettingsViewModel(IServiceProvider provider) : base(provider)
        {
        }
    }
}
