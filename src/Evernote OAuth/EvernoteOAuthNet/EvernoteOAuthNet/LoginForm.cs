using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.Collections.Specialized;

namespace EvernoteOAuthNet
{
    internal partial class LoginForm : Form
    {
        private oAuthEvernote _oauth;
        private String _token;
        private String _verifier;
        private String _tokenSecret;
        private bool _linkedAppNotebookSelected;

        public String Token
        {
            get
            {
                return _token;
            }
        }

        public String Verifier
        {
            get
            {
                return _verifier;
            }
        }

        public String TokenSecret
        {
            get
            {
                return _tokenSecret;
            }
        }

        public bool LinkedAppNotebookSelected
        {
            get
            {
                return _linkedAppNotebookSelected;
            }
        }


        public LoginForm(oAuthEvernote o, string windowTitle)
        {
            _oauth = o;
            _token = null;
            InitializeComponent();
            this.Text = windowTitle;
            this.addressTextBox.Text = o.AuthorizationLink;
            _token = _oauth.Token;
            _tokenSecret = _oauth.TokenSecret;
            browser.Navigate(new Uri(_oauth.AuthorizationLink));            

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            this.addressTextBox.Text = e.Url.ToString();
        }

        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //if (browser.Url.ToString().Contains(_oauth.CALLBACK_URL) | browser.Url.ToString().Contains(_oauth.CALLBACK_ALT_URL))
            if (!browser.Url.ToString().Contains(@"evernote.com") & !browser.Url.ToString().Contains(@"yinxiang.com"))
                {
                string queryParams = e.Url.Query;
                if (queryParams.Length > 0)
                {
                    //Store the Token and Token Secret
                    NameValueCollection qs = HttpUtility.ParseQueryString(queryParams);
                    if (qs["oauth_token"] != null)
                    {
                        _token = qs["oauth_token"];
                    }
                    if (qs["oauth_verifier"] != null)
                    {
                        _verifier = qs["oauth_verifier"];
                    }
                    if (qs["sandbox_lnb"] != null)
                    {
                        _linkedAppNotebookSelected = qs["sandbox_lnb"] == "true" ? true : false;
                    }
                }
                this.Close();
            }
        }

        private void addressTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Cancel the key press so the user can't enter a new url
            e.Handled = true; 
        }

    }
}
