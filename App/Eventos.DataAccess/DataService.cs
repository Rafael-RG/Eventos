﻿using Microsoft.EntityFrameworkCore;
using Eventos.Common.Interfaces;
using Eventos.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eventos.DataAccess
{ /// <summary>
  /// Data access service
  /// </summary>
    public class DataService : IDataService
    {

        /// <summary>
        /// Initialization
        /// </summary>
        public DataService()
        {
        }


        ///<inheritdoc/>
        public async Task<int> SaveItemsAsync<T>(IEnumerable<T> items, string tableName) where T : class
        {
            using (var databaseContext = new DatabaseContext())
            {
                if (!string.IsNullOrEmpty(tableName))
                {
                    await databaseContext.Database.ExecuteSqlRawAsync($"DELETE FROM {tableName}");
                }
                await databaseContext.Set<T>().AddRangeAsync(items);
                var itemsCount = await databaseContext.SaveChangesAsync().ConfigureAwait(false);
                return itemsCount;
            }
        }


        ///<inheritdoc/>
        public async Task<int> InsertOrUpdateItemsAsync<T>(T item) where T : class
        {
            using (var databaseContext = new DatabaseContext())
            {
                var itemsCount = await databaseContext.UpsertRange<T>(item).RunAsync();
                return itemsCount;
            }
        }


        ///<inheritdoc/>
        public async Task<int> DeleteItemAsync<T>(T item) where T : class
        {
            using (var databaseContext = new DatabaseContext())
            {
                databaseContext.Remove(item);
                var itemsCount = await databaseContext.SaveChangesAsync().ConfigureAwait(false);
                return itemsCount;
            }
        }


        ///<inheritdoc/>
        public async Task<User> LoadUserAsync()
        {
            using (var databaseContext = new DatabaseContext())
            {
                var user = await databaseContext.User.FirstOrDefaultAsync();
                return user;
            }
        }
    }
}
