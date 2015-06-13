using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EvernoteSDK.Tests
{
    [TestClass]
    public class ENSession_DeleteNote
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtils.AuthenticateToEverNote();
        }
        [TestMethod]
        public void WhenNoteContainsHtml_ShoudBeAbleToDelete()
        {
            // Create a new note (in the user's default notebook) with some HTML content.
            ENNote myFancyNote = new ENNote();
            myFancyNote.Title = "My fancy note to delete";
            myFancyNote.Content = ENNoteContent.NoteContentWithSanitizedHTML("<p>Hello, world - <i>this</i> is a <b>fancy</b> note - and here is a table:</p><br /> <br/><table border=\"1\" cellpadding=\"2\" cellspacing=\"0\" width=\"100%\"><tr><td>Red</td><td>Green</td></tr><tr><td>Blue</td><td>Yellow</td></tr></table>");
            ENNoteRef myFancyNoteRef = ENSession.SharedSession.UploadNote(myFancyNote, null);

            // Delete the HTML content note we just created.
            ENSession.SharedSession.DeleteNote(myFancyNoteRef);
        }
    }
}
