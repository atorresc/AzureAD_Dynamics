using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Xrm.Sdk.WebServiceClient;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;

namespace DynamicsDaemonClient
{
    class Program
    {
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string ServiceResourceID = ConfigurationManager.AppSettings["ida:serviceResourceID"];
        private static string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string ServiceBaseAddress = ConfigurationManager.AppSettings["ida:serviceBaseAddress"];
        private static string PostLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string AppKey = ConfigurationManager.AppSettings["ida:AppKey"];
        private static string Authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        private static string UserName = ConfigurationManager.AppSettings["ida:user"];
        private static string Password = ConfigurationManager.AppSettings["ida:password"];



        private static string Audience = ConfigurationManager.AppSettings["ida:Audience"];
        private static string ClientIdforUser = ConfigurationManager.AppSettings["ida:ClientIdforUser"];

        private static string ClientIdforCert = ConfigurationManager.AppSettings["ida:ClientIdforCert"];
        private static string certName = ConfigurationManager.AppSettings["ida:CertName"];

        private static AuthenticationParameters ap;


        static void Main(string[] args)
        {



            //Console.WriteLine("Retreive Token Method 1 (Manual Request User Credentials)..");
            ////Console.ReadKey();
            //var Token = PlainBodyAuthenticateWithCredentials();
            //Task.Run(async () => { await PrintContacts(Token); }).GetAwaiter().GetResult();


            //Console.WriteLine("Retreive Token Method 2 (Sdk Request User Credentials)..");
            ////Console.ReadKey();
            //Task.Run(async () => { await PrintContacts("", "Creds"); }).GetAwaiter().GetResult();


            //Console.WriteLine("Retreive Token Method 3 (Manual Request Client Secret)..");
            ////Console.ReadKey();
            //Token = PlainBodyAuthenticateWithSecret();
            //Task.Run(async () => { await PrintContacts(Token); }).GetAwaiter().GetResult();


            //Console.WriteLine("Retreive Token Method 4 (Sdk Request Client Secret)..");
            ////Console.ReadKey();
            //Token = PlainBodyAuthenticateWithSecret();
            //Task.Run(async () => { await PrintContacts("", "Secret"); }).GetAwaiter().GetResult();


            //Console.WriteLine("Retreive Token Method 5 (Certificate)..");
            //Console.ReadKey();
            //Token = PlainBodyAuthenticateWithSecret();
            //Task.Run(async () => { await PrintContacts("", "Cert"); }).GetAwaiter().GetResult();



            Console.WriteLine("Retreive Token Method 5 & CRM SDK..");
            //Console.ReadKey();
            //Token = PlainBodyAuthenticateWithSecret();
            Task.Run(async () => { await PrintContactsFromORganizationService(); }).GetAwaiter().GetResult();

            Console.WriteLine("Finished ..");
            Console.ReadKey();


        }

        private static async Task<AuthenticationResult> GetTokenWithCredentials()
        {
            Authority = "https://login.windows.net/{TenantGUID}/oauth2/authorize";
            //Task.WaitAll(Task.Run(async () => await RetriveURLAutorization(ServiceBaseAddress)));
            //AuthenticationContext authContext =
            //   new AuthenticationContext(ap.Authority, false);
            AuthenticationContext authContext =
              new AuthenticationContext(Authority, false);

            //UserPasswordCredential credentials = new UserPasswordCredential(UserName, Password);
            UserCredential credentials = new UserCredential(UserName, Password);
            AuthenticationResult result = await authContext.AcquireTokenAsync(ServiceResourceID, ClientIdforUser, credentials);
            return result;
            //AuthenticationResult authResult = authContext.AcquireTokenAsync(resource, clientCredentials);
        }





        private static async Task<AuthenticationResult> GetTokenWithSecret()
        {
            AuthenticationContext authContext = new AuthenticationContext(Authority, false);
            ClientCredential clientCredentials = new ClientCredential(ClientId, AppKey);
            AuthenticationResult result = await authContext.AcquireTokenAsync(ServiceResourceID, clientCredentials);
            return result;
        }

        private static async Task<AuthenticationResult> GetTokenWithCertificate()
        {
            ClientAssertionCertificate certCred = null;
            AuthenticationContext authContext = new AuthenticationContext(Authority);
            // Initialize the Certificate Credential to be used by ADAL.
            X509Certificate2 cert = null;

            cert = LoadCertificate();

            // Then create the certificate credential.
            certCred = new ClientAssertionCertificate(ClientIdforCert, cert);
            AuthenticationResult result = await authContext.AcquireTokenAsync(ServiceResourceID, certCred);
            return result;
        }

