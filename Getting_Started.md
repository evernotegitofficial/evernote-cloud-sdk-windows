Getting Started with the Evernote Cloud SDK for Windows
---

This document covers getting set up with the Cloud SDK for Windows, some quick examples, and some discussion of the primary classes. 

### Hello World!

Here's a quick example of how easy it is to create and upload a new note from your app:

    ENNote myPlainNote = new ENNote();
    myPlainNote.Title = "My plain text note";
    myPlainNote.Content = ENNoteContent.NoteContentWithString("Hello, world!");
    ENNoteRef myPlainNoteRef = ENSession.SharedSession.UploadNote(myPlainNote, null);

This creates a new, plaintext note, with a title, and uploads it to the user's default notebook. All you need to do first is get your app set up with the SDK. Here's how.

Setup
-----

### Register for an Evernote API key (and secret)...

You can do this on the [Evernote Developers portal page](http://dev.evernote.com/documentation/cloud/). Most applications will want to do this -- it's easy and instant. During development, you will point your app at Evernote's "sandbox" development environment. When you are ready to test on production, we will upgrade your key. (You can create test accounts on sandbox just by going to [sandbox.evernote.com](http://sandbox.evernote.com)).

### ...or get a Developer Token

You can also just test-drive the SDK against your personal production Evernote account, if you're afraid of commitment or are building a one-off tool for yourself. [Get a developer token here](https://www.evernote.com/api/DeveloperToken.action). Make sure to then use the alternate setup instructions given in the "Modify your application's startup code" section below.

### Include the library in your application

You have a few options:

- (Recommended) *Use Nuget.* From within Visual Studio, using the Nuget Package Manager, find and install "Evernote SDK for Windows" -- this will install the Evernote SDK and its required dependent assemblies into your C# or VB.NET project.

- (Alternative 1) From within Visual Studio, you can use the Add References option to add the DLL files contained in the "assemblies" folder of this repository into your C# or VB.NET project.

- (Alternative 2) If you want to build the entire SDK source alongside your own project files, you can do that too. The source code for this SDK is included here in the repository. Note: You'll need to start Visual Studio using "Run as Administrator" privileges in order to rebuild the SDK -- because it supports COM, it needs access to the Windows registry when it builds.

- *For use in a COM environment like Microsoft Office VBA.* Run the "EvernoteSDK_COMSetup.msi" installer that's included in the repository. This will install the necessary assemblies and register the SDK on your machine so it will be available to the COM subsystem.

### Add the standard include file to any file that uses the Evernote SDK

    #using EvernoteSDK;

### Modify your application's startup code

Do something like this in your application's startup code area. First you set up the `ENSession`, configuring it with your consumer key and secret. Then you perform authentication (see below for more on that).

    // Be sure to put your own consumer key and consumer secret here.
    ENSession.SetSharedSessionConsumerKey("your key", "your secret");

    if (ENSession.SharedSession.IsAuthenticated == false)
    {
        ENSession.SharedSession.AuthenticateToEvernote();
    }

Alternative if you're using a Developer Token (see above) to access *only* your personal, production account: *don't* set a consumer key/secret (or the sandbox environment). Instead, give the SDK your developer token and Note Store URL (both personalized and available from [this page](https://www.evernote.com/api/DeveloperToken.action)). Replace the setup call above with the following.

    ENSession.SetSharedSessionDeveloperToken("the token string that you got from us");

Now you're good to go (and your app will support the fast-path OAuth authentication that will be most effective for your users).

See it in action
----------------

Before we get into how to work with the simple object model to programmatically interact with Evernote, give it a spin the even easier way:

### Using the Sample App

The SDK comes with a simple sample application, called (shockingly) `SampleApp` so you can see the workflow for basic functionality. This app demonstrates authentication, creating some simple notes, and finding and displaying notes and thumbnails.

*Note* You will still need to edit the code of the sample to include your consumer key and secret. 

Basic Concepts
--------------

### Evernote Concepts

The object model in the SDK is designed to reflect a distilled version of the object model you're familiar with as an Evernote user. The most fundamental object in Evernote is the "note" (represented by an ENNote). A note is one chunk of content visible in a user's account. Its body is stored in a form of markup, and may have attached image or file "resources." A note also has a title, timestamps, tags, and other metadata. 

Notes exist inside of notebooks, and a user has at least one of these in their account. A user can move notes between notebooks using their Evernote client. Users can also join notebooks shared by other users. A user who is also a member of a [Business](http://evernote.com/business/) account will have access to business notebooks that they've created or joined.

The public objects in the SDK generally map to these objects.

### ENNote

An `ENNote` represents the complete set of content within a note (title, body, resources). This object, which you create and populate to upload, or receive as the result of a download operation, doesn't point to any specific note on the service; it's just a container for the content.

An `ENNote` can sport an array of `ENResource` objects that are like file attachments. A note has some content (represented by `ENNoteContent`) that makes up the "body" of the note. You can populate this from plaintext, HTML, etc.

### ENNoteRef

On the other hand, an `ENNoteRef` is an immutable, opaque reference to a specific note that exists on the service, in a user's account. When you upload an `ENNote`, you'll receive an `ENNoteRef` in response that points to the resulting service object. This object has convenience functions to serialize and deserialize it if you'd like to store it to access that service note at a later date.

### ENNotebook

An `ENNotebook` represents a notebook on the service. It has several properties that can tell you its name, business or sharing status, etc. 

### ENSession

The `ENSession` singleton (accessible via `SharedSession`) is the primary "interface" with the SDK. You'll use the session to authenticate a user with the service, and the session exposes the methods you'll use to perform Evernote operations.

Using the Evernote SDK
----------------------

### Authenticate

When your application starts up, if it's not already authenticated, you'll need to authenticate the `ENSession`.

    if (ENSession.SharedSession.IsAuthenticated == false)
    {
        ENSession.SharedSession.AuthenticateToEvernote();
    }

Calling `AuthenticateToEvernote` will start the OAuth process. ENSession will open a new Webbrowser window to display Evernote's OAuth web page and handle all the back-and-forth OAuth handshaking. When the user finishes this process, the Webbrowser window will be dismissed.

Authentication credentials are saved on the device once the user grants access, so this step is only necessary as part of an explicit linking. Subsequent access to the shared session will automatically restore the existing session for you. You can ask a session if it's already authenticated using the `IsAuthenticated` property.

N.B. The SDK supports switching between authentication environments to the Yinxiang Biji (Evernote China) service transparently. Please make sure your consumer key has been [activated](http://dev.evernote.com/support/) for the China service before you deploy or test with the China service.

### Adding Resources

We saw at the beginning in "Hello World!" how you'd create a new, plaintext note and upload it to the user's default notebook. Let's say you'd like to create a note with an image that you have. That's easy too. You just need to create an `ENResource` that represents the image data, and add it to the note before uploading:

    ENNote myResourceNote = new ENNote();
    myResourceNote.Title = "My test note with a resource";
    myResourceNote.Content = ENNoteContent.NoteContentWithString("Hello, resource!");
    byte[] myFile = StreamFile("<complete path and filename of a JPG file on your computer>");    // Be sure to replace this with a real JPG file
    ENResource myResource = new ENResource(myFile, "image/jpg", "My First Picture.jpg");
    myResourceNote.Resources.Add(myResource);
    ENNoteRef myResourceRef = ENSession.SharedSession.UploadNote(myResourceNote, null);

You aren't restricted to images; you can use any kind of file. Just use the appropriate initializer for `ENResource`. You'll need to know the data's MIME type to pass along.

### Creating a note using HTML or web content

The SDK contains a facility for supplying HTML or web content to a note.

    ENNote myFancyNote = new ENNote();
    myFancyNote.Title = "My first note";
    myFancyNote.Content = ENNoteContent.NoteContentWithSanitizedHTML("<p>Hello, world - <i>this</i> is a <b>fancy</b> note.</p>");
    ENNoteRef myFancyNoteRef = ENSession.SharedSession.UploadNote(myFancyNote, null);

This method handles general HTML content, including external style sheets which it will automatically inline. Please note that this is not a comprehensive "web clipper", though, and isn't designed to work fully on all arbitrary pages from the internet. It will work best on pages which have been generally designed for the purpose of being captured as note content. 

### Downloading and displaying an existing note

If you have an `ENNoteRef` object, either because you uploaded a note and kept the resulting note ref, or because you got one from a `FindNotes` operation (below), you can download the content of that note from the service like this:

    ENNote myDownloadedNote = ENSession.SharedSession.DownloadNote(myNoteRef);
	
But what can you do with a note? Well, you could change parts of the object, and reupload it to e.g. replace the existing note on the service. (See the documentation for `UploadNote`). 

### Finding notes in Evernote

The SDK provides a simplified search operation that can find notes available to the user. Use an `ENNoteSearch` to encapsulate a query. (There are a few basic search objects you can use, or create your own with anything valid in the [Evernote search grammar](https://dev.evernote.com/doc/articles/search_grammar.php)). For example, to search for the 20 most recent notes containing the word "redwood", you could use search like this:

    List<ENSessionFindNotesResult> myNotesList = ENSession.SharedSession.FindNotes(ENNoteSearch.NoteSearch("redwood"), null, ENSession.SearchScope.All, ENSession.SortOrder.RecentlyUpdated, 20);

    if (myNotesList.Count > 0)
    {
        foreach (ENSessionFindNotesResult result in myNotesList)
        {
			// Each ENSessionFindNotesResult has a noteRef along with other important metadata.
			Console.WriteLine("Found note with title: " + result.Title)
        }
    }

If you specify a notebook, the search will be limited to that notebook. If you omit the notebook, you can specify different combinations of search scope (personal, business, shared notebooks, etc), but please be aware of performance considerations. 

**Performance Warning** Doing a broadly scoped search, and/or specifying a very high number of max results against a user's account with significant content can result in slow responses and a poor user experience. If the number of results is unbounded, the client may run out of memory and be terminated if there are too many results! Business scope in particular can produce an unpredictable amount of results. Please consider your usage very carefully here. You can do paged searches, and have other low-level controls by [using the advanced API.](Working_with_the_Advanced_\(EDAM\)_API.md)

### Using the COM interface

This SDK is COM-compatible, so you can use it to integrate with Evernote in COM-based environments like Microsoft Office VBA. Because COM doesn't support use of static methods, there are a few differences when using the SDK via its COM interface.

**Reference the SDK** In order to use the SDK with COM, be sure to add a reference to the library first. For example, in a Microsoft Word or Excel Visual Basic window, from the main menu you would select **Tools** > **Add References...**, then place a checkmark next to **Evernote Cloud SDK for Windows**.

**Instantiation** Because static methods are not allowed, it's necessary to instantiate an "ENSessionCOM" object prior to calling `SetSharedSessionConsumerKey` (or `SetSharedSessionDeveloperToken`). 

    Dim EvernoteSession As New ENSessionCOM
    ' Be sure to put your own consumer key and consumer secret here.
    Call EvernoteSession.SetSharedSessionConsumerKey("your key", "your secret")

**Other Changes** For the same reason, the following objects should be instantiated in place of using their static counterparts in the regular API: `ENNoteSearchCOM` and `ENNoteContentCOM`.

### What else can I do?

Other things ENSession can do for you is enumerate all notebooks a user has access to, replace/update existing notes, search and download notes, and fetch thumbnails.

If you want to do more sophisticated work with Evernote, the primary interface that this SDK provides may not offer all of the functionality that you need. There is a lower-level API available that exposes the full breadth of the service capabilities at the expense of some learning overhead. [Have a look at this guide to advanced functionality to get started with it.](Working_with_the_Advanced_\(EDAM\)_API.md)

 

