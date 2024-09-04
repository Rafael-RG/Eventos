using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Backend.Common.Extensions;
using Backend.Common.Models;
using Backend.Common.Interfaces;
using Backend.Models;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Text;
using System;
using System.Net.Http.Headers;


using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.AspNetCore.Http;

namespace Backend.Service.Functions
{
    /// <summary>
    /// Documents backend API
    /// </summary>
    public class Events
    {
        private readonly ILogger<Events> logger;
        private readonly IEventsLogic businessLogic;


        /// <summary>
        /// Receive all the depedencies by DI
        /// </summary>        
        public Events(IEventsLogic businessLogic, ILogger<Events> logger)
        {
            this.logger = logger;
            this.businessLogic = businessLogic;
        }


        /// <summary>
        /// Validate a subscription
        /// </summary>       
        [Function(nameof(ValidateSubscriptionAsync))]
        public async Task<HttpResponseData> ValidateSubscriptionAsync(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = "validatesubscription")] HttpRequestData request)
        {
            return await request.CreateResponse(this.businessLogic.ValidateSubscriptionAsync, request.DeserializeBody<ValidateSubscriptionRequest>(), responseLinks =>
            {
                responseLinks.Links = new Dictionary<string, string> { };
            }, logger);
        }


        /// <summary>
        /// Save event
        /// </summary>       
        [Function(nameof(SaveEventAsync))]
        public async Task<HttpResponseData> SaveEventAsync(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = "saveevent")] HttpRequestData request)
        {
            return await request.CreateResponse(this.businessLogic.SaveEventAsync, request.DeserializeBody<EventRequest>(), responseLinks =>
            {
                responseLinks.Links = new Dictionary<string, string> { };
            }, logger);
        }

        /// <summary>
        /// Validate a subscription
        /// </summary>       
        [Function(nameof(GetEventsAsync))]
        public async Task<HttpResponseData> GetEventsAsync(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "events")] HttpRequestData request)
        {
            var queryParameters = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
            var userEmail = queryParameters["user"];


            return await request.CreateResponse(this.businessLogic.GetEventsAsync, userEmail, responseLinks =>
            {
                responseLinks.Links = new Dictionary<string, string> { };
            }, logger);
        }


        /// <summary>
        /// Validate a subscription
        /// </summary>       
        [Function(nameof(GetEventAsync))]
        public async Task<IActionResult> GetEventAsync(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "event")] HttpRequestData request)
        {
            var queryParameters = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
            var eventItem = queryParameters["event"];

            return await this.businessLogic.GetEventAsync(eventItem);
        }

        /// <summary>
        /// Save event
        /// </summary>       
        [Function(nameof(CreateUserAsync))]
        public async Task<HttpResponseData> CreateUserAsync(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = "createuser")] HttpRequestData request)
        {
            return await request.CreateResponse(this.businessLogic.CreateUserAsync, request.DeserializeBody<NewUser>(), responseLinks =>
            {
                responseLinks.Links = new Dictionary<string, string> { };
            }, logger);
        }

        /// <summary>
        /// Save event
        /// </summary>       
        [Function(nameof(ValidateRegistryAsync))]
        public async Task<HttpResponseData> ValidateRegistryAsync(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = "codevalidateregistry")] HttpRequestData request)
        {
            return await request.CreateResponse(this.businessLogic.ValidateRegistryAsync, request.DeserializeBody<ValidateRegistry>(), responseLinks =>
            {
                responseLinks.Links = new Dictionary<string, string> { };
            }, logger);
        }
    }
}


