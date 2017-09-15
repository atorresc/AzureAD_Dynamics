using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DaemonClientWithCert
{
    class Program
    {
        private static string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        //private static string AppKey = ConfigurationManager.AppSettings["ida:AppKey"];

        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];

        private static string Authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        private static AuthenticationContext authContext = new AuthenticationContext(Authority);
        //private static ClientCredential clientCredential = new ClientCredential(ClientId, AppKey);

        private static string ServiceResourceID = ConfigurationManager.AppSettings["ida:serviceResourceID"];
        private static ClientAssertionCertificate certCred = null;
        private static string certName = ConfigurationManager.AppSettings["ida:CertName"];

        static  void Main(string[] args)
        {
            Console.WriteLine("Press enter to start..");
            Console.Read();
            //AuthenticationResult result = null;

            // Create the authentication context to be used to acquire tokens.
            authContext = new AuthenticationContext(Authority);

            // Initialize the Certificate Credential to be used by ADAL.
            X509Certificate2 cert = null;

            cert = LoadCertificate();

            // Then create the certificate credential.
            certCred = new ClientAssertionCertificate(ClientId, cert);

           

            //result = authContext.AcquireTokenAsync(ServiceResourceID, clientCredential).Result;
            Console.WriteLine("Autenticacion satisfactoria");
            MakeHttpsCall().Wait();
            Console.WriteLine("Fin de ejecucion");
            Console.Read();
            Console.Read();
        }

        private static X509Certificate2 LoadCertificate()
        {
            X509Certificate2 cert = null;
            X509Store store = new X509Store(StoreLocation.CurrentUser);

            try
            {
                store.Open(OpenFlags.ReadOnly);
                // Place all certificates in an X509Certificate2Collection object.
                X509Certificate2Collection certCollection = store.Certificates;
                // Find unexpired certificates.
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                //certName = "50CD02980A6DDE299D14019AE08B9CAAF0AB22BE";
                // From the collection of unexpired certificates, find the ones with the correct name.
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, false);
                if (signingCert.Count == 0)
                {
                    // No matching certificate found.
                    throw new Exception("CErtificate Not Found");
                }
                // Return the first certificate in the collection, has the right name and is current.
                cert = signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).First();
            }
            finally
            {
                store.Close();
            }
            return cert;
        }

        private static async Task MakeHttpsCall()
        {
            AuthenticationResult result;
            result = await authContext.AcquireTokenAsync(ServiceResourceID, certCred);
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
