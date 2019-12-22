using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvernoteSDK.Tests
{
    /// <summary>
    /// Summary description for ENSession_FindNotes
    /// </summary>
    [TestClass]
    public class ENSession_FindNotes
    {
        public ENSession_FindNotes()
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
        
        //Use ClassInitialize to run code before running the first test in the class
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
        public void WhenSearchForAlreadyUploadedNote_ShouldSucceed()
        {
            string textToFind = Guid.NewGuid().ToString();
            ENNote myPlainNote = new ENNote();
            myPlainNote.Title = textToFind;
            myPlainNote.Content = ENNoteContent.NoteContentWithString(textToFind + "My plain text note");
            ENSession.SharedSession.UploadNote(myPlainNote, null);

            List<ENSessionFindNotesResult> myResultsList = ENSession.SharedSession.FindNotes(ENNoteSearch.NoteSearch(textToFind), null, ENSession.SearchScope.All, ENSession.SortOrder.RecentlyUpdated, 500);
            int noteCount = 0;
            if (myResultsList.Count > 0)
            {
                foreach (ENSessionFindNotesResult result in myResultsList)
                {
                    if(string.Equals( result.Title,textToFind)) {
                        noteCount += 1;
                    }
                }
            }
            Assert.IsTrue(noteCount == 1, "Either search failed or returned more results.");
        }
    }
}
