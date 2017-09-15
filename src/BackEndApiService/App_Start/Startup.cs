using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.ActiveDirectory;
using System.Configuration;

[assembly: OwinStartup(typeof(BackEndApiService.App_Start.Startup))]
namespace BackEndApiService.App_Start
{


    //https://demoappsad.onmicrosoft.com/BackEndApiService

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var azureADBearerAuthOptions = new WindowsAzureActiveDirectoryBearerAuthenticationOptions
            {
                Tenant = ConfigurationManager.AppSettings["ida:Tenant"]
            };

            azureADBearerAuthOptions.TokenValidationParameters =
                new System.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidAudience = ConfigurationManager.AppSettings["ida:Audience"]
                };

            app.UseWindowsAzureActiveDirectoryBearerAuthentication(azureADBearerAuthOptions);
        }
    }
}