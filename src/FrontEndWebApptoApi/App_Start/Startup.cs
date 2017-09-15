using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.Owin;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Cookies;
using System.Configuration;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

using Microsoft.Owin.Security.Notifications;
using Owin;

[assembly: OwinStartup(typeof(FrontEndWebApptoApi.App_Start.Startup))]
namespace FrontEndWebApptoApi.App_Start
{
    public class Startup
    {
        private string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private string tenant = ConfigurationManager.AppSettings["ida:Tenant"];

        private string ServiceResourceID = ConfigurationManager.AppSettings["ida:serviceResourceID"];
        private string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private string ServiceBaseAddress = ConfigurationManager.AppSettings["ida:serviceBaseAddress"];
        private string PostLogoutRedirectUri= ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private string AppKey = ConfigurationManager.AppSettings["ida:AppKey"];

        public void Configuration(IAppBuilder app)
        {
            string Authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions {
                    ClientId = ClientId,
                    Authority = Authority,
                    PostLogoutRedirectUri = PostLogoutRedirectUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthorizationCodeReceived = async (context) =>
                        {
                            var code = context.Code;
                            ClientCredential credential = new ClientCredential(ClientId, AppKey);
                            string userObjectID =
                                context.AuthenticationTicket.Identity.FindFirst(
                                    "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                            AuthenticationContext authContext = new AuthenticationContext(Authority, new WebSessionCache(userObjectID));
                            AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(code,
                                new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, ServiceResourceID);
                            //return;//  Task.FromResult(0);
                        },
                        AuthenticationFailed = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/Home/Error");
                            return Task.FromResult(0);
                        }
                    }
                    });
        }

    }
}