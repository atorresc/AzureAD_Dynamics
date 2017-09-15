using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private AuthenticationContext authContext = null;
        private AuthenticationResult result = null;
        private HttpClient httpClient = new HttpClient();
        Uri redirectUri = new Uri("http://anything");

        private string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private string tenant = ConfigurationManager.AppSettings["ida:Tenant"];

        private string ServiceResourceID = ConfigurationManager.AppSettings["ida:serviceResourceID"];
        private string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private string ServiceBaseAddress = ConfigurationManager.AppSettings["ida:serviceBaseAddress"];


        public MainWindow()
        {
            InitializeComponent();
            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            authContext = new AuthenticationContext(authority, new FileCache());
        }

        private async void signInButton_Click(object sender, RoutedEventArgs e)
        {
            if (signInButton.Content.ToString() == "Sign out")
            {
                authContext.TokenCache.Clear();
                ClearCookies();
                signInButton.Content = "Sign In";
                return;
            }
            try
            {
                result = await authContext.AcquireTokenAsync(ServiceResourceID, ClientId, redirectUri, new PlatformParameters(PromptBehavior.Always));
                signInButton.Content = "Sign out";
            }
            catch (AdalException ex)
            {
                serviceResult.Text = ex.ToString();
            }
        }

        private async void callServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (result == null)
                serviceResult.Text = "you need to sign in first";
            try
            {
                result = await authContext.AcquireTokenAsync(ServiceResourceID, ClientId, redirectUri,
                    new PlatformParameters(PromptBehavior.Never));
            }
            catch (AdalException ex)
            {
                serviceResult.Text = ex.ToString();
                return;
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await httpClient.GetAsync(ServiceBaseAddress + "api/Values/1");
            if (response.IsSuccessStatusCode)
            {
                string s = await response.Content.ReadAsStringAsync();
                serviceResult.Text = s;
            }
            else
                serviceResult.Text = "An error occured: " + response.ReasonPhrase;

        }

        private void ClearCookies()
        {
            const int INTERNET_OPTION_END_BROWSER_SESSION = 42;
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);

        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);


    }
}
