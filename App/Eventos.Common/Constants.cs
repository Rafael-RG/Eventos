namespace Eventos.Common
{
    /// <summary>
    /// Constants used for in the app
    /// </summary>
    public static class Constants
    {
        public static string DatabaseName = "Eventos.db";

        public static string WebApiKeyHeader => "x-functions-key";

        public static string WebApiKey => (IsReleaseEnvironment) ? "WEB_API_KEY_PROPERTY" : "";

        //public static string WebApiHost => (IsReleaseEnvironment) ? "WEB_API_HOST_PROPERTY" : "http://10.0.2.2:7071/api/"; 
        public static string WebApiHost => (IsReleaseEnvironment) ? "WEB_API_HOST_PROPERTY" : "https://rememberpro.azurewebsites.net/api/";

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

        public static string WebFileUri => "https://ambitious-grass-0f2288a0f.5.azurestaticapps.net/?event={0}";

        public static string GetEventsByUserUri => WebApiHost + "events?user={0}&code=R4oyjUREZfYaISSUIEvLVtbbJblNCLtXnkriVd9oV8llAzFuqAjLmw==";

        public static string SaveEvent => WebApiHost + "saveevent?code=R4oyjUREZfYaISSUIEvLVtbbJblNCLtXnkriVd9oV8llAzFuqAjLmw==";

        public static string CreateUser => WebApiHost + "createuser?code=R4oyjUREZfYaISSUIEvLVtbbJblNCLtXnkriVd9oV8llAzFuqAjLmw==";

        public static string ValidateRegistry => WebApiHost + "codevalidateregistry?code=R4oyjUREZfYaISSUIEvLVtbbJblNCLtXnkriVd9oV8llAzFuqAjLmw==";
    
        public static string FindUserAndSendCode => WebApiHost + "finduserandsendcode?code=R4oyjUREZfYaISSUIEvLVtbbJblNCLtXnkriVd9oV8llAzFuqAjLmw==";

        public static string ValidateCode => WebApiHost + "validatecode?code=R4oyjUREZfYaISSUIEvLVtbbJblNCLtXnkriVd9oV8llAzFuqAjLmw==";

        public static string RecoverPassword => WebApiHost + "recoverpassword?code=R4oyjUREZfYaISSUIEvLVtbbJblNCLtXnkriVd9oV8llAzFuqAjLmw==";
    
        public static string Login => WebApiHost + "login?code=R4oyjUREZfYaISSUIEvLVtbbJblNCLtXnkriVd9oV8llAzFuqAjLmw==";
    }
}
