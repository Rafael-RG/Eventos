using Eventos.Models;

namespace Eventos.Common.Interfaces
{

    /// <summary>
    /// Data access service interface
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Save a collections of items
        /// </summary>
        Task<int> SaveItemsAsync<T>(IEnumerable<T> items, string tableName) where T : class;

        /// <summary>
        /// Save a collections of items 
        /// </summary>
        Task<int> InsertOrUpdateItemsAsync<T>(T item) where T : class;
     
        
        /// <summary>
        /// Remove a item
        /// </summary>
        Task<int> DeleteItemAsync<T>(T item) where T : class;


        /// <summary>
        /// Loads items from the local storage
        /// </summary>
        Task<User> LoadUserAsync();
    }
}
