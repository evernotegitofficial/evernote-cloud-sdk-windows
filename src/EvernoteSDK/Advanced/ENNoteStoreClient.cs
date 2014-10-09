using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Type;
using Evernote.EDAM.UserStore;
using Thrift.Protocol;
using Thrift.Transport;

namespace EvernoteSDK
{
	namespace Advanced
	{
		public class ENNoteStoreClient
		{

			//         ! DO NOT INSTANTIATE THIS OBJECT DIRECTLY. GET ONE FROM AN AUTHENTICATED ENSESSION !

			private const int FIND_NOTES_DEFAULT_MAX_NOTES = 100;

			internal static ENNoteStoreClient NoteStoreClient(string url, string authenticationToken)
			{
				ENNoteStoreClient enClient = new ENNoteStoreClient();
				enClient.CachedNoteStoreUrl = url;
				enClient.CachedAuthenticationToken = authenticationToken;
				return enClient;
			}

			private NoteStore.Client _client;
			private NoteStore.Client Client
			{
				get
				{
					if (_client == null)
					{
						Uri url = new Uri(NoteStoreUrl());
						THttpClient transport = new THttpClient(url);
						TBinaryProtocol protocol = new TBinaryProtocol(transport);
						_client = new NoteStore.Client(protocol, protocol);
					}
					return _client;
				}
				set
				{
					_client = value;
				}
			}

			private string CachedNoteStoreUrl {get; set;}
			private string CachedAuthenticationToken {get; set;}

#region Override points for subclasses

			// Override points for subclasses that handle auth differently. This simple version just
			// returns the cached token and cached url

			protected internal virtual string AuthenticationToken()
			{
				return CachedAuthenticationToken;
			}

			protected internal virtual string NoteStoreUrl()
			{
				return CachedNoteStoreUrl;
			}

#endregion

			/////---------------------------------------------------------------------------------------
			///// @name NoteStore sync methods
			/////---------------------------------------------------------------------------------------

			///** Asks the NoteStore to provide information about the status of the user account corresponding to the provided authentication token.
			// */
			public SyncState GetSyncState()
			{
				return Client.getSyncState(AuthenticationToken());
			}

			///** Asks the NoteStore to provide the state of the account in order of last modification.
			// This request retrieves one block of the server's state so that a client can make several small requests against a large account rather than getting the entire state in one big message.
			// @param  afterUSN The client can pass this value to ask only for objects that have been updated after a certain point. This allows the client to receive updates after its last checkpoint rather than doing a full synchronization on every pass. The default value of "0" indicates that the client wants to get objects from the start of the account.
			// @param  maxEntries The maximum number of modified objects that should be returned in the result SyncChunk. This can be used to limit the size of each individual message to be friendly for network transfer. Applications should not request more than 256 objects at a time, and must handle the case where the service returns less than the requested number of objects in a given request even though more objects are available on the service.
			// @param  fullSyncOnly If true, then the client only wants initial data for a full sync. In this case, the service will not return any expunged objects, and will not return any Resources, since these are also provided in their corresponding Notes.
			// */
			public SyncChunk GetSyncChunk(int afterUSN, int maxEntries, bool fullSyncOnly)
			{
				return Client.getSyncChunk(AuthenticationToken(), afterUSN, maxEntries, fullSyncOnly);
			}

			///** Asks the NoteStore to provide the state of the account in order of last modification.
			// This request retrieves one block of the server's state so that a client can make several small requests against a large account rather than getting the entire state in one big message. This call gives more fine-grained control of the data that will be received by a client by omitting data elements that a client doesn't need. This may reduce network traffic and sync times.
			// @param  afterUSN The client can pass this value to ask only for objects that have been updated after a certain point. This allows the client to receive updates after its last checkpoint rather than doing a full synchronization on every pass. The default value of "0" indicates that the client wants to get objects from the start of the account.
			// @param  maxEntries The maximum number of modified objects that should be returned in the result SyncChunk. This can be used to limit the size of each individual message to be friendly for network transfer.
			// @param  filter The caller must set some of the flags in this structure to specify which data types should be returned during the synchronization. See the SyncChunkFilter structure for information on each flag.
			// */
			public SyncChunk GetFilteredSyncChunk(int afterUSN, int maxEntries, SyncChunkFilter filter)
			{
				return Client.getFilteredSyncChunk(AuthenticationToken(), afterUSN, maxEntries, filter);
			}

			///** Asks the NoteStore to provide information about the status of a linked notebook that has been shared with the caller, or that is public to the world.
			// This will return a result that is similar to getSyncState, but may omit SyncState.uploaded if the caller doesn't have permission to write to the linked notebook.
			// This function must be called on the shard that owns the referenced notebook. (I.e. the shardId in /shard/shardId/edam/note must be the same as LinkedNotebook.shardId.)
			// @param  linkedNotebook This structure should contain identifying information and permissions to access the notebook in question.
			// */
			public SyncState GetLinkedNotebookSyncState(LinkedNotebook linkedNotebook)
			{
				return Client.getLinkedNotebookSyncState(AuthenticationToken(), linkedNotebook);
			}

