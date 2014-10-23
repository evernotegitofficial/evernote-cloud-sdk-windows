using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Evernote.EDAM.Type;

namespace EvernoteSDK
{
	namespace Advanced
	{

		public class ENSessionAdvanced : ENSession
		{
			///**
			//*  Access the shared session object; this is the only way to get a valid ENSession.
			//*
			//*  return: The shared session object.
			//*/
			new public static ENSessionAdvanced SharedSession
			{
				get
				{
					if (ENSessionInterfaceType == InterfaceType.Basic)
					{
						var err = "Attempting to use the Advanced interface without first having called ENSessionAdvanced.SetSharedSessionConsumerKey";
						ENSDKLogger.ENSDKLogError(err);
						throw new Exception(err);
					}
					return _sharedSessionAdvanced.Value;
				}
			}

			new public static void SetSharedSessionConsumerKey(string sessionConsumerKey, string sessionConsumerSecret, string sessionHost = null)
			{
				ENSession.ENSessionInterfaceType = InterfaceType.Advanced;
				ENSession.SetSharedSessionConsumerKey(sessionConsumerKey, sessionConsumerSecret, sessionHost);
			}

			new public static void SetSharedSessionDeveloperToken(string sessionDeveloperToken, string sessionNoteStoreUrl)
			{
				ENSession.ENSessionInterfaceType = InterfaceType.Advanced;
				ENSession.SetSharedSessionDeveloperToken(sessionDeveloperToken, sessionNoteStoreUrl);
			}

			//// Indicates if your app is capable of supporting linked/business notebooks as app notebook destinations.
			//// Defaults to YES, as the non-advanced interface on ENSession will handle these transparently. If you're
			//// using the note store clients directly, either set this to NO, or be sure you test using a shared notebook as
			//// an app notebook.
			new public bool SupportsLinkedAppNotebook
			{
				get
				{
					return base.SupportsLinkedAppNotebook;
				}
				set
				{
					base.SupportsLinkedAppNotebook = value;
				}
			}

			//// Once authenticated, this flag will indicate whether the app notebook chosen by the user is, in fact, linked.
			//// (This will never be YES if you have set the flag above to NO). If so, you must take this into account:
			//// the primary note store will not allow you to access the notebook; instead, you must authenticate to the
			//// relevant linked notebook. You can find the linked notebook record by calling -listLinkedNotebooks on the
			//// primary note store.
			new public bool AppNotebookIsLinked
			{
				get
				{
					return base.AppNotebookIsLinked;
				}
			}

			//// This gives access to the preferences store that the session keeps independently from NSUserDefaults, and is
			//// destroyed when the session unauthenticates. This should generally not be used in your application, but
			//// it is used by the sample UIActivity to track recently-used notebook destinations, which are of course
			//// session-specific. If you use it, please namespace your keys appropriately to avoid collisions.
			new public ENPreferencesStore Preferences
			{
				get
				{
					return base.Preferences;
				}
			}

			// The following accessors all allow retrieval of an appropriate note store client to perform API operations with.

			///**
			// *  The primary note store client is valid for all personal notebooks, and can also be used to authenticate with
			// *  shared notebooks.
			// *
			// *  @return A client for the user's primary note store.
			// */
			new public ENNoteStoreClient PrimaryNoteStore
			{
				get
				{
					return base.PrimaryNoteStore;
				}
				set
				{
					base.PrimaryNoteStore = value;
				}
			}

			///**
			// *  The business note store client will only be non-nil if the authenticated user is a member of a business. With
			// *  it, you can access the business's notebooks.
			// *
			// *  @return A client for the user's business note store, or nil if the user is not a member of a business.
			// */
			new public ENNoteStoreClient BusinessNoteStore
			{
				get
				{
					return base.BusinessNoteStore;
				}
				set
				{
					base.BusinessNoteStore = value;
				}
			}

			///**
			// *  Every linked notebook requires its own note store client instance to access.
			// *
			// *  @param linkedNotebook A linked notebook record for which you'd like a note store client.
			// *
			// *  @return A client for the linked notebook's note store.
			// */
			new public ENNoteStoreClient NoteStoreForLinkedNotebook(LinkedNotebook linkedNotebook)
			{
				return base.NoteStoreForLinkedNotebook(linkedNotebook);
			}

			///**
			// *  Retrieves a note store client appropriate for accessing the note pointed to by the note ref.
			// *  Useful for "bridging" between the high-level and EDAM APIs.
			// *
			// *  @param noteRef A valid note ref.
			// *
			// *  @return A client for the note store that contains the note ref's note.
			// */
            new public ENNoteStoreClient NoteStoreForNoteRef(ENNoteRef noteRef)
            {
                return base.NoteStoreForNoteRef(noteRef);
            }

			///**
			// *  Retrieves a note store client appropriate for accessing a given notebook.
			// *  Useful for "bridging" between the high-level and EDAM APIs.
			// *
			// *  @param notebook A valid notebook.
			// *
			// *  @return A client for the note store that contains the notebook.
			// */
			public object NoteStoreforNotebook(ENNotebook notebook)
			{
				return base.NoteStoreForNotebook(notebook);
			}

            [ComVisible(false)]
            public string NoteHTMLContent(string noteContent, List<Resource> noteResources)
            {
                string content = null;
                try
                {
                    content = ENMLtoHTMLConverter.HTMLFromENMLContent(noteContent, noteResources);
                }
                catch (Exception)
                {
                    ENSDKLogger.ENSDKLogError("Unable to convert note content to HTML");
                }
                return content;
            }

