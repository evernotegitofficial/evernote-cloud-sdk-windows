using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvernoteSDK.Tests
{
    [TestClass]
    public class ENSession_ShareNote
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtils.AuthenticateToEverNote();
        }

        [TestMethod]
        public void WhenSharingSimpleNote_ShouldReturnSharedNoteUrl()
        {
            ENNote myPlainNote = new ENNote();
            myPlainNote.Title = "My plain text note to share";
            myPlainNote.Content = ENNoteContent.NoteContentWithString("Hello, world! I am sharing");
            ENNoteRef myPlainNoteRef = ENSession.SharedSession.UploadNote(myPlainNote, null);

            // Share this new note publicly.  "shareUrl" is the public URL to distribute to access the note.
            string shareUrl = ENSession.SharedSession.ShareNote(myPlainNoteRef);
            Assert.IsFalse(string.IsNullOrWhiteSpace(shareUrl), "Not able to obtain shared note url");
        }
    }
}
