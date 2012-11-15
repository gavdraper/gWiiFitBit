using gWiiFitBit.Lib;
using RestSharp;
using RestSharp.Authenticators;
using System;
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
        {
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

            /*            Authenticator a = new Authenticator(consumerKey,consumerSecret,requestUrl, AccessUrl,authorizeUrl);
            a.GetAuthUrlToken();
            var client = new RestClient(baseUrl);
            client.Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
            var request = new RestRequest(requestUrl.Replace(baseUrl,""), Method.POST);
            var response = client.Execute(request);
            var qs = RestSharp.Contrib.HttpUtility.ParseQueryString(response.Content);
            var oauth_token = qs["oauth_token"];
            var oauth_token_secret = qs["oauth_token_secret"];
            request = new RestRequest(authorizeUrl.Replace(baseUrl,"");
            request.AddParameter("oauth_token", oauth_token);
            var url = client.BuildUri(request).ToString();
            Process.Start(url);
            var verifier = "ep5pmianh5lg1n54ef9hhf9a4v"; // <-- Breakpoint here (set verifier in debugger)
            request = new RestRequest("oauth/access_token", Method.POST);
            client.Authenticator = OAuth1Authenticator.ForAccessToken(consumerKey, consumerSecret, oauth_token, oauth_token_secret, verifier);
            response = client.Execute(request);
            qs = RestSharp.Contrib.HttpUtility.ParseQueryString(response.Content);
            oauth_token = qs["oauth_token"];*/

            var client = new RestClient(baseUrl);
            client.Authenticator = OAuth1Authenticator.ForRequestToken(consumerKey, consumerSecret);
            var request = new RestRequest("1/user/-/body/log/weight.json", Method.POST);
            request.AddParameter("weight", weight.TotalStone.ToString());
            request.AddParameter("date", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            //    request.AddParameter("time", DateTime.UtcNow.ToString("HH:mm:ss"));
            client.Authenticator = OAuth1Authenticator.ForProtectedResource(consumerKey, consumerSecret, OAuthToken, OAuthTokenSecret);
            var response = client.Execute(request);
        }


    }
}