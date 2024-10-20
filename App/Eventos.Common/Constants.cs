﻿namespace Eventos.Common
{
    /// <summary>
    /// Constants used for in the app
    /// </summary>
    public static class Constants
    {
        public static string DatabaseName = "Eventos.db";

        public static string WebApiKeyHeader => "x-functions-key";

        public static string WebApiKey = "MhxA7YBAqb9eb5yAW83gzoRu6cXyo7BYu5Qo8T8IbfuMAzFuIeHHGg==";

        //public static string WebApiHost => (IsReleaseEnvironment) ? "WEB_API_HOST_PROPERTY" : "http://10.0.2.2:7071/api/"; 
        public static string WebApiHost = "https://recuerdame.azurewebsites.net/api/";

        public static string EnviromentName => (IsReleaseEnvironment) ? "ENVIRONMENT_NAME_PROPERTY" : "";
       
        public static string AppCenterDroid => (IsReleaseEnvironment) ? "APP_CENTER_DROID_PROPERTY" : "";

        public static string AppCenteriOS => (IsReleaseEnvironment) ? "APP_CENTER_IOS_PROPERTY" : "";

        public static bool IsReleaseEnvironment
        {
            get
            {
#if !DEBUG
                return true;
#else
                return false;
#endif
            }
        }
        

        public static string WebFileUri => "https://recuerdame.app/visit/?event={0}";

        public static string GetEventsByUserUri => WebApiHost + "events?user={0}&code=" + WebApiKey;

        public static string SaveEvent => WebApiHost + "saveevent?code=" + WebApiKey;

        public static string CreateUser => WebApiHost + "createuser?code=" + WebApiKey;

        public static string ValidateRegistry => WebApiHost + "codevalidateregistry?code=" + WebApiKey;
    
        public static string FindUserAndSendCode => WebApiHost + "finduserandsendcode?code=" + WebApiKey;

        public static string ValidateCode => WebApiHost + "validatecode?code=" + WebApiKey;

        public static string RecoverPassword => WebApiHost + "recoverpassword?code=" + WebApiKey;
    
        public static string Login => WebApiHost + "login?code=" + WebApiKey;

        public static string ValidateSuscription => WebApiHost + "validatesubscription?code=" + WebApiKey;
        
        public static string GetUser => WebApiHost + "user?code=" + WebApiKey;
        
        public static string ChangeUserData => WebApiHost + "changeuserdata?code=" + WebApiKey;
    }
}
