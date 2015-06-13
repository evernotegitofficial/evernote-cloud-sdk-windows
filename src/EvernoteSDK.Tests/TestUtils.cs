using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EvernoteSDK.Tests
{
    internal class TestUtils
    {

        // Support routine for displaying a note thumbnail.
        internal static byte[] StreamFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

            // Create a byte array of file stream length
            byte[] ImageData = new byte[Convert.ToInt32(fs.Length - 1) + 1];

            //Read block of bytes from stream into the byte array
            fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));

            //Close the File Stream
            fs.Close();
            //return the byte data
            return ImageData;
        }

        internal static void AuthenticateToEverNote()
        {
            if (TestConfiguration.ShouldUseDeveloperTokenToAuthenticate)
            {
                ENSession.SetSharedSessionDeveloperToken(TestConfiguration.DeveloperToken, TestConfiguration.NoteStoreUrl);
            }
            else
            {
                ENSession.SetSharedSessionConsumerKey(TestConfiguration.SessionConsumerKey, TestConfiguration.SessionConsumerSecret);
            }

            if (ENSession.SharedSession.IsAuthenticated == false)
            {
                ENSession.SharedSession.AuthenticateToEvernote();
            }
        }
    }
}
