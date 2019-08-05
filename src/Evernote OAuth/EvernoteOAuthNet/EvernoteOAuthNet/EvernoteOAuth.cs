using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace EvernoteOAuthNet
{
    public class EvernoteOAuth
    {

        public EvernoteOAuth(HostService service, string consumerKey, string consumerSecret, string windowTitle = "Please provide authorization to access your Evernote account")
        {
            _service = service;
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _windowTitle = windowTitle;
            _supportLinkedAppNotebook = false;
        }

        public EvernoteOAuth(HostService service, string consumerKey, string consumerSecret, bool supportLinkedAppNotebook, string windowTitle = "Please provide authorization to access your Evernote account")
        {
            _service = service;
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _supportLinkedAppNotebook = supportLinkedAppNotebook;
            _windowTitle = windowTitle;
        }

        public enum HostService { Production, Sandbox, Yinxiang };

        private string _consumerKey;
        private string _consumerSecret;
        private HostService _service;
        private string _windowTitle;

        private oAuthEvernote _oauth;

        private string _oauth_token;
        private string _edam_noteStoreUrl;
        private string _edam_userId;
        private long _edam_expires;
        private string _edam_webApiUrlPrefix;
        private bool _supportLinkedAppNotebook = false;
        private bool _linkedAppNotebookSelected;

        public string Token { get { return _oauth_token; } set { _oauth_token = value; } }
        public string NoteStoreUrl { get { return _edam_noteStoreUrl; } set { _edam_noteStoreUrl = value; } }
        public string UserId { get { return _edam_userId; } set { _edam_userId = value; } }
        public long Expires { get { return _edam_expires; } set { _edam_expires = value; } }
        public string WebApiUrlPrefix { get { return _edam_webApiUrlPrefix; } set { _edam_webApiUrlPrefix = value; } }
        public bool SupportLinkedAppNotebook { get { return _supportLinkedAppNotebook; } set { _supportLinkedAppNotebook = value; } }
        public bool LinkedAppNotebookSelected { get { return _linkedAppNotebookSelected; } set { _linkedAppNotebookSelected = value; } }


        public string Authorize()
        {
            try
            {
                _oauth = new oAuthEvernote(_service, _consumerKey, _consumerSecret, _supportLinkedAppNotebook, _windowTitle);
                String requestToken = _oauth.getRequestToken();
                // txtOutput.Text += "\n" + "Received request token: " + requestToken;

                _oauth.authorizeToken(_windowTitle);
                // txtOutput.Text += "\n" + "Token was authorized: " + _oauth.Token + " with verifier: " + _oauth.Verifier;

                NameValueCollection accessInfo = _oauth.getAccessToken();

                Token = accessInfo["oauth_token"];
                NoteStoreUrl = accessInfo["edam_noteStoreUrl"];
                UserId = accessInfo["edam_userId"];
                Expires = Convert.ToInt64(accessInfo["edam_expires"]);
                WebApiUrlPrefix = accessInfo["edam_webApiUrlPrefix"];
                LinkedAppNotebookSelected = _oauth.LinkedAppNotebookSelected;
                return string.Empty;

                // txtOutput.Text += "\n" + "Access token was received: " + _oauth.Token;
            }
            catch (Exception exp)
            {
                // txtOutput.Text += "\nException: " + exp.Message;
                return exp.Message;
            }
        }

    }
}