            public string NoteHTMLContent(string noteContent, ENCollection noteResources)
            {
                string content = null;
                try
                {
                    content = ENMLtoHTMLConverter.HTMLFromENMLContent(noteContent, noteResources);
                }
                catch (Exception)
                {
                    ENSDKLogger.ENSDKLogError("Unable to convert note content to HTML");
                }
                return content;
            }

            public string NoteTextContent(string noteContent)
            {
                string content = null;
                List<Resource> edamResources = new List<Resource>();
                try
                {
                    content = ENMLtoHTMLConverter.HTMLToText(ENMLtoHTMLConverter.HTMLFromENMLContent(noteContent, edamResources));
                }
                catch (Exception)
                {
                    ENSDKLogger.ENSDKLogError("Unable to convert note content to Text");
                }
                return content;
            }

            public string EvernoteNoteLink(ENNoteRef noteRef)
            {
                    string shardId = ShardIdForNoteRef(noteRef);
                    return String.Format("evernote:///view/{0}/{1}/{2}/{2}/", UserID, shardId, noteRef.Guid);
            }

		}

		public class ENSessionAdvancedForCOM
		{
			public void SetSharedSessionConsumerKey(string sessionConsumerKey, string sessionConsumerSecret, string sessionHost = null)
			{
				ENSessionAdvanced.SetSharedSessionConsumerKey(sessionConsumerKey, sessionConsumerSecret, sessionHost);
			}

			public void SetSharedSessionDeveloperToken(string sessionDeveloperToken, string sessionNoteStoreUrl)
			{
				ENSessionAdvanced.SetSharedSessionDeveloperToken(sessionDeveloperToken, sessionNoteStoreUrl);
			}

			public ENSession SharedSession()
			{
				return ENSessionAdvanced.SharedSession;
			}
		}

		public class ENSessionFindNotesResultAdvanced : ENSessionFindNotesResult
		{
			///**
			//*  The update sequence number (USN) associated with the current version of the note find result.
			//*/
			new public object UpdateSequenceNumber
			{
				get
				{
					return base.UpdateSequenceNumber;
				}
				set
				{
					base.UpdateSequenceNumber = Convert.ToInt32(value);
				}
			}
		}

		public class ENNoteAdvanced : ENNote
		{
            public ENNoteAdvanced()
            {
            }

            public ENNoteAdvanced(Note edamNote) : base(edamNote)
            {
            }

			///**
			//*  A property indicating the "source" URL for this note. Optional, and useful mainly in contexts where the 
			//*  note is captured from web content.
			//*/
			new public string SourceUrl
			{
				get
				{
					return base.SourceUrl;
				}
				set
				{
					base.SourceUrl = value;
				}
			}

			///**
			//*  An optional dictionary of attributes which are used at upload time only to apply to an EDAMNote's attributes during
			//*  its creation. The keys in the dictionary should be valid keys in an EDAMNoteAttributes, e.g. "author", or "sourceApplication";
			//*  the values are the objects to apply. 
			//*
			//*  Note that downloaded notes do not populate this dictionary; if you need to inspect properties of an EDAMNote that aren't
			//*  represented by ENNote, you should use ENNoteStoreClient's -getNoteWithGuid... method to download the EDAMNote directly.
			//*/
			new public Dictionary<string, object> EdamAttributes
			{
				get
				{
					return base.EdamAttributes;
				}
				set
				{
					base.EdamAttributes = value;
				}
			}
		}

		public class ENNoteContentAdvanced : ENNoteContent
		{
			///**
			// *  Class method to create note content directly from a string of valid ENML.
			// *
			// *  @param enml A valid ENML string. (Invalid ENML will fail at upload time.)
			// *
			// *  @return A note content object.
			// */
			new public static object NoteContentWithENML(string enml)
			{
				return ENNoteContent.NoteContentWithENML(enml);
			}

			///**
			// *  The designated initializer for this class; initializes note content with a string of valid ENML.
			// *
			// *  @param enml A valid ENML string. (Invalid ENML will fail at upload time.)
			// *
			// *  @return A note content object.
			// */
			public ENNoteContentAdvanced(string enml) : base(enml)
			{
			}

			///**
			// *  Return the content of the receiver in ENML format. For content created with ENML to begin with, this
			// *  will simply return that ENML. For content created with other input means, the content will be transformed
			// *  to ENML.
			// *
			// *  @return A note content object.
			// */
			//- (NSString *)enml;
			new public string Enml()
			{
				return base.Enml();
			}
		}

		public class ENResourceAdvanced : ENResource
		{
			///**
			// *  A property indicating the "source" URL for this resource. Optional, and useful mainly in contexts where the
			// *  resource is captured from web content.
			// */
			new public string SourceUrl
			{
				get
				{
					return base.SourceUrl;
				}
				set
				{
					base.SourceUrl = value;
				}
			}

			///**
			// *  Accessor for the MD5 hash of the data of a resource. This is useful when writing ENML.
			// *
			// *  @return The hash for this resource.
			// */
			new public byte[] DataHash
			{
				get
				{
					return base.DataHash;
				}
				set
				{
					base.DataHash = value;
				}
			}
		}

		public class ENNoteRefAdvanced : ENNoteRef
		{
			///**
			// *  The Evernote service guid for the note that this note ref points to. Valid only with a note store client
			// *  that also corresponds to this note ref; see ENSession to retrieve an appropriate note store client.
			// */
			new public string Guid
			{
				get
				{
					return base.Guid;
				}
				set
				{
					base.Guid = value;
				}
			}
		}

		public class ENNotebookAdvanced : ENNotebook
		{
			///**
			//*  The Evernote service guid for the note that this notebook corresponds to. Valid only with a note store client
			//*  that also corresponds to this notebook; see ENSession to retrieve an appropriate note store client.
			//*/
			//
			new public string Guid
			{
				get
				{
					return base.Guid;
				}
			}
        }
	}

}