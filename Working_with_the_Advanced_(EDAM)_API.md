Working with the Advanced (EDAM) API
---

The Evernote Cloud SDK for Windows exposes a simple, workflow oriented set of operations and objects to enable the most common Evernote tasks: creating and uploading notes; and finding, downloading and displaying existing notes. However, Evernote is capable of far more than is exposed directly from `ENSession`. To provide the full breadth of service capabilities, the SDK also provides access to the "EDAM" API, which is a reflection of the actual web service API. This has the benefits of allowing your app to do anything an Evernote client can do, including syncing, working with tags, saved searches, image recognition data, and more. However, it also requires somewhat more detailed knowledge. This guide explains how to access the EDAM API, how to use it in combination with the higher-level functions, and where to go for more information.

### What is the EDAM API?

The EDAM (Evernote Data Access and Management) API is the name given to Evernote's full web service API, through which all Evernote and third-party apps access the service. It is not a REST-style interface; instead, Evernote provides client code that can talk to the service on your behalf. That API is [documented in full here](http://dev.evernote.com/doc/reference/). 

### Objects and basic concepts

Generally, a user's account information is hosted on a server endpoint called a "note store". You'll use a note store client object to call methods on the service, and use EDAM objects to send and receive data. 

When using the EDAM objects, if you want to create or read note content, be aware that it is kept natively in a form of HTML called ENML (Evernote Markup Language). There is a helper class called `ENMLWriter` in the SDK to help you produce this format.

### Accessing the EDAM API in the SDK

By default, the Using directive you include for the SDK is intentionally compact and doesn't include the EDAM objects. To use them in your app, in addition to specifying `EvernoteSDK`, also add `EvernoteSDKAdvanced`.

    using EvernoteSDK;
    using EvernoteSDKAdvanced;

The `EvernoteNSDKAdvanced` directive will give you easy access to the full EDAM layer, and also expose some additional properties on the `EN...` objects that will let you bridge between the two worlds more easily. 

In addition, when supplying your consumer key and secret (or your developer token), use the `ENSessionAdvanced` object instead of `ENSession`.

    ENSessionAdvanced.SetSharedSessionConsumerKey("your key", "your secret");

After that, you're free to use either the `ENSession` or `ENSessionAdvanced` object, although you'll need to use the latter when accessing Advanced functionality.

### Getting a note store client

Once authenticated, the shared session knows how to provide multiple note store clients depending on what you want. Different "domains" of user data are sometimes kept in different places, and require different note store clients. All personal notes are kept in one place, but if the user is a member of a business, the business data will use a different client. And each shared notebook the user has joined will use an independent note store client:

    ENNoteStoreClient PrimaryNoteStore;
    ENNoteStoreClient BusinessNoteStore;
    ENNoteStoreClient NoteStoreForLinkedNotebook(linkedNotebook);

The primary note store is always valid, and accesses a user's personal data. The business note store will be `null` unless the user is connected to a business. Do not instantiate the store clients directly. The session will handle authenticating to linked and business note stores for you automatically.

### Hello World example

The following code snippet demonstrates how to create and upload a trivial new note using the EDAM API:

    // First create the ENML content for the note.
    ENMLWriter writer = new ENMLWriter();
    writer.WriteStartDocument();
    writer.WriteString("Hello again, world.");
    writer.WriteEndDocument();

    // Create a note locally.
    Note myNote = new Note();
    myNote.Title = "Sample note from the Advanced world";
    myNote.Content = writer.Contents.ToString();

    // Create the note in the service, in the user's personal, default notebook.
    ENNoteStoreClient store = ENSessionAdvanced.SharedSession.PrimaryNoteStore;
    Note resultNote = store.CreateNote(myNote);

### Working with dates

The SDK adds two extension methods, `ToEdamTimestamp` and `ToDateTime`, to convert between Evernote's internal date/time format and a DateTime variable.

            myNoteAdv.EdamAttributes["ReminderOrder"] = DateTime.Now.ToEdamTimestamp();

            DateTime noteCreatedOn = myNote.Created.ToDateTime();

### Bridging with the `EN*` classes 

You might want to, say, use the handy unified `ListNotebooks` method on ENSession, or `FindNotes` to save you some time, and then operate on the results using the EDAM methods. The normal `EvernoteSDK` include keeps all EDAM information private to avoid unnecessary confusion. However, once you add `EvernoteSDKAdvanced`, you get some additional properties on the standard objects that help you bridge the two APIs. 

`ENNoteRef` points to a note that exists somewhere on the service. Let's say you want to download that note using the EDAM API. You can ask the session for a corresponding note store, and then use the `Guid` property of the note ref to know which note you are referring to. e.g.

    ENNoteRef noteRef = /* some note ref from an earlier call to find or upload */
    ENNoteStoreClient noteStore = ENSessionAdvanced.SharedSession.NoteStoreForNoteRef(noteRef);
    Note edamNote = noteStore.GetNote(noteRef.Guid, true, false, false, false);

`ENNotebook` offers similar additions, so you can use the main SDK's "listNotebooks" method to get the merged set of all user-visible notebooks, and then use one of those notebooks directly in the EDAM API. 

`ENNoteContent` has an advanced initializer that allows you to set it up with ENML directly.

### Working with App Notebooks

If your app is set up to use App Notebooks, then as part of the authentication process, the user will create (or select) a notebook to restrict all app access to. If you're using the EDAM API directly, you need to be aware of where this notebook could be located-- it can be in a linked or business notebook. By default, the main API of ENSession knows how to handle any app notebook, but if you use the EDAM API, you can tell the session (before authenticating) if you're not prepared to handle linked or business notebooks, eg:

    ENSession.SharedSession.SupportsLinkedAppNotebook = false;

If you *are* willing to support this, though, and the user chooses one, you can find out by looking at the session's `AppNotebookIsLinked` property. If this is `true`, then you'll expect to call `ListLinkedNotebooks...` etc on the note store.

### Where to learn more

This guide only covers the basics of how to interface with the EDAM layer. For full documentation on the service and the API, please consult [dev.evernote.com](http://dev.evernote.com). API documentation and many guides with futher detail can be found linked from [http://dev.evernote.com/doc/](http://dev.evernote.com/doc/).
