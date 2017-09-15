using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DaemonClientApp
{
    class Program
    {
        private static string ClientId= ConfigurationManager.AppSettings["ida:ClientId"];
        private static string AppKey = ConfigurationManager.AppSettings["ida:AppKey"];

        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static  string tenant = ConfigurationManager.AppSettings["ida:Tenant"];

        private static string Authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        private static AuthenticationContext authContext = new AuthenticationContext(Authority);
        private static ClientCredential clientCredential = new ClientCredential(ClientId, AppKey);

        private static string ServiceResourceID = ConfigurationManager.AppSettings["ida:serviceResourceID"];
        static void Main(string[] args)
        {
            Console.WriteLine("Press enter to start..");
            Console.Read();
            AuthenticationResult result = null;

            result = authContext.AcquireTokenAsync(ServiceResourceID, clientCredential).Result;
            Console.WriteLine("Autenticacion satisfactoria");
            MakeHttpsCall(result).Wait();
            Console.WriteLine("Fin de ejecucion");
            Console.Read();
            Console.Read();
        }

        private static async Task MakeHttpsCall(AuthenticationResult result)
        {
            string serviceBaseAddress = "http://localhost:57042/";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            HttpResponseMessage response = await httpClient.GetAsync(serviceBaseAddress + "api/values/55");

            if (response.IsSuccessStatusCode)
            {
                string r = await response.Content.ReadAsStringAsync();
                Console.WriteLine(r);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    authContext.TokenCache.Clear();
                }
                Console.WriteLine("Access Denied!");
            }
        }


    }
}