			///** Asks the NoteStore to provide information about the contents of a linked notebook that has been shared with the caller, or that is public to the world.
			// This will return a result that is similar to getSyncChunk, but will only contain entries that are visible to the caller. I.e. only that particular Notebook will be visible, along with its Notes, and Tags on those Notes.
			// This function must be called on the shard that owns the referenced notebook. (I.e. the shardId in /shard/shardId/edam/note must be the same as LinkedNotebook.shardId.)
			// @param  linkedNotebook This structure should contain identifying information and permissions to access the notebook in question. This must contain the valid fields for either a shared notebook (e.g. shareKey) or a public notebook (e.g. username, uri)
			// @param  afterUSN The client can pass this value to ask only for objects that have been updated after a certain point. This allows the client to receive updates after its last checkpoint rather than doing a full synchronization on every pass. The default value of "0" indicates that the client wants to get objects from the start of the account.
			// @param  maxEntries The maximum number of modified objects that should be returned in the result SyncChunk. This can be used to limit the size of each individual message to be friendly for network transfer. Applications should not request more than 256 objects at a time, and must handle the case where the service returns less than the requested number of objects in a given request even though more objects are available on the service.
			// @param  fullSyncOnly If true, then the client only wants initial data for a full sync. In this case, the service will not return any expunged objects, and will not return any Resources, since these are also provided in their corresponding Notes.
			// */
			public object GetLinkedNotebookSyncChunk(LinkedNotebook linkedNotebook, int afterUSN, int maxEntries, bool fullSyncOnly)
			{
				return Client.getLinkedNotebookSyncChunk(AuthenticationToken(), linkedNotebook, afterUSN, maxEntries, fullSyncOnly);
			}

			/////---------------------------------------------------------------------------------------
			///// @name NoteStore notebook methods
			/////---------------------------------------------------------------------------------------

			///** Returns a list of all of the notebooks in the account.
			// */
			public List<Notebook> ListNotebooks()
			{
				return Client.listNotebooks(AuthenticationToken());
			}

			///** Returns the current state of the notebook with the provided GUID. The notebook may be active or deleted (but not expunged).
			// @param  guid The GUID of the notebook to be retrieved.
			// */
			public Notebook GetNotebook(string guid)
			{
				return Client.getNotebook(AuthenticationToken(), guid);
			}

			///** Returns the notebook that should be used to store new notes in the user's account when no other notebooks are specified.
			// */
			public Notebook GetDefaultNotebook()
			{
				return Client.getDefaultNotebook(AuthenticationToken());
			}

			///** Asks the service to make a notebook with the provided name.
			// @param  notebook The desired fields for the notebook must be provided on this object. The name of the notebook must be set, and either the 'active' or 'defaultNotebook' fields may be set by the client at creation. If a notebook exists in the account with the same name (via case-insensitive compare), this will throw an EDAMUserException.
			// */
			public Notebook CreateNotebook(Notebook notebook)
			{
				return Client.createNotebook(AuthenticationToken(), notebook);
			}

			///** Submits notebook changes to the service. The provided data must include the notebook's guid field for identification.
			// @param  notebook The notebook object containing the requested changes.
			// */
			public int UpdateNotebook(Notebook notebook)
			{
				return Client.updateNotebook(AuthenticationToken(), notebook);
			}

			///** Permanently removes the notebook from the user's account. After this action, the notebook is no longer available for undeletion, etc. If the notebook contains any Notes, they will be moved to the current default notebook and moved into the trash (i.e. Note.active=false).
			// @param  guid The GUID of the notebook to delete.
			// */
			public int ExpungeNotebook(string guid)
			{
				return Client.expungeNotebook(AuthenticationToken(), guid);
			}

			/////---------------------------------------------------------------------------------------
			///// @name NoteStore tag methods
			/////---------------------------------------------------------------------------------------

			///** Returns a list of the tags in the account. Evernote does not support the undeletion of tags, so this will only include active tags.
			// */
            [ComVisible(false)]
            public List<Tag> ListTags()
			{
				return Client.listTags(AuthenticationToken());
			}

            public ENCollection ListTagsForCOM()
            {
                List<Tag> tags = Client.listTags(AuthenticationToken());

                ENCollection comResults = new ENCollection();
                foreach (Tag tag in tags)
                {
                    EdamTag edTag = new EdamTag();
                    edTag.Guid = tag.Guid;
                    edTag.Name = tag.Name;
                    edTag.ParentGuid = tag.ParentGuid;
                    edTag.UpdateSequenceNum = tag.UpdateSequenceNum;
                    object tempvar = edTag;
                    object tempkey = tag.Guid;
                    comResults.Add(ref tempvar, ref tempkey);
                }
                return comResults;
            }

			///** Returns a list of the tags that are applied to at least one note within the provided notebook. If the notebook is public, the authenticationToken may be ignored.
			// @param  guid the GUID of the notebook to use to find tags
			// */
			public List<Tag> ListTagsByNotebook(string guid)
			{
				return Client.listTagsByNotebook(AuthenticationToken(), guid);
			}

			///** Returns the current state of the Tag with the provided GUID.
			// @param  guid The GUID of the tag to be retrieved.
			// */
			public Tag GetTag(string guid)
			{
				return Client.getTag(AuthenticationToken(), guid);
			}

			///** Asks the service to make a tag with a set of information.
			// @param  tag The desired list of fields for the tag are specified in this object. The caller must specify the tag name, and may provide the parentGUID.
			// */
			public Tag CreateTag(Tag tag)
			{
				return Client.createTag(AuthenticationToken(), tag);
			}

			///** Submits tag changes to the service. The provided data must include the tag's guid field for identification. The service will apply updates to the following tag fields: name, parentGuid
			// @param  tag The tag object containing the requested changes.
			// */
			public int UpdateTag(Tag tag)
			{
				return Client.updateTag(AuthenticationToken(), tag);
			}

			///** Removes the provided tag from every note that is currently tagged with this tag. If this operation is successful, the tag will still be in the account, but it will not be tagged on any notes.
			// This function is not indended for use by full synchronizing clients, since it does not provide enough result information to the client to reconcile the local state without performing a follow-up sync from the service. This is intended for "thin clients" that need to efficiently support this as a UI operation.
			// @param  guid The GUID of the tag to remove from all notes.
			// */
			public void UntagAll(string guid)
			{
				Client.untagAll(AuthenticationToken(), guid);
			}

