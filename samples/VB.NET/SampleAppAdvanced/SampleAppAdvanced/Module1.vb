Imports EvernoteSDK
Imports EvernoteSDK.Advanced
Imports Evernote.EDAM.Type

Module Module1

    Sub Main()

        ' Supply your key using ENSessionAdvanced instead of ENEsssion, to indicate your use of the Advanced interface.
        ' Be sure to put your own consumer key and consumer secret here.
        ENSessionAdvanced.SetSharedSessionConsumerKey("your key", "your secret")

        If ENSession.SharedSession.IsAuthenticated = False Then
            ENSession.SharedSession.AuthenticateToEvernote()
        End If

        ' Create a note (in the user's default notebook) with an attribute set (in this case, the ReminderOrder attribute to create a Reminder).
        Dim myNoteAdv As New ENNoteAdvanced()
        myNoteAdv.Title = "Sample note with Reminder set"
        myNoteAdv.Content = ENNoteContent.NoteContentWithString("Hello, world - this note has a Reminder on it.")
        myNoteAdv.EdamAttributes("ReminderOrder") = DateTime.Now.ToEdamTimestamp()
        Dim myRef As ENNoteRef = ENSession.SharedSession.UploadNote(myNoteAdv, Nothing)

        ' Now we'll create an EDAM Note.
        ' First create the ENML content for the note.
        Dim writer As New ENMLWriter()
        writer.WriteStartDocument()
        writer.WriteString("Hello again, world.")
        writer.WriteEndDocument()
        ' Create a note locally.
        Dim myNote As New Note()
        myNote.Title = "Sample note from the Advanced world"
        myNote.Content = writer.Contents.ToString()
        ' Create the note in the service, in the user's personal, default notebook.
        Dim store As ENNoteStoreClient = ENSessionAdvanced.SharedSession.PrimaryNoteStore
        Dim resultNote As Note = store.CreateNote(myNote)
    End Sub

End Module
