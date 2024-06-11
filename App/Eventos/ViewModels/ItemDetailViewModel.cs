using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Logic to show info from an item selected
    /// </summary>
    public class ItemDetailViewModel : BaseViewModel
    {
      
        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public ItemDetailViewModel(IServiceProvider provider) : base(provider)
        {
        }
    }
}