			///** Permanently deletes the tag with the provided GUID, if present.
			// NOTE: This function is not available to third party applications. Calls will result in an EDAMUserException with the error code PERMISSION_DENIED.
			// @param  guid The GUID of the tag to delete.
			// */
			public int ExpungeTag(string guid)
			{
				return Client.expungeTag(AuthenticationToken(), guid);
			}

			/////---------------------------------------------------------------------------------------
			///// @name NoteStore SavedSearch methods
			/////---------------------------------------------------------------------------------------

			///** Returns a list of the searches in the account. Evernote does not support the undeletion of searches, so this will only include active searches.
			// */
			public List<SavedSearch> ListSearches()
			{
				return Client.listSearches(AuthenticationToken());
			}

			///** Returns the current state of the search with the provided GUID.
			// @param  guid The GUID of the search to be retrieved.
			// */
			public SavedSearch GetSearch(string guid)
			{
				return Client.getSearch(AuthenticationToken(), guid);
			}

			///** Asks the service to make a saved search with a set of information.
			// @param  search The desired list of fields for the search are specified in this object. The caller must specify the name, query, and format of the search.
			// */
			public SavedSearch CreateSearch(SavedSearch search)
			{
				return Client.createSearch(AuthenticationToken(), search);
			}

			///** Submits search changes to the service. The provided data must include the search's guid field for identification. The service will apply updates to the following search fields: name, query, and format.
			// @param  search The search object containing the requested changes.
			// */
			public int UpdateSearch(SavedSearch search)
			{
				return Client.updateSearch(AuthenticationToken(), search);
			}

			///** Permanently deletes the saved search with the provided GUID, if present.
			// NOTE: This function is not available to third party applications. Calls will result in an EDAMUserException with the error code PERMISSION_DENIED.
			// @param  guid The GUID of the search to delete.
			// */
			public int ExpungeSearch(string guid)
			{
				return Client.expungeSearch(AuthenticationToken(), guid);
			}

			/////---------------------------------------------------------------------------------------
			///// @name NoteStore notes methods
			/////---------------------------------------------------------------------------------------

			///** Identify related entities on the service, such as notes, notebooks, and tags related to notes or content.

			// @param  query The information about which we are finding related entities.
			// @param  resultSpec Allows the client to indicate the type and quantity of information to be returned, allowing a saving of time and bandwidth.
			// */
			public RelatedResult FindRelated(RelatedQuery query, RelatedResultSpec resultSpec)
			{
				return Client.findRelated(AuthenticationToken(), query, resultSpec);
			}

			///** Used to find a set of the notes from a user's account based on various criteria specified via a NoteFilter object.
			// The Notes (and any embedded Resources) will have empty Data bodies for contents, resource data, and resource recognition fields. These values must be retrieved individually.
			// @param  filter The list of criteria that will constrain the notes to be returned.
			// @param  offset The numeric index of the first note to show within the sorted results. The numbering scheme starts with "0". This can be used for pagination.
			// @param  maxNotes The most notes to return in this query. The service will return a set of notes that is no larger than this number, but may return fewer notes if needed. The NoteList.totalNotes field in the return value will indicate whether there are more values available after the returned set.
			// */
			public NoteList FindNotes(NoteFilter filter, int offset, int maxNotes)
			{
				return Client.findNotes(AuthenticationToken(), filter, offset, maxNotes);
			}

			///** Finds the position of a note within a sorted subset of all of the user's notes.
			// This may be useful for thin clients that are displaying a paginated listing of a large account, which need to know where a particular note sits in the list without retrieving all notes first.
			// @param  filter The list of criteria that will constrain the notes to be returned.
			// @param  guid The GUID of the note to be retrieved.
			// */
			public int FindNoteOffset(NoteFilter filter, string guid)
			{
				return Client.findNoteOffset(AuthenticationToken(), filter, guid);
			}

			///** Used to find the high-level information about a set of the notes from a user's account based on various criteria specified via a NoteFilter object.
			// This should be used instead of 'findNotes' whenever the client doesn't really need all of the deep structure of every Note and Resource, but just wants a high-level list of information. This will save time and bandwidth.
			// @param  filter The list of criteria that will constrain the notes to be returned.
			// @param  offset The numeric index of the first note to show within the sorted results. The numbering scheme starts with "0". This can be used for pagination.
			// @param  maxNotes The mximum notes to return in this query. The service will return a set of notes that is no larger than this number, but may return fewer notes if needed. The NoteList.totalNotes field in the return value will indicate whether there are more values available after the returned set.
			// @param  resultSpec This specifies which information should be returned for each matching Note. The fields on this structure can be used to eliminate data that the client doesn't need, which will reduce the time and bandwidth to receive and process the reply.
			// */
			public NotesMetadataList FindNotesMetadata(NoteFilter filter, int offset, int maxNotes, NotesMetadataResultSpec resultSpec)
			{
				return Client.findNotesMetadata(AuthenticationToken(), filter, offset, maxNotes, resultSpec);
			}

			///** This function is used to determine how many notes are found for each notebook and tag in the user's account, given a current set of filter parameters that determine the current selection.
			// This function will return a structure that gives the note count for each notebook and tag that has at least one note under the requested filter. Any notebook or tag that has zero notes in the filtered set will not be listed in the reply to this function (so they can be assumed to be 0).
			// @param  filter The note selection filter that is currently being applied. The note counts are to be calculated with this filter applied to the total set of notes in the user's account.
			// @param  withTrash If true, then the NoteCollectionCounts.trashCount will be calculated and supplied in the reply. Otherwise, the trash value will be omitted.
			// */
			public NoteCollectionCounts FindNotesCount(NoteFilter filter, bool withTrash)
			{
				return Client.findNoteCounts(AuthenticationToken(), filter, withTrash);
			}

