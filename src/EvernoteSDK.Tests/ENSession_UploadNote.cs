using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace EvernoteSDK.Tests
{
    [TestClass]
    public class ENSession_UploadNote
    {
        [TestInitialize]
        public void Initialize()
        {
            TestUtils.AuthenticateToEverNote();
        }
        [TestMethod]
        public void WhenContentIsSimpleText_ShouldSucceed()
        {
            // Get a list of all notebooks in the user's account.
            List<ENNotebook> myNotebookList = ENSession.SharedSession.ListNotebooks();

            // Create a new note (in the user's default notebook) with some plain text content.
            ENNote myPlainNote = new ENNote();
            myPlainNote.Title = "My plain text note";
            myPlainNote.Content = ENNoteContent.NoteContentWithString("Hello, world!");
            ENNoteRef myPlainNoteRef = ENSession.SharedSession.UploadNote(myPlainNote, null);
        }
        [TestMethod]
        public void WhenContentIsHtml_ShouldSucceed()
        {
            // Create a new note (in the user's default notebook) with some HTML content.
            ENNote myFancyNote = new ENNote();
            myFancyNote.Title = "My first note";
            myFancyNote.Content = ENNoteContent.NoteContentWithSanitizedHTML("<p>Hello, world - <i>this</i> is a <b>fancy</b> note - and here is a table:</p><br /> <br/><table border=\"1\" cellpadding=\"2\" cellspacing=\"0\" width=\"100%\"><tr><td>Red</td><td>Green</td></tr><tr><td>Blue</td><td>Yellow</td></tr></table>");
            ENNoteRef myFancyNoteRef = ENSession.SharedSession.UploadNote(myFancyNote, null);
        }
        
        [TestMethod]
        public void WhenNoteContainsResource_ShouldSucceed()
        {
            // Create a new note with a resource.
            ENNote myResourceNote = new ENNote();
            myResourceNote.Title = "My test note with a resource";
            myResourceNote.Content = ENNoteContent.NoteContentWithString("Hello, resource!");
            byte[] myFile = TestUtils.StreamFile(TestConfiguration.PathToJPEGFile);
            ENResource myResource = new ENResource(myFile, "image/jpg", "My First Picture.jpg");
            myResourceNote.Resources.Add(myResource);
            ENNoteRef myResourceRef = ENSession.SharedSession.UploadNote(myResourceNote, null);

        }
    }
}
