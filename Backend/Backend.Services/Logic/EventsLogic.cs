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
using System.Net.Mail;
using System.Net;


namespace Backend.Service.BusinessLogic
{
    /// </inheritdoc/>
    public class EventsLogic : BaseLogic, IEventsLogic
    {
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string SoportUserEmail { get; set; }
        private string SoportPasswordEmail { get; set; }
        private string SoportHostEmail { get; set; }

        /// <summary>
        /// Gets by DI the dependeciees
        /// </summary>
        /// <param name="dataAccess"></param>
        public EventsLogic(ISessionProvider sessionProvider, IDataAccess dataAccess, ILogger<EventsLogic> logger, IHttpService httpService) : base(sessionProvider, dataAccess, logger, httpService)
        {
            this.ClientId = Environment.GetEnvironmentVariable("HotmartClientId");
            this.ClientSecret = Environment.GetEnvironmentVariable("HotmartClientSecret");
            this.SoportUserEmail = Environment.GetEnvironmentVariable("SoportUserEmail");
            this.SoportPasswordEmail = Environment.GetEnvironmentVariable("SoportPasswordEmail");
            this.SoportHostEmail = Environment.GetEnvironmentVariable("SoportHostEmail");
        }


        /// </inheritdoc/>
        public async Task<Result<SuscriberUserInfo>> ValidateSubscriptionAsync(ValidateSubscriptionRequest validateSubscriptionRequest)
        {
            var res = new Result<SuscriberUserInfo>();

            try
            {
                var accessToken = await this.GetAccessTokenAsync();

                var userEmail = validateSubscriptionRequest.UserEmail;

                if (string.IsNullOrEmpty(accessToken))
                {
                    res.Success = false;
                    res.Data = null;
                    res.Message = "No se pudo obtener el token de acceso";
                    return res;
                }

                if (string.IsNullOrEmpty(userEmail))
                {
                    res.Success = false;
                    res.Data = null;
                    res.Message = "Error al validar suscripcion";
                    return res;
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

                        var plans = await this.dataAccess.GetPlansAsync();

                        var suscriberUserInfo = new SuscriberUserInfo();
                        suscriberUserInfo.IsSubscribed = false;
                        suscriberUserInfo.ClickCount = 0;

                        foreach (var subscription in subscriptionData.Items)
                        {

                            if (plans.Any(p=>p.RowKey == subscription.Plan.Id.ToString()) 
                                && subscription.Status == "ACTIVE")
                            {
                                var user = await this.dataAccess.GetUserAsync(userEmail);
                                if (user.LastPeriod != subscription.Date_Next_Charge) 

                                {
                                    user.LastPeriod = subscription.Date_Next_Charge;
                                    user.TotalClicksCurrentPeriod = 0;
                                    await this.dataAccess.SaveNewUserAsync(user);
                                }
                                
                                var plan = plans.FirstOrDefault(p => p.RowKey == subscription.Plan.Id.ToString());

                                suscriberUserInfo.IsSubscribed = true;
                                suscriberUserInfo.Email = userEmail;
                                suscriberUserInfo.ClickCount = plan.Clicks;
                                suscriberUserInfo.Plan = subscription.Plan.Name;
                                suscriberUserInfo.PlanFinishDate = DateTimeOffset.FromUnixTimeSeconds(subscription.Date_Next_Charge/1000);
                            }
                        }

                        res.Success = true;
                        res.Data = suscriberUserInfo;
                        return res;
                    }
                    else
                    {
                        res.Success = false;
                        res.Data = null;
                        res.Message = "No se pudo obtener la información de la suscripción";
                        return res;
                    }
                }

            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Data = null;
                res.Message = ex.Message;
                return res;
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
                var eventEntry = new EventEntry();

                if (string.IsNullOrEmpty(newEvent?.RowKey)) 
                {
                    eventEntry.Title = newEvent.Title;
                    eventEntry.Description = newEvent.Description;
                    eventEntry.Date = newEvent.Date;
                    eventEntry.Email = newEvent.Email;
                    eventEntry.EndTime = newEvent.EndTime;
                    eventEntry.Count = newEvent.Count;
                    eventEntry.PartitionKey = newEvent.Email;
                    eventEntry.StartTime = newEvent.StartTime;
                    eventEntry.Zone = newEvent.Zone;
                    eventEntry.ZoneId = newEvent.ZoneId;
                    eventEntry.EventURl = newEvent.EventURl;
                    eventEntry.IsDeleted = newEvent.IsDelete;
                    eventEntry.RowKey = Guid.NewGuid().ToString();
                }
                else
                {
                    var getEvent = await this.dataAccess.GetEventAsync(newEvent.RowKey);
                    
                    eventEntry = getEvent;

                    if (newEvent.IsDelete)
                    {
                        eventEntry.IsDeleted = true;
                    }
                    else 
                    {
                        eventEntry.Title = newEvent.Title;
                        eventEntry.Description = newEvent.Description;
                        eventEntry.Date = newEvent.Date;
                        eventEntry.StartTime = newEvent.StartTime;
                        eventEntry.EndTime = newEvent.EndTime;
                        eventEntry.Zone = newEvent.Zone;
                        eventEntry.ZoneId = newEvent.ZoneId;
                        eventEntry.EventURl = newEvent.EventURl;
                    }
                }

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
                        EventURl = x.EventURl,
                        IsDelete = x.IsDeleted
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

                var suscription = await this.ValidateSubscriptionAsync(new ValidateSubscriptionRequest { UserEmail = eventEntry.Email });

                if (!suscription.Data.IsSubscribed)
                {
                    return null;
                }

                var user = await this.dataAccess.GetUserAsync(eventEntry.Email);

                if (user.TotalClicksCurrentPeriod >= suscription.Data.ClickCount)
                {
                    return null;
                }

                user.TotalClicks = user.TotalClicks + 1;
                user.TotalClicksCurrentPeriod = user.TotalClicksCurrentPeriod + 1;
                await this.dataAccess.SaveNewUserAsync(user);

                eventEntry.Count = eventEntry.Count + 1;
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

                var fileName = $"{eventItem.Title}.ics";

                return new FileContentResult(byteArray, "text/calendar")
                {
                    FileDownloadName = fileName,
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

        public async Task<Result<bool>> FindUserAndSendCodeAsync(UserEmail userEmail)
        {
            var response = new Result<bool>();

            try
            {
                var user = await this.dataAccess.GetUserAsync(userEmail.Email);

                if (user.PartitionKey != null)
                {
                    var code = new Random().Next(1000, 9999).ToString();

                    user.LastCodeActivation = code;

                    var result = await this.dataAccess.SaveNewUserAsync(user);

                    if (result)
                    {
                        await SendEmail(code, user.Email, user.FullName, 1);

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

        public async Task<Result<bool>> ValidateCodeAsync(ValidateRegistry validateUserRequest)
        {
            var response = new Result<bool>();

            try
            {
                var user = await this.dataAccess.GetUserAsync(validateUserRequest.Email);

                if (user.PartitionKey != null && user.LastCodeActivation == validateUserRequest.Code)
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
            catch
            {
                response.Data = false;
                response.Success = false;
            }

            return response;
        }

        public async Task<Result<bool>> RecoveryPasswordAsync(RecoveryPassword recoveryPassword)
        {
            var response = new Result<bool>();

            try
            {
                var user = await this.dataAccess.GetUserAsync(recoveryPassword.Email);

                if (user.PartitionKey != null)
                {

                    user.Password = recoveryPassword.Password;

                    var result = await this.dataAccess.SaveNewUserAsync(user);

                    if (result)
                    {
                        await SendEmail("", recoveryPassword.Email, user.FullName, 2);

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

        public async Task<Result<UserEntry>> LoginAsync(RecoveryPassword credentials)
        {
            var response = new Result<UserEntry>();

            try
            {
                var user = await this.dataAccess.GetUserAsync(credentials.Email);

                if (user.PartitionKey != null && user.Password == credentials.Password)
                {
                    response.Data = user;
                    response.Success = true;
                }
                else
                {
                    response.Message = "Usuario o contraseña incorrectos";
                    response.Success = false;
                }

            }
            catch
            {
                response.Message = "Usuario o contraseña incorrectos";
                response.Success = false;
            }

            return response;
        }
        
        public async Task<Result<UserEntry>> GetUserAsync(ValidateSubscriptionRequest data)
        {
            var response = new Result<UserEntry>();

            try
            {
                var user = await this.dataAccess.GetUserAsync(data.UserEmail);

                if (user.PartitionKey != null)
                {
                    response.Data = user;
                    response.Success = true;
                }
                else
                {
                    response.Message = "Usuario no encontrado";
                    response.Success = false;
                }

            }
            catch
            {
                response.Message = "Usuario no encontrado";
                response.Success = false;
            }

            return response;
        }

        private async Task SendEmail(string code, string email, string name, int recovery = 0)
        {
            try
            {
                var body = $"<html>\r\n  <head></head>\r\n  <body>\r\n    <p>¡Hola {name}!</p>\r\n    <p>Nos alegra mucho que te hayas unido a <strong>Recuérdame</strong>.</p>\r\n    <p>Para que puedas empezar a disfrutar de todos nuestros servicios, por favor ingresa el siguiente código de verificación en la app:</p>\r\n    <h2 style=\"color: #ABBA3C;\">{code}</h2>\r\n    <p>Si tienes alguna duda o necesitas ayuda, no dudes en contactarnos.</p>\r\n    <p>¡Bienvenido a la comunidad de <strong>Recuérdame</strong>!</p>\r\n    <p>Saludos,</p>\r\n    <p>El equipo de Recuérdame</p>\r\n  </body>\r\n</html>\r\n";
                var subject = "¡Bienvenido a Recuérdame!";

                if (recovery == 1)
                {
                    body = $"<html>\r\n  <head></head>\r\n  <body>\r\n    <p>¡Hola {name}!</p>\r\n    <p>Recibimos una solicitud para recuperar tu contraseña en <strong>Recuérdame</strong>.</p>\r\n    <p>Para que puedas recuperar tu contraseña, por favor ingresa el siguiente código en la app:</p>\r\n    <h2 style=\"color: #ABBA3C;\">{code}</h2>\r\n    <p>Si no solicitaste recuperar tu contraseña, por favor ignora este mensaje.</p>\r\n    <p>Si tienes alguna duda o necesitas ayuda, no dudes en contactarnos.</p>\r\n    <p>Saludos,</p>\r\n    <p>El equipo de Recuérdame</p>\r\n  </body>\r\n</html>\r\n";
                    subject = "Recuperación de contraseña";
                }
                else if (recovery == 2)
                {
                    body = $"<html>\r\n  <head></head>\r\n  <body>\r\n    <p>¡Hola {name}!</p>\r\n   <p>Te informamos que tu contraseña ha sido actualizada satisfactoriamente.</p>\r\n   <p>Si tienes alguna duda o necesitas ayuda, no dudes en contactarnos.</p>\r\n      <p>Saludos,</p>\r\n    <p>El equipo de Recuérdame</p>\r\n  </body>\r\n</html>\r\n";
                    subject = "Se cambió tu contraseña";
                }

                SmtpClient smtpClient = new SmtpClient();
                NetworkCredential smtpCredentials = new NetworkCredential(this.SoportUserEmail, this.SoportPasswordEmail);

                MailMessage message = new MailMessage();
                MailAddress fromAddress = new MailAddress(this.SoportUserEmail);
                MailAddress toAddress = new MailAddress(email);

                smtpClient.Host = this.SoportHostEmail;
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = smtpCredentials;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Timeout = 60000;

                message.From = fromAddress;
                message.To.Add(toAddress);
                message.IsBodyHtml = true;
                message.Subject = subject;
                message.Body = body;

                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                // Otro error
            }
        }

    }

}