			///** Returns the current state of the note in the service with the provided GUID. The ENML contents of the note will only be provided if the 'withContent' parameter is true.
			// The service will include the meta-data for each resource in the note, but the binary contents of the resources and their recognition data will be omitted. If the Note is found in a public notebook, the authenticationToken will be ignored (so it could be an empty string). The applicationData fields are returned as keysOnly.
			// @param  guid The GUID of the note to be retrieved.
			// @param  withContent If true, the note will include the ENML contents of its 'content' field.
			// @param  withResourcesData If true, any Resource elements in this Note will include the binary contents of their 'data' field's body.
			// @param  withResourcesRecognition If true, any Resource elements will include the binary contents of the 'recognition' field's body if recognition data is present.
			// @param  withResourcesAlternateData If true, any Resource elements in this Note will include the binary contents of their 'alternateData' fields' body, if an alternate form is present.
			// */
            [ComVisible(false)]
            public Note GetNote(string guid, bool withContent, bool withResourcesData, bool withResourcesRecognition, bool withResourcesAlternateData)
			{
				return Client.getNote(AuthenticationToken(), guid, withContent, withResourcesData, withResourcesRecognition, withResourcesAlternateData);
			}

            public EdamNote GetNoteForCOM(string guid, bool withContent, bool withResourcesData, bool withResourcesRecognition, bool withResourcesAlternateData)
            {
                Note note = Client.getNote(AuthenticationToken(), guid, withContent, withResourcesData, withResourcesRecognition, withResourcesAlternateData);
                EdamNote edamNote = new EdamNote();
                edamNote.Guid = note.Guid;
                edamNote.Title = note.Title;
                edamNote.Content = note.Content;
                edamNote.ContentHash = note.ContentHash;
                edamNote.ContentLength = note.ContentLength;
                edamNote.Created = note.Created;
                edamNote.Updated = note.Updated;
                edamNote.Deleted = note.Deleted;
                edamNote.Active = note.Active;
                edamNote.UpdateSequenceNum = note.UpdateSequenceNum;
                edamNote.NotebookGuid = note.NotebookGuid;
                edamNote.TagGuidsList = note.TagGuids;
                edamNote.ResourcesList = note.Resources;
                edamNote.Attributes = note.Attributes;
                edamNote.TagNamesList = note.TagNames;
                return edamNote;            
            }

			///** Get all of the application data for the note identified by GUID, with values returned within the LazyMap fullMap field.
			// If there are no applicationData entries, then a LazyMap with an empty fullMap will be returned. If your application only needs to fetch its own applicationData entry, use getNoteApplicationDataEntry instead.
			// @param  guid The GUID of the note who's application data is to be retrieved.
			// */
			public LazyMap GetNoteApplicationData(string guid)
			{
				return Client.getNoteApplicationData(AuthenticationToken(), guid);
			}

			///** Get the value of a single entry in the applicationData map for the note identified by GUID.
			// @param  guid The GUID of the note
			// @param key The key in the dictionary
			// */
			public string GetNoteApplicationDataEntry(string guid, string key)
			{
				return Client.getNoteApplicationDataEntry(AuthenticationToken(), guid, key);
			}

			///** Update, or create, an entry in the applicationData map for the note identified by guid.
			// @param  guid The GUID of the note
			// @param key The key in the dictionary
			// @param value The value in the dictionary
			// */
			public int SetNoteApplicationDataEntry(string guid, string key, string value)
			{
				return Client.setNoteApplicationDataEntry(AuthenticationToken(), guid, key, value);
			}

			///** Remove an entry identified by 'key' from the applicationData map for the note identified by 'guid'. Silently ignores an unset of a non-existing key.
			// @param  guid The GUID of the note
			// @param key key from applicationData map
			// */
			public int UnsetNoteApplicationDataEntry(string guid, string key)
			{
				return Client.unsetNoteApplicationDataEntry(AuthenticationToken(), guid, key);
			}

			///** Returns XHTML contents of the note with the provided GUID. If the Note is found in a public notebook, the authenticationToken will be ignored (so it could be an empty string).

			// @param  guid The GUID of the note to be retrieved.
			// @param success Success completion block.
			// @param failure Failure completion block.
			// */
			public string GetNoteContent(string guid)
			{
				return Client.getNoteContent(AuthenticationToken(), guid);
			}

			///** Returns a block of the extracted plain text contents of the note with the provided GUID.
			// This text can be indexed for search purposes by a light client that doesn't have capabilities to extract all of the searchable text content from the note and its resources. If the Note is found in a public notebook, the authenticationToken will be ignored (so it could be an empty string).
			// @param  guid The GUID of the note to be retrieved.
			// @param  noteOnly If true, this will only return the text extracted from the ENML contents of the note itself. If false, this will also include the extracted text from any text-bearing resources (PDF, recognized images)
			// @param  tokenizeForIndexing If true, this will break the text into cleanly separated and sanitized tokens. If false, this will return the more raw text extraction, with its original punctuation, capitalization, spacing, etc. 
			// @param success Success completion block.
			// */
			public string GetNoteSearchText(string guid, bool noteOnly, bool tokenizeForIndexing)
			{
				return Client.getNoteSearchText(AuthenticationToken(), guid, noteOnly, tokenizeForIndexing);
			}

