using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EvernoteOAuthNet;

namespace AuthSample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*
             * To use EvernoteOAuth:
             *      1. Instantiate the class, indicating via the parameters whether you're targeting the Sandbox, Production, or Yinxiang Biji service, and supplying your ConsumerKey and ConsumerSecret.
             *         You can also optionally pass a boolean True if you're using App Notebooks and want to allow the user to select a Shared/Business notebook as the App Notebook,
             *         as well as optionally the caption for the authorization window, if you want to override the default caption.
             *      2. Call EvernoteOAuth.Authorize.
             *      3. If the user's account was not successfully authorized, the specific error text will be returned from the Authorize call.
             *         If the authorization was successful, the return value will be null, and the following fields will be set and available for you to query:
             *              Token
             *              Expires
             *              NoteStoreUrl
             *              UserId
             *              WebApiUrlPrefix
             *              LinkedAppNotebookSelected
             */

            string myConsumerKey = "Specify your Consumer Key here";
            string myConsumerSecret = "Specify your Consumer Secret here";
            EvernoteOAuth oauth = new EvernoteOAuth(EvernoteOAuth.HostService.Production, myConsumerKey, myConsumerSecret, true);
            string errResponse = oauth.Authorize();
            if (errResponse.Length == 0)
            {
                MessageBox.Show(string.Format("Token: {0}\r\n\r\nExpires: {1}\r\n\r\nNoteStoreUrl: {2}\r\n\r\nUserId: {3}\r\n\r\nWebApiUrlPrefix: {4}\r\n\r\nLinked App Notebook was selected: {5}", oauth.Token, oauth.Expires, oauth.NoteStoreUrl, oauth.UserId, oauth.WebApiUrlPrefix, oauth.LinkedAppNotebookSelected.ToString()));
            }
            else
            {
                MessageBox.Show("A problem has occurred in attempting to authorize the use of your Evernote account: " + errResponse);
            }
        }

    }
}
