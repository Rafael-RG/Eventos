using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Common.Interfaces
{
    public interface IHttpService
    {

        /// <summary>
        /// Executes a post method 
        /// </summary>
        Task<T> PostAsync<T>(object postData, string uri, Dictionary<string, string> headers = null, JsonSerializerSettings settings = null);


        /// <summary>
        /// Executes a GET method to the service
        /// </summary>   
        Task<JObject> GetJsonObjectAsync(string uri, JsonSerializerSettings settings = null);



        /// <summary>
        /// Executes a GET method to the service
        /// </summary>   
        Task<JObject[]> GetJsonArrayAsync(string uri, JsonSerializerSettings settings = null);
    }
}