			///** Returns a block of the extracted plain text contents of the resource with the provided GUID.
			// This text can be indexed for search purposes by a light client that doesn't have capability to extract all of the searchable text content from a resource. If the Resource is found in a public notebook, the authenticationToken will be ignored (so it could be an empty string).
			// @param  guid The GUID of the resource to be retrieved.
			// */
			public string GetResourceSearchText(string guid)
			{
				return Client.getResourceSearchText(AuthenticationToken(), guid);
			}

			///** Returns a list of the names of the tags for the note with the provided guid.
			// This can be used with authentication to get the tags for a user's own note, or can be used without valid authentication to retrieve the names of the tags for a note in a public notebook.
			// @param  guid The GUID of the note.
			// */
			public List<string> GetNoteTagNames(string guid)
			{
				return Client.getNoteTagNames(AuthenticationToken(), guid);
			}

			///** Asks the service to make a note with the provided set of information.
			// @param  note A Note object containing the desired fields to be populated on the service.
			// @exception EDAMUserException Thrown if the note is not valid.
			// @exception EDAMNotFoundException If the note is not found by GUID
			// */
			public Note CreateNote(Note note)
			{
				return Client.createNote(AuthenticationToken(), note);
			}

			///** Submit a set of changes to a note to the service.
			// The provided data must include the note's guid field for identification. The note's title must also be set.
			// @param  note A Note object containing the desired fields to be populated on the service. With the exception of the note's title and guid, fields that are not being changed do not need to be set. If the content is not being modified, note.content should be left unset. If the list of resources is not being modified, note.resources should be left unset.
			// */
			public Note UpdateNote(Note note)
			{
				return Client.updateNote(AuthenticationToken(), note);
			}

			///** Moves the note into the trash. The note may still be undeleted, unless it is expunged.
			// This is equivalent to calling updateNote() after setting Note.active = false
			// @param  guid The GUID of the note to delete.
			// */
			public int DeleteNote(string guid)
			{
				return Client.deleteNote(AuthenticationToken(), guid);
			}

			///** Permanently removes a Note, and all of its Resources, from the service.
			// NOTE: This function is not available to third party applications. Calls will result in an EDAMUserException with the error code PERMISSION_DENIED.
			// @param  guid The GUID of the note to delete.
			// */
			public int ExpungeNote(string guid)
			{
				return Client.expungeNote(AuthenticationToken(), guid);
			}

			///** Permanently removes a list of Notes, and all of their Resources, from the service.
			// This should be invoked with a small number of Note GUIDs (e.g. 100 or less) on each call. To expunge a larger number of notes, call this method multiple times. This should also be used to reduce the number of Notes in a notebook before calling expungeNotebook() or in the trash before calling expungeInactiveNotes(), since these calls may be prohibitively slow if there are more than a few hundred notes. If an exception is thrown for any of the GUIDs, then none of the notes will be deleted. I.e. this call can be treated as an atomic transaction.
			// NOTE: This function is not available to third party applications. Calls will result in an EDAMUserException with the error code PERMISSION_DENIED.
			// @param  guids The list of GUIDs for the Notes to remove.
			// */
			public int ExpungeNotes(List<string> guids)
			{
				return Client.expungeNotes(AuthenticationToken(), guids);
			}

			///** Permanently removes all of the Notes that are currently marked as inactive.
			// This is equivalent to "emptying the trash", and these Notes will be gone permanently. This operation may be relatively slow if the account contains a large number of inactive Notes.
			// NOTE: This function is not available to third party applications. Calls will result in an EDAMUserException with the error code PERMISSION_DENIED.
			// */
			public int ExpungeInactiveNotes()
			{
				return Client.expungeInactiveNotes(AuthenticationToken());
			}

			///** Performs a deep copy of the Note with the provided GUID 'noteGuid' into the Notebook with the provided GUID 'toNotebookGuid'.
			// The caller must be the owner of both the Note and the Notebook. This creates a new Note in the destination Notebook with new content and Resources that match all of the content and Resources from the original Note, but with new GUID identifiers. The original Note is not modified by this operation. The copied note is considered as an "upload" for the purpose of upload transfer limit calculation, so its size is added to the upload count for the owner.
			// @param  guid The GUID of the Note to copy.
			// @param  toNotebookGuid The GUID of the Notebook that should receive the new Note.
			// */
			public Note CopyNote(string guid, string toNotebookGuid)
			{
				return Client.copyNote(AuthenticationToken(), guid, toNotebookGuid);
			}

			///** Returns a list of the prior versions of a particular note that are saved within the service.
			// These prior versions are stored to provide a recovery from unintentional removal of content from a note. The identifiers that are returned by this call can be used with getNoteVersion to retrieve the previous note. The identifiers will be listed from the most recent versions to the oldest.
			// @param  guid The GUID of the Note.
			// */
			public List<NoteVersionId> ListNoteVersions(string guid)
			{
				return Client.listNoteVersions(AuthenticationToken(), guid);
			}

			///** This can be used to retrieve a previous version of a Note after it has been updated within the service.
			// The caller must identify the note (via its guid) and the version (via the updateSequenceNumber of that version). to find a listing of the stored version USNs for a note, call listNoteVersions. This call is only available for notes in Premium accounts. (I.e. access to past versions of Notes is a Premium-only feature.)
			// @param  guid The GUID of the note to be retrieved.
			// @param  updateSequenceNum The USN of the version of the note that is being retrieved
			// @param  withResourcesData If true, any Resource elements in this Note will include the binary contents of their 'data' field's body.
			// @param  withResourcesRecognition If true, any Resource elements will include the binary contents of the 'recognition' field's body if recognition data is present.
			// @param  withResourcesAlternateData If true, any Resource elements in this Note will include the binary contents of their 'alternateData' fields' body, if an alternate form is present.
			// */
			public Note GetNoteVersion(string guid, int updateSequenceNum, bool withResourcesData, bool withResourcesRecognition, bool withResourcesAlternateData)
			{
				return Client.getNoteVersion(AuthenticationToken(), guid, updateSequenceNum, withResourcesData, withResourcesRecognition, withResourcesAlternateData);
			}

