using gWiiFitBit.Lib;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace gWiiFitBit.FormsApp
{
    public partial class MainForm : Form
    {
        Thread wiiBoardThread;
        WiiBoard wiiBoard;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            wiiBoard = new WiiBoard();
            wiiBoard.OnWeightChanged += OnWeightChanged;
            wiiBoardThread = new Thread(() =>
            {
                wiiBoard.Connect();
            });
            wiiBoardThread.Start();            
        }

        private void OnWeightChanged(Weight weight)
        {
            this.Invoke((MethodInvoker)delegate
            {
                lblWeight.Text = weight.ToString();
            });
            UploadWeightToFitBit(weight);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (wiiBoardThread.IsAlive) wiiBoardThread.Abort();
            wiiBoard.Disconnect();
        }

        private void UploadWeightToFitBit(Weight weight)
        {
            this.Invoke((MethodInvoker)delegate
            {
                label1.Text = "Uploading...";
            });
            //TODO : on receive Token and token secret store them somewhere so the user does not have to approve
            //access to the FitBit account every time. This needs to be removed from App.Config and is just there
            //for testing. 

            var consumerKey = System.Configuration.ConfigurationManager.AppSettings["ConsumerKey"];
            var consumerSecret = System.Configuration.ConfigurationManager.AppSettings["ConsumerSecret"];
            var OAuthToken = System.Configuration.ConfigurationManager.AppSettings["OAuthToken"];
            var OAuthTokenSecret = System.Configuration.ConfigurationManager.AppSettings["OAuthTokenSecret"];
            var baseUrl = System.Configuration.ConfigurationManager.AppSettings["OAuthBaseURL"];
            var requestUrl = baseUrl + System.Configuration.ConfigurationManager.AppSettings["OAuthRequestURL"];
            var AccessUrl = baseUrl + System.Configuration.ConfigurationManager.AppSettings["OAuthAccessUrl"];
            var authorizeUrl = baseUrl + System.Configuration.ConfigurationManager.AppSettings["OAuthAuthorizeUrl"];
            
            /*
            var client = new RestClient(baseUrl);
            client.Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
            var request = new RestRequest(requestUrl.Replace(baseUrl,""), Method.POST);
            var response = client.Execute(request);
            var qs = RestSharp.Contrib.HttpUtility.ParseQueryString(response.Content);
            OAuthToken = qs["oauth_token"];
            OAuthTokenSecret = qs["oauth_token_secret"];
            request = new RestRequest(authorizeUrl.Replace(baseUrl,""));
            request.AddParameter("oauth_token", OAuthToken);
            var url = client.BuildUri(request).ToString();
            url.Replace(baseUrl, "https://www.fitbit.com/");
            Process.Start(url);
            var verifier = "p8f6pp53c7ng2mqbfcaucrm3h0"; // <-- Breakpoint here (set verifier in debugger)
            request = new RestRequest("oauth/access_token", Method.POST);
            client.Authenticator = OAuth1Authenticator.ForAccessToken(consumerKey, consumerSecret, OAuthToken, OAuthTokenSecret, verifier);
            response = client.Execute(request);
            qs = RestSharp.Contrib.HttpUtility.ParseQueryString(response.Content);
            OAuthToken = qs["oauth_token"];
            OAuthTokenSecret = qs["oauth_token_secret"];
            */

            var client = new RestClient(baseUrl);
            var request = new RestRequest("1/user/-/body/log/weight.json", Method.POST);
            request.AddParameter("weight", weight.TotalKg.ToString());
            request.AddParameter("date", DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"));
            request.AddParameter("time", DateTime.UtcNow.ToString("HH:mm:ss"));
            client.Authenticator = OAuth1Authenticator.ForProtectedResource(consumerKey, consumerSecret, OAuthToken, OAuthTokenSecret);
            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label1.Text = "Uploaded";
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label1.Text = "Upload Failed";
                });
            }
        }


    }
}