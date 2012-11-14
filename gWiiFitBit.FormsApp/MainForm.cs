using gWiiFitBit.FormsApp.FitBit;
using gWiiFitBit.Lib;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Contrib;
using System;
using System.Diagnostics;
using System.Threading;
using System.Web;
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
            /*    wiiBoard = new WiiBoard();
                wiiBoard.OnWeightChanged += OnWeightChanged;
                wiiBoardThread = new Thread(() =>
                {
                    wiiBoard.Connect();
                });
                wiiBoardThread.Start();*/
            UploadWeightToFitBit(new Weight(71));
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
        {/*
            Authenticator a = new Authenticator(
               System.Configuration.ConfigurationManager.AppSettings["ConsumerKey"],
               System.Configuration.ConfigurationManager.AppSettings["ConsumerSecret"],
               "http://api.fitbit.com/oauth/request_token",

               "http://api.fitbit.com/oauth/access_token",
               "http://www.fitbit.com/oauth/authorize"
               );
            a.GetAuthUrlToken();*/
            //Pin : pm4jaad5migqs153dddqi0p3o4

            var consumerKey = System.Configuration.ConfigurationManager.AppSettings["ConsumerKey"];
            var consumerSecret = System.Configuration.ConfigurationManager.AppSettings["ConsumerSecret"];
            var OAuthToken = System.Configuration.ConfigurationManager.AppSettings["OAuthToken"];
            var OAuthTokenSecret = System.Configuration.ConfigurationManager.AppSettings["OAuthTokenSecret"];
            /*
            var client = new RestClient("http://api.fitbit.com/");
            client.Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
            var request = new RestRequest("oauth/request_token", Method.POST);
            var response = client.Execute(request);
            var qs = RestSharp.Contrib.HttpUtility.ParseQueryString(response.Content);
            var oauth_token = qs["oauth_token"];
            var oauth_token_secret = qs["oauth_token_secret"];
            request = new RestRequest("oauth/authorize");
            request.AddParameter("oauth_token", oauth_token);
            var url = client.BuildUri(request).ToString();
            Process.Start(url);

            var verifier = "ep5pmianh5lg1n54ef9hhf9a4v"; // <-- Breakpoint here (set verifier in debugger)
            request = new RestRequest("oauth/access_token", Method.POST);
            client.Authenticator = OAuth1Authenticator.ForAccessToken(
                consumerKey, consumerSecret, oauth_token, oauth_token_secret, verifier
            );
            response = client.Execute(request);

            qs = RestSharp.Contrib.HttpUtility.ParseQueryString(response.Content);
            oauth_token = qs["oauth_token"];
            */
            //Perform Actions
            var client = new RestClient("http://api.fitbit.com/");
            client.Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
            var request = new RestRequest("1/user/-/body/log/weight.json", Method.POST);
            request.AddParameter("weight", weight.TotalStone.ToString());
            request.AddParameter("date", DateTime.UtcNow.ToString("yyyy-MM-dd"));
        //    request.AddParameter("time", DateTime.UtcNow.ToString("HH:mm:ss"));
            client.Authenticator = OAuth1Authenticator.ForProtectedResource(
                consumerKey, consumerSecret, OAuthToken, OAuthTokenSecret);
            var response = client.Execute(request);
        }


    }
}