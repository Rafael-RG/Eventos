using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Backend.Common.Models;
using Backend.Common.Interfaces;
using Backend.Models;
using Backend.Common.Logic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Hosting;


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
        public EventsLogic(ISessionProvider sessionProvider, IDataAccess dataAccess, ILogger<EventsLogic> logger, IHttpService httpService) : base(sessionProvider, dataAccess, logger, httpService)
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


        /// </inheritdoc/>
        public async Task<Result<(bool,string)>> SaveEventAsync(EventRequest newEvent)
        {
            var response = new Result<(bool, string)>();

            try
            {
                var eventEntry = new EventEntry
                {
                    Title = newEvent.Title,
                    Description = newEvent.Description,
                    Date = newEvent.Date,
                    Email = newEvent.Email,
                    EndTime = newEvent.EndTime,
                    Count = newEvent.Count,
                    PartitionKey = newEvent.Email,
                    RowKey = Guid.NewGuid().ToString(),
                    StartTime = newEvent.StartTime,
                    Zone = newEvent.Zone,
                    ZoneId = newEvent.ZoneId,
                    EventURl = newEvent.EventURl
                };

                var result = await this.dataAccess.SaveEntryAsync(eventEntry);


                if (result)
                {
                    response.Data = (true, eventEntry.RowKey);
                    response.Success = true;
                }
                else 
                {
                    response.Data = (false, "Ocurrió un error al crear el recordatorio");
                    response.Success = false;
                }

            }
            catch
            {
                response.Data = (false, "Ocurrió un error al crear el recordatorio");
                response.Success = false;
            }

            return response;
        }


        /// </inheritdoc/>
        public async Task<Result<EventResponse>> GetEventsAsync(string email)
        {
            try
            {
                var eventEntryList = await this.dataAccess.GetEventsAsync(email);

                var result = new Result<EventResponse>(new EventResponse
                {
                    Events = eventEntryList.Select(x => new Event
                    {
                        Title = x.Title,
                        Description = x.Description,
                        Date = x.Date,
                        Email = x.Email,
                        RowKey = x.RowKey,
                        EndTime = x.EndTime,
                        StartTime = x.StartTime,
                        Zone = x.Zone,
                        ZoneId = x.ZoneId,
                        Count= x.Count,
                        EventURl = x.EventURl
                    }).ToList()
                });

                return result;
            }
            catch
            {
                return new Result<EventResponse>();
            }

        }

        /// </inheritdoc/>
        public async Task<FileContentResult> GetEventAsync(string rowKey)
        {
            try
            {
                var eventEntry = await this.dataAccess.GetEventAsync(rowKey);

                eventEntry.Count= eventEntry.Count + 1;

                var result = await this.dataAccess.SaveEntryAsync(eventEntry);

                var eventItem = new Event
                {
                    Title = eventEntry.Title,
                    Description = eventEntry.Description,
                    Date = eventEntry.Date,
                    Email = eventEntry.Email,
                    RowKey = eventEntry.RowKey,
                    EndTime = eventEntry.EndTime,
                    StartTime = eventEntry.StartTime,
                    Zone = eventEntry.Zone,
                    ZoneId = eventEntry.ZoneId,
                    Count = eventEntry.Count,
                    EventURl = eventEntry.EventURl
                };

                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(eventItem.ZoneId);

                var icsContent = GenerateICSContent(eventItem.Title, eventItem.Description, eventItem.StartTime, eventItem.EndTime, timeZoneInfo, eventEntry.EventURl);

                var byteArray = Encoding.UTF8.GetBytes(icsContent);

                return new FileContentResult(byteArray, "text/calendar")
                {
                    FileDownloadName = "event.ics",
                };
            }
            catch
            {
                return null;
            }

        }

        private string GenerateICSContent(string title, string description, DateTimeOffset startDateTime, DateTimeOffset endDateTime, TimeZoneInfo timeZoneInfo, string url)
        {
            StringBuilder sb = new StringBuilder();
            string timeZoneId = timeZoneInfo.Id;
            string timeZoneStart = startDateTime.ToString("yyyyMMddTHHmmss");
            string timeZoneEnd = endDateTime.ToString("yyyyMMddTHHmmss");
            string uid = Guid.NewGuid().ToString();
            string dtStamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//YourCompany//YourProduct//EN");
            sb.AppendLine("CALSCALE:GREGORIAN");

            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine($"TZID:{timeZoneId}");
            sb.AppendLine("BEGIN:STANDARD");
            sb.AppendLine($"DTSTART:{timeZoneStart}");
            sb.AppendLine("TZOFFSETFROM:+0000"); // Ajusta según tu zona horaria
            sb.AppendLine("TZOFFSETTO:+0000"); // Ajusta según tu zona horaria
            sb.AppendLine("END:STANDARD");
            sb.AppendLine("END:VTIMEZONE");

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{uid}");
            sb.AppendLine($"DTSTAMP:{dtStamp}");
            sb.AppendLine($"SUMMARY:{title.Replace(",", "\\,").Replace(";", "\\;")}");
            sb.AppendLine($"DESCRIPTION:{description.Replace(",", "\\,").Replace(";", "\\;")}");
            sb.AppendLine($"DTSTART;TZID={timeZoneId}:{timeZoneStart}");
            sb.AppendLine($"DTEND;TZID={timeZoneId}:{timeZoneEnd}");
            if (!string.IsNullOrEmpty(url))
            {
                sb.AppendLine($"URL:{url}");
            }

            // Añadir alerta (recordatorio)
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine("TRIGGER:-PT15M");
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine($"DESCRIPTION:Reminder for {title}");
            sb.AppendLine("END:VALARM");

            sb.AppendLine("END:VEVENT");

            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        public async Task<Result<(bool, string)>> CreateUserAsync(NewUser newUser)
        {
            var response = new Result<(bool, string)>();

            try
            {
                var user = await this.dataAccess.GetUserAsync(newUser.Email);

                if (user.PartitionKey != null && user.IsActive) 
                {
                    response.Data = (false, "El usuario ya existe");
                    response.Success = false;
                }
                else if (user.PartitionKey != null && !user.IsActive)
                {
                    await SendEmail(user.LastCodeActivation, newUser.Email, newUser.FullName);
                    response.Data = (true, $"Correo reenviado: {newUser.Email}");
                    response.Success = true;
                }
                else
                {
                    var code = new Random().Next(1000, 9999).ToString();

                    var userEntry = new UserEntry
                    {
                        FullName = newUser.FullName,
                        Email = newUser.Email,
                        Password = newUser.Password,
                        Country = newUser.Country,
                        PartitionKey = newUser.Email,
                        RowKey = Guid.NewGuid().ToString(),
                        IsActive = false,
                        LastCodeActivation = code
                    };

                    var result = await this.dataAccess.SaveNewUserAsync(userEntry);

                    if (result)
                    {
                        await SendEmail(code, newUser.Email, newUser.FullName);

                        response.Data = (true, "Creado y enviado");
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = (false, "Ocurrió un error al crear el usuario");
                        response.Success = false;
                    }
                }

            }
            catch
            {
                response.Data = (false, "Ocurrió un error al crear el usuario");
                response.Success = false;
            }

            return response;
        }

        public async Task<Result<bool>> ValidateRegistryAsync(ValidateRegistry validateUserRequest)
        {
            var response = new Result<bool>();

            try
            {
                var user = await this.dataAccess.GetUserAsync(validateUserRequest.Email);

                if (user.PartitionKey != null && user.LastCodeActivation == validateUserRequest.Code)
                {
                    user.IsActive = true;
                    user.LastCodeActivation = string.Empty;

                    var result = await this.dataAccess.SaveNewUserAsync(user);

                    if (result)
                    {
                        response.Data = true;
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = false;
                        response.Success = false;
                    }
                }
                else
                {
                    response.Data = false;
                    response.Success = false;
                }

            }
            catch
            {
                response.Data = false;
                response.Success = false;
            }

            return response;
        }

        private async Task SendEmail(string code, string email, string name)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient();
                NetworkCredential smtpCredentials = new NetworkCredential("soporte@recuerdame.app", "R@F@el94006859R@");

                MailMessage message = new MailMessage();
                MailAddress fromAddress = new MailAddress("soporte@recuerdame.app");
                MailAddress toAddress = new MailAddress(email);

                smtpClient.Host = "host.globalpingames.com";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = smtpCredentials;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Timeout = 60000;

                message.From = fromAddress;
                message.To.Add(toAddress);
                message.IsBodyHtml = true;
                message.Subject = "¡Bienvenido a Recuérdame!";
                message.Body = $"<html>\r\n  <head></head>\r\n  <body>\r\n    <p>¡Hola {name}!</p>\r\n    <p>Nos alegra mucho que te hayas unido a <strong>Recuérdame</strong>.</p>\r\n    <p>Para que puedas empezar a disfrutar de todos nuestros servicios, por favor ingresa el siguiente código de verificación en la app:</p>\r\n    <h2 style=\"color: #ABBA3C;\">{code}</h2>\r\n    <p>Si tienes alguna duda o necesitas ayuda, no dudes en contactarnos.</p>\r\n    <p>¡Bienvenido a la comunidad de <strong>Recuérdame</strong>!</p>\r\n    <p>Saludos,</p>\r\n    <p>El equipo de Recuérdame</p>\r\n  </body>\r\n</html>\r\n";

                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                // Otro error
            }
        }

    }

}
