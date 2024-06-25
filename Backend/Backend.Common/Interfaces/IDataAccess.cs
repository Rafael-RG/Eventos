using Backend.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Backend.Common.Interfaces
{
    /// <summary>
    /// Data Access interface
    /// </summary>
    public interface IDataAccess
    {
        /// <summary>
        /// Documents collection
        /// </summary>
        //IRepository<Document> Documents { get; }

     
        /// <summary>
        /// Saves all the changess
        /// </summary>
        //Task<int> SaveChangesAsync();

        /// <summary>
        /// Get sas token
        /// </summary>
        string GetSasToken(string container, int expiresOnMinutes);

        /// <summary>
        /// Create a new blob
        /// </summary>
        Task<Uri> CreateBlobAsync(Stream content, string filenanem, string containerName);

        /// <summary>
        /// Create a new item
        /// </summary>
        Task<bool> SaveEntryAsync(EventEntry newEvent);

        /// <summary>
        /// Get events by email
        /// </summary>
        Task<List<EventEntry>> GetEventsAsync(string email);

    }
}