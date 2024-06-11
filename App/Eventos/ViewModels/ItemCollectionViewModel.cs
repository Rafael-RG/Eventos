using Eventos.Common.Interfaces;
using Eventos.Common.ViewModels;


namespace Eventos.ViewModels
{
    /// <summary>
    /// Sample viewmodel to show a collection of items
    /// </summary>
    public class ItemCollectionViewModel : BaseViewModel
    {
      
        /// <summary>
        /// Gets by DI the required services
        /// </summary>
        public ItemCollectionViewModel(IServiceProvider provider) : base(provider)
        {
        }
    }
}