			/////---------------------------------------------------------------------------------------
			///// @name NoteStore resource methods
			/////---------------------------------------------------------------------------------------

			///** Returns the current state of the resource in the service with the provided GUID.
			// If the Resource is found in a public notebook, the authenticationToken will be ignored (so it could be an empty string). Only the keys for the applicationData will be returned.
			// @param  guid The GUID of the resource to be retrieved.
			// @param  withData If true, the Resource will include the binary contents of the 'data' field's body.
			// @param  withRecognition If true, the Resource will include the binary contents of the 'recognition' field's body if recognition data is present.
			// @param  withAttributes If true, the Resource will include the attributes
			// @param  withAlternateData If true, the Resource will include the binary contents of the 'alternateData' field's body, if an alternate form is present.
			// */
			public Resource GetResource(string guid, bool withData, bool withRecognition, bool withAttributes, bool withAlternateData)
			{
				return Client.getResource(AuthenticationToken(), guid, withData, withRecognition, withAttributes, withAlternateData);
			}

			///** Get all of the application data for the Resource identified by GUID, with values returned within the LazyMap fullMap field. If there are no applicationData entries, then a LazyMap with an empty fullMap will be returned. If your application only needs to fetch its own applicationData entry, use getResourceApplicationDataEntry instead.
			// @param  guid The GUID of the Resource.
			// */
			public LazyMap GetResourceApplicationData(string guid)
			{
				return Client.getResourceApplicationData(AuthenticationToken(), guid);
			}

			///** Get the value of a single entry in the applicationData map for the Resource identified by GUID.
			// @param  guid The GUID of the Resource.
			// @param key key in the dictionary
			// */
			public string GetResourceApplicationDataEntry(string guid, string key)
			{
				return Client.getResourceApplicationDataEntry(AuthenticationToken(), guid, key);
			}

			///** Update, or create, an entry in the applicationData map for the Resource identified by guid.
			// @param  guid The GUID of the Resource.
			// @param key key in the dictionary
			// @param value value in the dictionary
			// */
			public int SetResourceApplicationDataEntry(string guid, string key, string value)
			{
				return Client.setResourceApplicationDataEntry(AuthenticationToken(), guid, key, value);
			}

			///** Remove an entry identified by 'key' from the applicationData map for the Resource identified by 'guid'.
			// @param  guid The GUID of the Resource.
			// @param key key in the dictionary
			// */
			//- (void)unsetResourceApplicationDataEntryWithGuid:(EDAMGuid)guid
			//                                              key:(NSString *)key
			//                                          success:(void(^)(int32_t usn))success
			//                                          failure:(void(^)(NSError *error))failure;
			public int UnsetResourceApplicationDataEntry(string guid, string key)
			{
				return Client.unsetResourceApplicationDataEntry(AuthenticationToken(), guid, key);
			}

			///** Submit a set of changes to a resource to the service.
			// This can be used to update the meta-data about the resource, but cannot be used to change the binary contents of the resource (including the length and hash). These cannot be changed directly without creating a new resource and removing the old one via updateNote.
			// @param  resource A Resource object containing the desired fields to be populated on the service. The service will attempt to update the resource with the following fields from the client: guid(must be provided to identify the resource),mime,width,height,duration,attributes(optional. if present, the set of attributes will be replaced).
			// */
			public int UpdateResource(Resource resource)
			{
				return Client.updateResource(AuthenticationToken(), resource);
			}

			///** Returns binary data of the resource with the provided GUID.
			// For example, if this were an image resource, this would contain the raw bits of the image. If the Resource is found in a public notebook, the authenticationToken will be ignored (so it could be an empty string).
			// @param  guid The GUID of the resource to be retrieved.
			// */
			public byte[] GetResourceData(string guid)
			{
				return Client.getResourceData(AuthenticationToken(), guid);
			}

			///** Returns the current state of a resource, referenced by containing note GUID and resource content hash.
			// @param  guid The GUID of the note that holds the resource to be retrieved.
			// @param  contentHash The MD5 checksum of the resource within that note. Note that this is the binary checksum, for example from Resource.data.bodyHash, and not the hex-encoded checksum that is used within an en-media tag in a note body.
			// @param  withData If true, the Resource will include the binary contents of the 'data' field's body.
			// @param  withRecognition If true, the Resource will include the binary contents of the 'recognition' field's body.
			// @param  withAlternateData If true, the Resource will include the binary contents of the 'alternateData' field's body, if an alternate form is present.
			// */
			public Resource GetResourceByHash(string guid, byte[] contentHash, bool withData, bool withRecognition, bool withAlternateData)
			{
				return Client.getResourceByHash(AuthenticationToken(), guid, contentHash, withData, withRecognition, withAlternateData);
			}

			///** Returns the binary contents of the recognition index for the resource with the provided GUID.
			// If the caller asks about a resource that has no recognition data, this will throw EDAMNotFoundException. If the Resource is found in a public notebook, the authenticationToken will be ignored (so it could be an empty string).
			// @param  guid The GUID of the resource whose recognition data should be retrieved.
			// */
			public byte[] GetResourceRecognition(string guid)
			{
				return Client.getResourceRecognition(AuthenticationToken(), guid);
			}

