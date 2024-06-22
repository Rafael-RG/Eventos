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



    }
}


