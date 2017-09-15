using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

[assembly: OwinStartup(typeof(FrontAppWeb.App_Start.Startup))]
namespace FrontAppWeb.App_Start
{
    public class Startup
    {
        private string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private string postLogoutRedirectUri= ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        public void Configuration(IAppBuilder app)
        {
            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);


            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/Error/message=" + context.Exception.Message);
                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}