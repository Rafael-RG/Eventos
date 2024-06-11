using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Sample entry form
    /// </summary>
    public class EntryFormViewModel : BaseViewModel
    {
      
        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public EntryFormViewModel(IServiceProvider provider) : base(provider)
        {
        }
    }
}
