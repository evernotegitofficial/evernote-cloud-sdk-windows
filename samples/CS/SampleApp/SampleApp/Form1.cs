using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using EvernoteSDK;

namespace SampleApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Be sure to put your own consumer key and consumer secret here.
            ENSession.SetSharedSessionConsumerKey("your key", "your secret");

            if (ENSession.SharedSession.IsAuthenticated == false)
            {
                ENSession.SharedSession.AuthenticateToEvernote();
            }

            // Get a list of all notebooks in the user's account.
            List<ENNotebook> myNotebookList = ENSession.SharedSession.ListNotebooks();

            // Create a new note (in the user's default notebook) with some plain text content.
            ENNote myPlainNote = new ENNote();
            myPlainNote.Title = "My plain text note";
            myPlainNote.Content = ENNoteContent.NoteContentWithString("Hello, world!");
            ENNoteRef myPlainNoteRef = ENSession.SharedSession.UploadNote(myPlainNote, null);

            // Share this new note publicly.  "shareUrl" is the public URL to distribute to access the note.
            string shareUrl = ENSession.SharedSession.ShareNote(myPlainNoteRef);

            // Create a new note (in the user's default notebook) with some HTML content.
            ENNote myFancyNote = new ENNote();
            myFancyNote.Title = "My first note";
            myFancyNote.Content = ENNoteContent.NoteContentWithSanitizedHTML("<p>Hello, world - <i>this</i> is a <b>fancy</b> note - and here is a table:</p><br /> <br/><table border=\"1\" cellpadding=\"2\" cellspacing=\"0\" width=\"100%\"><tr><td>Red</td><td>Green</td></tr><tr><td>Blue</td><td>Yellow</td></tr></table>");
            ENNoteRef myFancyNoteRef = ENSession.SharedSession.UploadNote(myFancyNote, null);

            // Delete the HTML content note we just created.
            ENSession.SharedSession.DeleteNote(myFancyNoteRef);

            // Create a new note with a resource.
            ENNote myResourceNote = new ENNote();
            myResourceNote.Title = "My test note with a resource";
            myResourceNote.Content = ENNoteContent.NoteContentWithString("Hello, resource!");
            byte[] myFile = StreamFile("<complete path and filename of a JPG file on your computer>");    // Be sure to replace this with a real JPG file
            ENResource myResource = new ENResource(myFile, "image/jpg", "My First Picture.jpg");
            myResourceNote.Resources.Add(myResource);
            ENNoteRef myResourceRef = ENSession.SharedSession.UploadNote(myResourceNote, null);

            // Search for some text across all notes (i.e. personal, shared, and business).
            // Change the Search Scope parameter to limit the search to only personal, shared, business - or combine flags for some combination.
            string textToFind = "some text to find*";
            List<ENSessionFindNotesResult> myResultsList = ENSession.SharedSession.FindNotes(ENNoteSearch.NoteSearch(textToFind), null, ENSession.SearchScope.All, ENSession.SortOrder.RecentlyUpdated, 500);
            int personalCount = 0;
            int sharedCount = 0;
            int businessCount = 0;
            if (myResultsList.Count > 0)
            {
                foreach (ENSessionFindNotesResult result in myResultsList)
                {
                    if (result.NoteRef.Type == ENNoteRef.ENNoteRefType.TypePersonal)
                    {
                        personalCount++;
                    }
                    else if (result.NoteRef.Type == ENNoteRef.ENNoteRefType.TypeShared)
                    {
                        sharedCount++;
                    }
                    else if (result.NoteRef.Type == ENNoteRef.ENNoteRefType.TypeBusiness)
                    {
                        businessCount++;
                    }
                }
            }

            if (myResultsList.Count > 0)
            {
                // Given a NoteRef instance, download that note.
                ENNote myDownloadedNote = ENSession.SharedSession.DownloadNote(myResultsList[0].NoteRef);

                // Serialize a NoteRef.
                byte[] mySavedRef = myResultsList[0].NoteRef.AsData();
                // And deserialize it.
                ENNoteRef newRef = ENNoteRef.NoteRefFromData(mySavedRef);

                // Download the thumbnail for a note; then display it on this app's form.
                byte[] thumbnail = ENSession.SharedSession.DownloadThumbnailForNote(myResultsList[0].NoteRef, 120);
                try
                {
                    MemoryStream ms = new MemoryStream(thumbnail, 0, thumbnail.Length);
                    ms.Position = 0;
                    System.Drawing.Image image1 = System.Drawing.Image.FromStream(ms, false, false);
                    pictureBoxThumbnail.Image = image1;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                // Display a note's content as HTML in a WebBrowser control.
                string myContent = myDownloadedNote.HtmlContent;
                webBrowser1.DocumentText = myContent;
            }
        }


        // Support routine for displaying a note thumbnail.
         static byte[] StreamFile(string filename)
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
    }
}