			///** If the Resource with the provided GUID has an alternate data representation (indicated via the Resource.alternateData field), then this request can be used to retrieve the binary contents of that alternate data file. If the caller asks about a resource that has no alternate data form, this will throw EDAMNotFoundException.
			// @param  guid The GUID of the resource whose recognition data should be retrieved.
			// */
			public byte[] GetResourceAlternateData(string guid)
			{
				return Client.getResourceAlternateData(AuthenticationToken(), guid);
			}

			///** Returns the set of attributes for the Resource with the provided GUID. If the Resource is found in a public notebook, the authenticationToken will be ignored (so it could be an empty string).
			// @param  guid The GUID of the resource whose attributes should be retrieved.
			// */
			public ResourceAttributes GetResourceAttributes(string guid)
			{
				return Client.getResourceAttributes(AuthenticationToken(), guid);
			}


			/////---------------------------------------------------------------------------------------
			///// @name NoteStore shared notebook methods
			/////---------------------------------------------------------------------------------------

			///** Looks for a user account with the provided userId on this NoteStore shard and determines whether that account contains a public notebook with the given URI.
			// If the account is not found, or no public notebook exists with this URI, this will throw an EDAMNotFoundException, otherwise this will return the information for that Notebook. If a notebook is visible on the web with a full URL like http://www.evernote.com/pub/sethdemo/api Then 'sethdemo' is the username that can be used to look up the userId, and 'api' is the publicUri.
			// @param  userId The numeric identifier for the user who owns the public notebook. To find this value based on a username string, you can invoke UserStore.getPublicUserInfo
			// @param  publicUri The uri string for the public notebook, from Notebook.publishing.uri.
			// */
			public Notebook GetPublicNotebook(int userID, string publicUri)
			{
				return Client.getPublicNotebook(userID, publicUri);
			}

			///** Used to construct a shared notebook object. The constructed notebook will contain a "share key" which serve as a unique identifer and access token for a user to access the notebook of the shared notebook owner.
			// @param  sharedNotebook An shared notebook object populated with the email address of the share recipient, the notebook guid and the access permissions. All other attributes of the shared object are ignored.
			// */
			public SharedNotebook CreateSharedNotebook(SharedNotebook sharedNotebook)
			{
				return Client.createSharedNotebook(AuthenticationToken(), sharedNotebook);
			}

			///** Send a reminder message to some or all of the email addresses that a notebook has been shared with.
			// The message includes the current link to view the notebook.
			// @param  guid The guid of the shared notebook
			// @param  messageText User provided text to include in the email
			// @param  recipients The email addresses of the recipients. If this list is empty then all of the users that the notebook has been shared with are emailed. If an email address doesn't correspond to share invite members then that address is ignored.
			// */
			public int SendMessageToSharedNotebookMembers(string guid, string messageText, List<string> recipients)
			{
				return Client.sendMessageToSharedNotebookMembers(AuthenticationToken(), guid, messageText, recipients);
			}

			///** Lists the collection of shared notebooks for all notebooks in the users account.
			// */
			public List<SharedNotebook> ListSharedNotebooks()
			{
				return Client.listSharedNotebooks(AuthenticationToken());
			}

			///** Expunges the SharedNotebooks in the user's account using the SharedNotebook.id as the identifier.
			// NOTE: This function is not available to third party applications. Calls will result in an EDAMUserException with the error code PERMISSION_DENIED.
			// @param sharedNotebookIds a list of SharedNotebook.id longs identifying the objects to delete permanently.
			// */
			public int ExpungeSharedNotebooks(List<long> sharedNotebookIds)
			{
				return Client.expungeSharedNotebooks(AuthenticationToken(), sharedNotebookIds);
			}

			///** Asks the service to make a linked notebook with the provided name, username of the owner and identifiers provided.
			// A linked notebook can be either a link to a public notebook or to a private shared notebook.
			// @param  linkedNotebook The desired fields for the linked notebook must be provided on this object. The name of the linked notebook must be set. Either a username uri or a shard id and share key must be provided otherwise a EDAMUserException is thrown.
			// */
			public LinkedNotebook CreateLinkedNotebook(LinkedNotebook linkedNotebook)
			{
				return Client.createLinkedNotebook(AuthenticationToken(), linkedNotebook);
			}

			///** Asks the service to update a linked notebook.
			// @param  linkedNotebook Updates the name of a linked notebook.
			// */
			public int UpdateLinkedNotebook(LinkedNotebook linkedNotebook)
			{
				return Client.updateLinkedNotebook(AuthenticationToken(), linkedNotebook);
			}

			///** Returns a list of linked notebooks
			// */
			public List<LinkedNotebook> ListLinkedNotebooks()
			{
				return Client.listLinkedNotebooks(AuthenticationToken());
			}

			///** Permanently expunges the linked notebook from the account.
			// NOTE: This function is not available to third party applications. Calls will result in an EDAMUserException with the error code PERMISSION_DENIED.
			// @param  guid The LinkedNotebook.guid field of the LinkedNotebook to permanently remove from the account.
			// */
			public int ExpungeLinkedNotebook(string guid)
			{
				return Client.expungeLinkedNotebook(AuthenticationToken(), guid);
			}

			///** Asks the service to produce an authentication token that can be used to access the contents of a shared notebook from someone else's account.
			// This authenticationToken can be used with the various other NoteStore calls to find and retrieve notes, and if the permissions in the shared notebook are sufficient, to make changes to the contents of the notebook.
			// @param  shareKeyOrGlobalId The 'shareKey' (or 'globalId') identifier from the SharedNotebook that was granted to some recipient. This string internally encodes the notebook identifier and a security signature.
			// */
			public AuthenticationResult AuthenticateToSharedNotebook(string shareKeyOrGlobalId)
			{
				return Client.authenticateToSharedNotebook(shareKeyOrGlobalId, AuthenticationToken());
			}

