using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvernoteSDK.Tests
{
    class TestConfiguration
    {
        /// <summary>
        /// If its true use ENSession.SetSharedSessionDeveloperToken("developer token", "note store url);
        /// Else use ENSession.SetSharedSessionConsumerKey("your key", "your secret"); for authentication
        /// </summary>
        internal static bool ShouldUseDeveloperTokenToAuthenticate { get; }=true;
        /// <summary>
        /// Path and filename of a JPG file on your computer. Be sure to replace this with a real JPG file
        /// </summary>
        internal static string PathToJPEGFile { get; }= @"";

        // Be sure to put your own development token here.
        public static string DeveloperToken { get;  } = "your development token here";
        public static string NoteStoreUrl { get;  } = "url to your note store";
        // Be sure to put your own consumer key and consumer secret here.
        public static string SessionConsumerKey { get; }= "your key";
        public static string SessionConsumerSecret { get;  }= "your secret";
    }
}
