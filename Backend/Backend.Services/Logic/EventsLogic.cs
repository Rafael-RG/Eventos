using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Backend.Common.Models;
using Backend.Common.Interfaces;
using Backend.Models;
using Backend.Common.Logic;
using System.Linq;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Backend.Service.BusinessLogic
{
    /// </inheritdoc/>
    public class EventsLogic : BaseLogic, IEventsLogic
    {
        private int SubscriptionProductId { get; set; }
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }

        /// <summary>
        /// Gets by DI the dependeciees
        /// </summary>
        /// <param name="dataAccess"></param>
        public EventsLogic(ISessionProvider sessionProvider, IDataAccess dataAccess, ILogger<EventsLogic> logger) : base(sessionProvider, dataAccess, logger)
        {
            this.SubscriptionProductId = Convert.ToInt32(Environment.GetEnvironmentVariable("HotmartSubscriptionProductId"));
            this.ClientId = Environment.GetEnvironmentVariable("HotmartClientId");
            this.ClientSecret = Environment.GetEnvironmentVariable("HotmartClientSecret");
        }


        /// </inheritdoc/>
        public async Task<Result<bool>> ValidateSubscriptionAsync(ValidateSubscriptionRequest validateSubscriptionRequest)
        {
            try
            {
                var accessToken = await this.GetAccessTokenAsync();

                var userEmail = validateSubscriptionRequest.UserEmail;
                bool isActive = false;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(userEmail))
                {
                    return new Result<bool>(isActive);
                }

                using (var client = new HttpClient())
                {
                    string hotmartApiUrl = $"https://developers.hotmart.com/payments/api/v1/subscriptions?subscriber_email={userEmail}";

                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    HttpResponseMessage response = await client.GetAsync(hotmartApiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var subscriptionData = JsonConvert.DeserializeObject<SubscriptionsResponse>(responseBody);

                        foreach (var subscription in subscriptionData.Items)
                        {
                            var productId = subscription.Product.Id;
                            string status = subscription.Status;

                            if (productId == this.SubscriptionProductId && status == "ACTIVE")
                            {
                                isActive = true;
                            }
                        }

                        return new Result<bool>(isActive);
                    }
                    else
                    {
                        return new Result<bool>(isActive);
                    }
                }

            }
            catch (Exception ex)
            {
                return new Result<bool>(false);
            }
        }

        /// </inheritdoc/>
        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                string basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{this.ClientId}:{this.ClientSecret}"));

                var request = new HttpRequestMessage(HttpMethod.Post, $"https://api-sec-vlc.hotmart.com/security/oauth/token?grant_type=client_credentials&client_id={this.ClientId}&client_secret={this.ClientSecret}")
                {
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);

                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var tokenResponse = JObject.Parse(responseBody);
                        return tokenResponse["access_token"].ToString();
                    }
                    else
                    {
                        return string.Empty;
                    }

                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }


    }


}