			///** This function is used to retrieve extended information about a shared notebook by a guest who has already authenticated to access that notebook.
			// This requires an 'authenticationToken' parameter which should be the resut of a call to authenticateToSharedNotebook(...). I.e. this is the token that gives access to the particular shared notebook in someone else's account -- it's not the authenticationToken for the owner of the notebook itself.
			// */
			public SharedNotebook GetSharedNotebookByAuth()
			{
				return Client.getSharedNotebookByAuth(AuthenticationToken());
			}

			///** Attempts to send a single note to one or more email recipients.
			// @param  parameters The note must be specified either by GUID (in which case it will be sent using the existing data in the service), or else the full Note must be passed to this call. This also specifies the additional email fields that will be used in the email.
			// */
			public void EmailNote(NoteEmailParameters parameters)
			{
				Client.emailNote(AuthenticationToken(), parameters);
			}

			///** If this note is not already shared (via its own direct URL), then this will start sharing that note.
			// This will return the secret "Note Key" for this note that can currently be used in conjunction with the Note's GUID to gain direct read-only access to the Note. If the note is already shared, then this won't make any changes to the note, and the existing "Note Key" will be returned. The only way to change the Note Key for an existing note is to stopSharingNote first, and then call this function.
			// @param  guid The GUID of the note to be shared.
			// */
			public string ShareNote(string guid)
			{
				return Client.shareNote(AuthenticationToken(), guid);
			}

			///** If this note is not already shared then this will stop sharing that note and invalidate its "Note Key", so any existing URLs to access that Note will stop working. If the Note is not shared, then this function will do nothing.
			// @param  guid The GUID of the note to be un-shared.
			// */
			public void StopSharingNote(string guid)
			{
				Client.stopSharingNote(AuthenticationToken(), guid);
			}

			///** Asks the service to produce an authentication token that can be used to access the contents of a single Note which was individually shared from someone's account.
			// This authenticationToken can be used with the various other NoteStore calls to find and retrieve the Note and its directly-referenced children.
			// @param  guid The GUID identifying this Note on this shard.
			// @param  noteKey The 'noteKey' identifier from the Note that was originally created via a call to shareNote() and then given to a recipient to access.
			// @param authenticationToken Optional, only required for Yinxiang
			// */
			public AuthenticationResult AuthenticateToSharedNote(string guid, string noteKey)
			{
				return Client.authenticateToSharedNote(guid, noteKey, AuthenticationToken());
			}

			///** Update a SharedNotebook object.
			// @param  sharedNotebook The SharedNotebook object containing the requested changes. The "id" of the shared notebook must be set to allow the service to identify the SharedNotebook to be updated. In addition, you MUST set the email, permission, and allowPreview fields to the desired values. All other fields will be ignored if set.
			// */
			public int UpdateSharedNotebook(SharedNotebook sharedNotebook)
			{
				return Client.updateSharedNotebook(AuthenticationToken(), sharedNotebook);
			}

			///** Set shared notebook recipient settings.
			// @param sharedNotebookId The shared notebooks id
			// @param recipientSettings The settings of the recipient
			// */
			public int SetSharedNotebookRecipientSettings(long sharedNotebookId, SharedNotebookRecipientSettings recipientSettings)
			{
				return Client.setSharedNotebookRecipientSettings(AuthenticationToken(), sharedNotebookId, recipientSettings);
			}

#region Protected routines

			internal List<NoteMetadata> FindNotesMetadata(NoteFilter filter, int maxResults, NotesMetadataResultSpec resultSpec)
			{
				return FindNotesMetadataInternal(filter, 0, resultSpec, maxResults, new List<NoteMetadata>());
			}

			internal List<NoteMetadata> FindNotesMetadataInternal(NoteFilter filter, int offset, NotesMetadataResultSpec resultSpec, int maxResults, List<NoteMetadata> results)
			{
				// If we've already fulfilled a bounded find order, then we are done.
				if (maxResults > 0 && results.Count >= maxResults)
				{
					return results;
				}

				// For this call, ask for the remaining number to fulfill the order, but don't exceed standard max.
				int maxNotesThisCall = FIND_NOTES_DEFAULT_MAX_NOTES;
				if (maxResults > 0)
				{
					maxNotesThisCall = Math.Min(maxResults - results.Count, maxNotesThisCall);
				}

				try
				{
					NotesMetadataList metadata = FindNotesMetadata(filter, offset, maxNotesThisCall, resultSpec);
					// Add these results.
					results.AddRange(metadata.Notes);
					// Did we reach the total? (Use this formulation instead of checking against the results array length
					// because in theory the note count total could change between calls.
					int nextIndex = metadata.StartIndex + metadata.Notes.Count;
					int remainingCount = metadata.TotalNotes - nextIndex;
					// Go for another round if there are more to get.
					if (remainingCount > 0)
					{
						results = FindNotesMetadataInternal(filter, nextIndex, resultSpec, maxResults, results);
						return results;
					}
					else
					{
						// Done.
						return results;
					}
				}
                catch (Evernote.EDAM.Error.EDAMNotFoundException)
                {
                    // Failed to get the sharedNotebook from the service.
                    // The shared notebook could be deleted from the owner.
                    ENSDKLogger.ENSDKLogError("EDAMNotFound error in FindNotesMetadata - shared notebook not found");
                    return null;
                }
				catch (Exception ex)
				{
					throw new Exception(ex.Message, ex.InnerException);
				}
			}

#endregion

		}
	}

}