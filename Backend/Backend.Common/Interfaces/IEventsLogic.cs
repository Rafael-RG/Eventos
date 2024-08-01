﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Backend.Common.Models;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Common.Interfaces
{
    /// <summary>
    /// Documents business logic
    /// </summary>
    public interface IEventsLogic
    {
         /// <summary>
        /// Gets all the documents
        /// </summary>
        /// <returns></returns>
        Task<Result<bool>> ValidateSubscriptionAsync(ValidateSubscriptionRequest validateSubscriptionRequest);

        /// <summary>
        /// Gets all the documents
        /// </summary>
        /// <returns></returns>
        Task<Result<(bool, string)>> SaveEventAsync(EventRequest newEvent);

        /// <summary>
        /// Gets all the documents
        /// </summary>
        /// <returns></returns>
        Task<Result<EventResponse>> GetEventsAsync(string email);

        /// <summary>
        /// Get event
        /// </summary>
        /// <returns></returns>
        Task<FileContentResult> GetEventAsync(string rowKey);
    }


}
