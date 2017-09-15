using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FrontEndWebApptoApi.Controllers
{
    [Authorize]
    public class ValuesController : Controller
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string audienceId = ConfigurationManager.AppSettings["ida:Audience"];
        private static string appKey = ConfigurationManager.AppSettings["ida:appKey"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:aadInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:tenant"];

        private static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        private static AuthenticationContext authContext = new AuthenticationContext(authority);
        private static ClientCredential clientCredential = new ClientCredential(audienceId, appKey);

        // appID of the web api
        private static string serviceResourceID = ConfigurationManager.AppSettings["ida:serviceResourceID"];
        private static string serviceBaseAddress =ConfigurationManager.AppSettings["ida:serviceBaseAddress"];  // base url of the web api


        // GET: Values
        public async Task<ActionResult> Index()
        {
            AuthenticationResult result = null;
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            AuthenticationContext authContext = new AuthenticationContext(authority, new WebSessionCache(userObjectID));


            ClientCredential credential = new ClientCredential(clientId, appKey);
            result = await authContext.AcquireTokenSilentAsync(serviceResourceID, credential,
                new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, serviceBaseAddress + "api/values/1");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string r = await response.Content.ReadAsStringAsync();
                ViewBag.Results = r;
                return View("Index");
            }
            else
            {
                string r = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    authContext.TokenCache.Clear();
                }
                ViewBag.ErrorMessage = "AuthorizationRequired";
                ViewBag.ErrorDescription = r;
                return View("Index");
            }

            
        }
    }
}