using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvernoteSDK.Tests
{
    /// <summary>
    /// Summary description for ENSession_DownloadNotes
    /// </summary>
    [TestClass]
    public class ENSession_DownloadNotes
    {
        public ENSession_DownloadNotes()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            TestUtils.AuthenticateToEverNote();
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void WhenDownloadForAlreadyUploadedNote_ShouldDownloadCorrectNote()
        {
            string textToFind = Guid.NewGuid().ToString();
            ENNote myPlainNote = new ENNote();
            myPlainNote.Title = textToFind;
            myPlainNote.Content = ENNoteContent.NoteContentWithString(textToFind);
            ENSession.SharedSession.UploadNote(myPlainNote, null);

            List<ENSessionFindNotesResult> myResultsList = ENSession.SharedSession.FindNotes(ENNoteSearch.NoteSearch(textToFind), null, ENSession.SearchScope.All, ENSession.SortOrder.RecentlyUpdated, 500);
            bool contentDownloaded = false;
            if (myResultsList.Count > 0)
            {
                // Given a NoteRef instance, download that note.
                ENNote myDownloadedNote = ENSession.SharedSession.DownloadNote(myResultsList[0].NoteRef);
                contentDownloaded= string.Equals(myDownloadedNote.TextContent, textToFind);
            }
            Assert.IsTrue(contentDownloaded, "Note able to download required note");
        }
    }
}
