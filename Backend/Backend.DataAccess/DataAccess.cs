using Azure;
using Azure.Data.Tables;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Backend.Common.Interfaces;
using Backend.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DataAccess
{
    /// <inheritdoc/>
    public class DataAccess : IDataAccess //, IDisposable
    {
        //private readonly DatabaseContext context;
        private bool disposed = false;
        private readonly string storageConnectionString;
        private readonly BlobServiceClient blobServiceClient;
        private readonly TableServiceClient tableServiceClient;
       
        
        /// <inheritdoc/>
        public IRepository<Document> Documents { get; }


        /// <summary>
        /// Gets the configuration
        /// </summary>
        public DataAccess(IConfiguration configuration)
        {
            this.storageConnectionString = configuration["StorageConnectionString"];
            this.blobServiceClient = new BlobServiceClient(this.storageConnectionString);  
            this.tableServiceClient = new TableServiceClient(this.storageConnectionString);
        }



        /// <inheritdoc/>
        public string GetSasToken(string container, int expiresOnMinutes)
        {
            // Generates the token for this account
            var accountKey = string.Empty;
            var accountName = string.Empty;
            var connectionStringValues = this.storageConnectionString.Split(';')
                .Select(s => s.Split([ '=' ], 2))
                .ToDictionary(s => s[0], s => s[1]);
            if (connectionStringValues.TryGetValue("AccountName", out var accountNameValue) && !string.IsNullOrWhiteSpace(accountNameValue)
                && connectionStringValues.TryGetValue("AccountKey", out var accountKeyValue) && !string.IsNullOrWhiteSpace(accountKeyValue))
            {
                accountKey = accountKeyValue;
                accountName = accountNameValue;

                var storageSharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);
                var blobSasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = container,
                    ExpiresOn = DateTime.UtcNow + TimeSpan.FromMinutes(expiresOnMinutes)
                };

                blobSasBuilder.SetPermissions(BlobAccountSasPermissions.All);
                var queryParams = blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential);
                var sasToken = queryParams.ToString();
                return sasToken;
            }
            return string.Empty;
        }

        /// <summary>
        /// Crear un nuevo blob en un contenedor
        /// </summary>
        public async Task<Uri> CreateBlobAsync(Stream content, string filenanem, string containerName)
        {
            try
            {
                var containerClient = this.blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(filenanem);

                using (var stream = new MemoryStream())
                {
                    content.CopyTo(stream);
                    stream.Position = 0;
                    await blobClient.UploadAsync(stream, true);
                }

                // Devuelve el URI del blob
                return blobClient.Uri;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// create a new table in the storage account
        /// </summary>
        public async Task<bool> SaveEntryAsync(EventEntry newEvent)
        {
            try 
            {
                var tableClient = this.tableServiceClient.GetTableClient("events");
                await tableClient.CreateIfNotExistsAsync();

                await tableClient.UpsertEntityAsync(newEvent);

                
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        /// <summary>
        /// create a new table in the storage account
        /// </summary>
        public async Task<bool> SaveEventClickedDateInfoAsync(EventClickedInfoEntry eventClickedInfo)
        {
            try
            {
                var tableClient = this.tableServiceClient.GetTableClient("clickedinfo");
                await tableClient.CreateIfNotExistsAsync();

                await tableClient.UpsertEntityAsync(eventClickedInfo);


                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// create a new table in the storage account
        /// </summary>
        public async Task<bool> SaveNewUserAsync(UserEntry newUser)
        {
            try
            {
                var tableClient = this.tableServiceClient.GetTableClient("users");
                await tableClient.CreateIfNotExistsAsync();

                await tableClient.UpsertEntityAsync(newUser);

                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Get all the events from the table by email
        /// </summary>
        public async Task<UserEntry> GetUserAsync(string email)
        {
            try
            {
                var tableClient = this.tableServiceClient.GetTableClient("users");
                await tableClient.CreateIfNotExistsAsync();
                var query = tableClient.QueryAsync<UserEntry>(filter: $"PartitionKey eq '{email}'");
                var user = new UserEntry();
                await foreach (var item in query)
                {
                    user = item;
                    break;
                }
                return user;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get all the events from the table by email
        /// </summary>
        public async Task<List<EventEntry>> GetEventsAsync(string email)
        {
            try
            {
                var tableClient = this.tableServiceClient.GetTableClient("events");
                await tableClient.CreateIfNotExistsAsync();
                var query = tableClient.QueryAsync<EventEntry>(filter: $"PartitionKey eq '{email}'");
                var events = new List<EventEntry>();
                await foreach (var item in query)
                {
                    events.Add(item);
                }
                return events;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<EventClickedInfoEntry>> GetEventsClickedInfoAsync(string partitionKey)
        {
            try
            {
                var tableClient = this.tableServiceClient.GetTableClient("clickedinfo");
                await tableClient.CreateIfNotExistsAsync();
                var query = tableClient.QueryAsync<EventClickedInfoEntry>(filter: $"PartitionKey eq '{partitionKey}'");
                var eventClickedInfo = new List<EventClickedInfoEntry>();
                await foreach (var item in query)
                {
                    eventClickedInfo.Add(item);
                }
                return eventClickedInfo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get all the events from the table by email
        /// </summary>
        public async Task<EventEntry> GetEventAsync(string rowKey)
        {
            try
            {
                var tableClient = this.tableServiceClient.GetTableClient("events");
                await tableClient.CreateIfNotExistsAsync();
                var query = tableClient.QueryAsync<EventEntry>(filter: $"RowKey eq '{rowKey}'");
                var eventEntity = new EventEntry();
                
                await foreach (var item in query)
                {
                    eventEntity = item;
                    break;
                }

                return eventEntity;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get all the events from the table by email
        /// </summary>
        public async Task<List<PlanSuscribeEntry>> GetPlansAsync()
        {
            try
            {
                var tableClient = this.tableServiceClient.GetTableClient("planes");
                await tableClient.CreateIfNotExistsAsync();
                var query = tableClient.QueryAsync<PlanSuscribeEntry>(filter: $"PartitionKey eq 'recuerdame'");
                var eventEntity = new List<PlanSuscribeEntry>();

                await foreach (var item in query)
                {
                    eventEntity.Add(item);
                }

                return eventEntity;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DevUserEntry> GetDevUserAsync(string email)
        {
            try
            {
                var tableClient = this.tableServiceClient.GetTableClient("devuser");
                await tableClient.CreateIfNotExistsAsync();
                var query = tableClient.QueryAsync<DevUserEntry>(filter: $"RowKey eq '{email}'");
                var eventEntity = new DevUserEntry();

                await foreach (var item in query)
                {
                    eventEntity = item;
                    break;
                }

                return eventEntity;
            }
            catch
            {
                return null;
            }
        }
    }
}