        private static async Task PrintContacts(string Token = "", string method = "")
        {
            AuthenticationResult authentication;
            if (string.IsNullOrEmpty(Token))
            {
                switch (method)
                {
                    case "Creds":
                        authentication = await GetTokenWithCredentials();
                        break;
                    case "Secret":
                        authentication = await GetTokenWithSecret();
                        break;

                    case "Cert":
                        authentication = await GetTokenWithCertificate();
                        break;
                    default:
                        throw new Exception("Mehtod Not Defined");
                        //break;
                }

                Token = authentication.AccessToken;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ServiceBaseAddress);
                httpClient.Timeout = new TimeSpan(0, 2, 0);
                httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Token);

                HttpResponseMessage retrieveResponse =
                          await httpClient.GetAsync("contacts?$top=10");

                if (retrieveResponse.IsSuccessStatusCode)
                {
                    JObject jRetrieveResponse = JObject.Parse(retrieveResponse.Content.ReadAsStringAsync().Result);

                    var count = 0;
                    foreach (var item in jRetrieveResponse["value"])
                    {
                        count++;
                        Console.WriteLine($"Item Retrieved: {count} - {item["fullname"]}");
                    }
                    Console.WriteLine("Contacts retrieved");

                }
                else
                {
                    var error = retrieveResponse.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"{error}");
                }
            }
        }


        private static async Task PrintContactsFromORganizationService()
        {
            AuthenticationResult authentication = await GetTokenWithCertificate();
            var orgService = new OrganizationWebProxyClient(
               new Uri("https://{DynamicsOrganization}.api.crm.dynamics.com/XRMServices/2011/Organization.svc/web"), false)
            {
                HeaderToken = authentication.AccessToken
            };

            var client = new CrmServiceClient(orgService);
            if (client.IsReady)
            {
                Console.WriteLine("user id:{0}", client.GetMyCrmUserId());
            }

            var Query = new QueryExpression()
            {
                EntityName="contact",
                NoLock = true,
                ColumnSet = new ColumnSet("fullname"),
                TopCount=10
                
            };
            var contacts = client.RetrieveMultiple(Query);

            if (contacts != null && contacts.Entities!= null && contacts.Entities.Any())
            {
                var count = 0;
                foreach (var item in contacts.Entities)
                {
                    count++;
                    Console.WriteLine($"Item Retrieved: {count} - {item["fullname"]}");
                }
            }
        }

        private static string PlainBodyAuthenticateWithCredentials()
        {
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("resource", ServiceResourceID),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", ClientIdforUser),
                new KeyValuePair<string, string>("username", UserName),
                new KeyValuePair<string, string>("password", Password)
                //new KeyValuePair<string, string>("client_secret", cred.ClientSecret),
            };

            using (var client = new HttpClient())
            {
                string baseUrl = $"https://login.windows.net/{tenant}/oauth2/";
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new FormUrlEncodedContent(postData);
                HttpResponseMessage response = client.PostAsync("token", content).Result;
                string jsonString = response.Content.ReadAsStringAsync().Result;
                dynamic responseData = JsonConvert.DeserializeObject(jsonString);
                return responseData.access_token;
            }
        }

        private static string PlainBodyAuthenticateWithSecret()
        {
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("resource", ServiceResourceID),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", AppKey),
            };

            using (var client = new HttpClient())
            {
                string baseUrl = $"https://login.windows.net/{tenant}/oauth2/";
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new FormUrlEncodedContent(postData);
                HttpResponseMessage response = client.PostAsync("token", content).Result;
                string jsonString = response.Content.ReadAsStringAsync().Result;
                dynamic responseData = JsonConvert.DeserializeObject(jsonString);
                return responseData.access_token;
            }
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

        private static X509Certificate2 LoadCertificate(string path, string certpassword)
        {
            var Certpath = @"C:\Users\User\Desktop\TodoListDaemonWIthCert.pfx";
            Certpath = path;
            var cert = new X509Certificate2(Certpath, certpassword);
            //cert.PrivateKey;
            return cert;
        }



        #region Not used

        public static async Task RetriveURLAutorization(string ProtectedResourceURL)
        {
            ap = await AuthenticationParameters.CreateFromResourceUrlAsync(
              new Uri(ProtectedResourceURL));
        }
        #endregion
    }
}
