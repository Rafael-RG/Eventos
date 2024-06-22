using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Logic to show info from an item selected
    /// </summary>
    public class EventDetailViewModel : BaseViewModel
    {
      
        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public EventDetailViewModel(IServiceProvider provider) : base(provider)
        {
        }
    }
}
