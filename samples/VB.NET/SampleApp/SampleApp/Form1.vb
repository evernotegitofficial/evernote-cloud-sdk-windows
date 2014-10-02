Imports EvernoteSDK
Imports System.IO

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' Be sure to put your own consumer key and consumer secret here.
        ENSession.SetSharedSessionConsumerKey("your key", "your secret")

        If ENSession.SharedSession.IsAuthenticated = False Then
            ENSession.SharedSession.AuthenticateToEvernote()
        End If

        ' Get a list of all notebooks in the user's account.
        Dim myNotebookList As List(Of ENNotebook) = ENSession.SharedSession.ListNotebooks()

        ' Create a new note (in the user's default notebook) with some plain text content.
        Dim myPlainNote As ENNote = New ENNote()
        myPlainNote.Title = "My plain text note"
        myPlainNote.Content = ENNoteContent.NoteContentWithString("Hello, world!")
        Dim myPlainNoteRef As ENNoteRef = ENSession.SharedSession.UploadNote(myPlainNote, Nothing)

        ' Share this new note publicly.  "shareUrl" is the public URL to distribute to access the note.
        Dim shareUrl = ENSession.SharedSession.ShareNote(myPlainNoteRef)

        ' Create a new note (in the user's default notebook) with some HTML content.
        Dim myFancyNote As ENNote = New ENNote()
        myFancyNote.Title = "My plain text note"
        myFancyNote.Content = ENNoteContent.NoteContentWithSanitizedHTML("<p>Hello, world - <i>this</i> is a <b>fancy</b> note - and here is a table:</p><br /> <br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" width=""100%""><tr><td>Red</td><td>Green</td></tr><tr><td>Blue</td><td>Yellow</td></tr></table>")
        Dim myFancyNoteRef As ENNoteRef = ENSession.SharedSession.UploadNote(myFancyNote, Nothing)

        ' Delete the HTML content note we just created.
        ENSession.SharedSession.DeleteNote(myFancyNoteRef)

        ' Create a new note with a resource.
        Dim myResourceNote As ENNote = New ENNote()
        myResourceNote.Title = "My test note with a resource"
        myResourceNote.Content = ENNoteContent.NoteContentWithString("Hello, resource!")
        Dim myFile As Byte() = StreamFile("<complete path and filename of a JPG file on your computer>")    ' Be sure to replace this with a real JPG file
        Dim myResource As ENResource = New ENResource(myFile, "image/jpg", "My Best Shot.jpg")
        myResourceNote.Resources.Add(myResource)
        Dim myResourceRef As ENNoteRef = ENSession.SharedSession.UploadNote(myResourceNote, Nothing)

        ' Search for some text across all notes (i.e. personal, shared, and business).
        ' Change the Search Scope parameter to limit the search to only personal, shared, business - or combine flags for some combination.
        Dim textToFind = "some text to find*"
        Dim myResultsList As List(Of ENSessionFindNotesResult) = ENSession.SharedSession.FindNotes(ENNoteSearch.NoteSearch(textToFind), Nothing, ENSession.SearchScope.All, ENSession.SortOrder.RecentlyUpdated, 500)
        Dim personalCount As Integer = 0
        Dim sharedCount As Integer = 0
        Dim bizCount As Integer = 0
        If myResultsList.Count > 0 Then
            For Each note As ENSessionFindNotesResult In myResultsList
                Select Case note.NoteRef.Type
                    Case ENNoteRef.ENNoteRefType.TypePersonal
                        personalCount += 1
                    Case ENNoteRef.ENNoteRefType.TypeShared
                        sharedCount += 1
                    Case ENNoteRef.ENNoteRefType.TypeBusiness
                        bizCount += 1
                End Select
            Next
        End If

        If myResultsList.Count > 0 Then
            ' Given a NoteRef instance, download that note.
            Dim myDownloadedNote As ENNote = ENSession.SharedSession.DownloadNote(myResultsList(0).NoteRef)

            ' Serialize a NoteRef.
            Dim mySavedRef As Byte() = myResultsList(0).NoteRef.AsData
            ' And deserialize it.
            Dim newRef As ENNoteRef = ENNoteRef.NoteRefFromData(mySavedRef)

            ' Download the thumbnail for a note; then display it on this app's form.
            Dim thumbnail As Byte() = ENSession.SharedSession.DownloadThumbnailForNote(myResultsList(0).NoteRef, 120)
            Try
                Dim ms As New MemoryStream(thumbnail, 0, thumbnail.Length)
                ms.Position = 0
                Dim image1 As System.Drawing.Image = System.Drawing.Image.FromStream(ms, False, False)
                PictureBoxThumbnail.Image = image1
            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try

            ' Display a note's content as HTML in a WebBrowser control.
            Dim myContent = myDownloadedNote.HtmlContent
            WebBrowser1.DocumentText = myContent
        End If

    End Sub


    ' Support routine for displaying a note thumbnail.
    Private Function StreamFile(ByVal filename As String) As Byte()
        Dim fs As New FileStream(filename, FileMode.Open, FileAccess.Read)

        ' Create a byte array of file stream length
        Dim ImageData As Byte() = New Byte(CInt(fs.Length - 1)) {}

        'Read block of bytes from stream into the byte array
        fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length))

        'Close the File Stream
        fs.Close()
        'return the byte data
        Return ImageData
    End Function

End Class