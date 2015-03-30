using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Type;
using Evernote.EDAM.UserStore;
using EvernoteOAuthNet;
using EvernoteSDK.Advanced;

namespace EvernoteSDK
{
	public class ENSession : ENBusinessNoteStoreClient.IENBusinessNoteStoreClient, ENLinkedNoteStoreClient.IENLinkedNoteStoreClient
	{
		private const string ENSessionBootstrapServerBaseURLStringCN = "app.yinxiang.com";
		private const string ENSessionBootstrapServerBaseURLStringUS = "www.evernote.com";

		private const string ENSessionPreferencesFilename = "EvernoteSDKPrefs.bin";
		private const string ENSessionPreferencesCredentialStore = "CredentialStore";
		private const string ENSessionPreferencesCurrentProfileName = "CurrentProfileName";
		private const string ENSessionPreferencesUser = "User";
		private const string ENSessionPreferencesAppNotebookIsLinked = "AppNotebookIsLinked";
		private const string ENSessionPreferencesLinkedAppNotebook = "LinkedAppNotebook";
		private const string ENSessionPreferencesSharedAppNotebook = "SharedAppNotebook";

		// 
		// A value indicating how the session should approach creating vs. updating existing notes when uploading.
		//
		public enum UploadPolicy
		{
			// Always create a new note.
			Create,
			// Replace existing note if present.
			Replace,
			// Attempt to replace existing note, but if it no longer exists, create new instead.
			ReplaceOrCreate
		}

		// 
		// Option flags for search scope when finding notes.
		//
		[Flags]
		public enum SearchScope
		{
			// Only used if specifying an explicit notebook instead.
			None = 0,
			// Search among all personal notebooks.
			Personal = 1,
			// Search among all notebooks shared to the user by others.
			PersonalLinked = 2,
			// Search among all business notebooks the user has joined.
			Business = 4,
			// Use this if your app uses an "App Notebook". (any other set flags will be ignored.)
			AppNotebook = 8,
			DefaultScope = Personal,
			All = Personal | PersonalLinked | Business
		}

		//
		// Option flags for ordering of results from finding notes.
		//
		[Flags]
		public enum SortOrder
		{
			// The following options address the kind of sort that should be used.

			// Case-insensitive order by title.
			Title = 1,
			// Most recently created first.
			RecentlyCreated = 2,
			// Most recently updated first.
			RecentlyUpdated = 4,
			// Most relevant first. NB only valid when using a single search scope.
			Relevance = 8,

			// The following options address the ordering of the sort.

			// Default order (no flag).
			Normal = 0,
			// Reverse order.
			Reverse = 65536
		}

		internal enum InterfaceType
		{
			Basic,
			Advanced
		}

		private static readonly int ENSessionNotebooksCacheValidity = (5 * 60); // 5 minutes
		internal static InterfaceType ENSessionInterfaceType;

		private struct ENSessionListNotebooksContext
		{
			public List<ENNotebook> ResultNotebooks;
			public List<LinkedNotebook> LinkedPersonalNotebooks;
			public Dictionary<string, SharedNotebook> SharedBusinessNotebooks;
			public List<string> SharedBusinessNotebookGuids;
			public Dictionary<string, Notebook> BusinessNotebooks;
			public Dictionary<string, object> SharedNotebooks;
			public int PendingSharedNotebooks;
		}

		private struct ENSessionUploadContext
		{
			public Note Note;
			public ENNoteRef RefToReplace;
			public ENNotebook Notebook;
			public ENSession.UploadPolicy Policy;
			public ENNoteStoreClient NoteStore;
			public ENNoteRef NoteRef;
		}

		private struct ENSessionFindNotesContext
		{
			public ENNotebook scopeNotebook;
			public SearchScope scope;
			public SortOrder order;
			public NoteFilter noteFilter;
			public NotesMetadataResultSpec resultSpec;
			public int maxResults;
			public bool requiresLocalMerge;
			public bool sortAscending;
			public List<ENNotebook> allNotebooks;
			public List<ENNotebook> linkedNotebooksToSearch;
			public List<NoteMetadata> findMetadataResults;
			public IEnumerable<string> resultGuidsFromBusiness;
			public List<ENSessionFindNotesResult> results;
		}

		public ENSessionDefaultLogger Logger = new ENSessionDefaultLogger();
		public string SourceApplication;

		private bool _IsAuthenticated;
		public bool IsAuthenticated
		{
			get
			{
				return _IsAuthenticated;
			}
		}

		public bool IsPremiumUser
		{
			get
			{
				return EdamUser.Privilege >= PrivilegeLevel.PREMIUM;
			}
		}

		public bool IsBusinessUser
		{
			get
			{
				return EdamUser.Accounting.BusinessId != 0;
			}
		}

		public string UserDisplayName
		{
			get
			{
				string name = EdamUser.Name ?? EdamUser.Username;
				return (name ?? string.Empty);
			}
		}

		public string BusinessDisplayName
		{
			get
			{
				if (IsBusinessUser)
				{
					return EdamUser.Accounting.BusinessName;
				}
				return null;
			}
		}

		internal int UserID
		{
			get
			{
				return EdamUser.Id;
			}
		}

		private ENAuthCache _authCache;
		private ENAuthCache AuthCache
		{
			get
			{
				if (_authCache == null)
				{
					_authCache = new ENAuthCache();
				}
				return _authCache;
			}
			set
			{
				_authCache = value;
			}
		}

		protected string SessionHost {get; set;}
		private User EdamUser {get; set;}
		private string PrimaryAuthenticationToken {get; set;}
		//private string BusinessShardId {get; set;}
		private List<ENNotebook> NotebooksCache {get; set;}
		private DateTime NotebooksCacheDate {get; set;}
		private bool AuthenticationCompleted {get; set;}

		private static string SessionHostOverride;
		private static string ConsumerKey;
		private static string ConsumerSecret;
		private static string DeveloperToken;
		private static string NoteStoreUrl;

#region Advanced
		//// Indicates if your app is capable of supporting linked/business notebooks as app notebook destinations.
		//// Defaults to YES, as the non-advanced interface on ENSession will handle these transparently. If you're
		//// using the note store clients directly, either set this to NO, or be sure you test using a shared notebook as
		//// an app notebook.
		internal bool SupportsLinkedAppNotebook {get; set;}

		//// Once authenticated, this flag will indicate whether the app notebook chosen by the user is, in fact, linked.
		//// (This will never be YES if you have set the flag above to NO). If so, you must take this into account:
		//// the primary note store will not allow you to access the notebook; instead, you must authenticate to the
		//// relevant linked notebook. You can find the linked notebook record by calling -listLinkedNotebooks on the
		//// primary note store.
		internal bool AppNotebookIsLinked
		{
			get
			{
                var linked = Preferences.ObjectForKey(ENSessionPreferencesAppNotebookIsLinked);
                return (linked != null) ? (bool)linked : false;
			}
		}

		//// This gives access to the preferences store that the session keeps independently from NSUserDefaults, and is
		//// destroyed when the session unauthenticates. This should generally not be used in your application, but
		//// it is used by the sample UIActivity to track recently-used notebook destinations, which are of course
		//// session-specific. If you use it, please namespace your keys appropriately to avoid collisions.
		private ENPreferencesStore _Preferences;
		internal ENPreferencesStore Preferences
		{
			get
			{
				return _Preferences;
			}
		}

#endregion

#region Session setup

		public ENSession()
		{
			// Check to see if the app's setup parameters are set and look reasonable.
			// If this test fails, we'll essentially set a singleton to null and never be able
			// to fix it, which is the desired development-time behavior.
			if (!(CheckSharedSessionSettings()))
			{
				_sharedSession = null;
			}

			// Default to supporting linked notebooks for app notebook. Developer can toggle this off
			// if they're using advanced features and don't want to deal with the added complexity.
			SupportsLinkedAppNotebook = true;
			Startup();
		}

		///**
		//*  Set up the session object with an app consumer key and secret. This is the standard setup
		//*  method. App keys are available from dev.evernote.com. You must call this method BEFORE the
		//*  sharedSession is accessed.
		//*
		//*   key    Consumer key for your app
		//*   secret Consumer secret for yor app
		//*   host   (optional) If you're using a non-production host, like the developer sandbox, specify it here.
		//*/
		public static void SetSharedSessionConsumerKey(string sessionConsumerKey, string sessionConsumerSecret, string sessionHost = null)
		{
			ConsumerKey = sessionConsumerKey;
			ConsumerSecret = sessionConsumerSecret;
			SessionHostOverride = sessionHost;
			DeveloperToken = null;
			NoteStoreUrl = null;
		}

		///**
		//*  Set up the session object with a developer token and Note Store URL. This is an alternate
		//*  setup method used only when you are authenticating directly to your own production account. (An
		//*  app for general distribution will use a consumer key and secret.) You must call this method BEFORE
		//* the sharedSession is accessed.
		//*
		//*   token The developer token
		//*   url   The Note Store URL.
		//*/
		public static void SetSharedSessionDeveloperToken(string sessionDeveloperToken, string sessionNoteStoreUrl)
		{
			DeveloperToken = sessionDeveloperToken;
			NoteStoreUrl = sessionNoteStoreUrl;
			ConsumerKey = null;
			ConsumerSecret = null;
		}

		///**
		//*  Access the shared session object; this is the only way to get a valid ENSession.
		//*
		//*  return: The shared session object.
		//*/
		private static Lazy<ENSession> _sharedSession = new Lazy<ENSession>(() => new ENSession());
		protected static Lazy<ENSessionAdvanced> _sharedSessionAdvanced = new Lazy<ENSessionAdvanced>(() => new ENSessionAdvanced());
		public static ENSession SharedSession
		{
			get
			{
				if (ENSessionInterfaceType == InterfaceType.Advanced)
				{
					return _sharedSessionAdvanced.Value;
				}
				else
				{
					return _sharedSession.Value;
				}
			}
		}

		private static bool CheckSharedSessionSettings()
		{
			if (DeveloperToken != null && NoteStoreUrl != null)
			{
				return true;
			}

			if (ConsumerKey != null && ConsumerKey != "your key" && ConsumerSecret != null && ConsumerSecret != "your secret")
			{
				return true;
			}

			string err = "Cannot create shared Evernote session without either a valid consumer key/secret pair, or a developer token set.";
			// Use Console.WriteLine and not the session logger here, or we'll deadlock since we're still creating the session.
			Console.WriteLine(err);
			throw new ArgumentException(err);
		}
#endregion

		public void Startup()
		{
			Logger = new ENSessionDefaultLogger();
			_Preferences = new ENPreferencesStore(ENSessionPreferencesFilename);
			AuthenticationCompleted = false;

			SelectInitialSessionHost();

			// If the developer token is set, then we can short circuit the entire auth flow and just call ourselves authenticated.
			if (DeveloperToken != null)
			{
				_IsAuthenticated = true;
				PrimaryAuthenticationToken = DeveloperToken;
				PerformPostAuthentication();
				return;
			}

			// We'll restore an existing session if there was one. Check to see if we have valid
			// primary credentials stashed away already.
			ENCredentials credentials = CredentialsForHost(SessionHost);
			if (credentials == null || !credentials.AreValid())
			{
				_IsAuthenticated = false;
				Preferences.RemoveAllObjects();
				return;
			}

			_IsAuthenticated = true;
			PrimaryAuthenticationToken = credentials.AuthenticationToken;

			// We appear to have valid personal credentials, so populate the user object from cache,
			// and pull up business credentials. Refresh the business credentials if necessary, and the user
			// object always.
			EdamUser = (User)Preferences.ObjectForKey(ENSessionPreferencesUser);

			PerformPostAuthentication();
		}

		private void SelectInitialSessionHost()
		{
			if (SessionHostOverride != null && SessionHostOverride.Length > 0)
			{
				// Use the override given by the developer. This is optional, and
				// generally used for the sandbox.
				SessionHost = SessionHostOverride;
			}
			else if (NoteStoreUrl != null)
			{
				// If we have a developer key, just get the host from the note store url.
				Uri nsUrl = new Uri(NoteStoreUrl);
				SessionHost = nsUrl.Host;
			}
			else if (CurrentProfileName() == ENConstants.ENBootstrapProfileNameInternational)
			{
				SessionHost = ENSessionBootstrapServerBaseURLStringUS;
			}
			else if (CurrentProfileName() == ENConstants.ENBootstrapProfileNameChina)
			{
				SessionHost = ENSessionBootstrapServerBaseURLStringCN;
			}
			else
			{
				// Choose the initial host based on locale. Simplified Chinese locales get the yinxiang service.
				string locale = CultureInfo.CurrentCulture.ToString().ToLower();
                if (locale.StartsWith("zh_hans") || locale == "zh_cn" || locale == "zh")
				{
					SessionHost = ENSessionBootstrapServerBaseURLStringCN;
				}
				else
				{
					SessionHost = ENSessionBootstrapServerBaseURLStringUS;
				}
			}
		}

		public void AuthenticateToEvernote()
		{
			// Authenticate is idempotent; check if we're already authenticated.
			if (IsAuthenticated)
			{
				return;
			}

			EdamUser = null;

			// If the developer token is set, then we can short circuit the entire auth flow and just call ourselves authenticated.
			if (DeveloperToken != null)
			{
				_IsAuthenticated = true;
				PrimaryAuthenticationToken = DeveloperToken;
				PerformPostAuthentication();
				return;
			}

			// Start bootstrapping
			string locale = CultureInfo.CurrentCulture.ToString();
			ENUserStoreClient bootstrapUserStore = new ENUserStoreClient(UserStoreUrl(), null);
			try
			{
				BootstrapInfo info = bootstrapUserStore.GetBootstrapInfo(locale);
				// Use first profile as the preferred profile.
				BootstrapProfile profile = info.Profiles[0];
				string host = profile.Settings.ServiceHost;
				EvernoteOAuth.HostService service = 0;
				if (host == ENSessionBootstrapServerBaseURLStringUS)
				{
					service = EvernoteOAuth.HostService.Production;
				}
				else if (host == ENSessionBootstrapServerBaseURLStringCN)
				{
					service = EvernoteOAuth.HostService.Yinxiang;
				}
				else
				{
					service = EvernoteOAuth.HostService.Sandbox;
				}

				// Perform the authentication.
				var oauth = new EvernoteOAuth(service, ConsumerKey, ConsumerSecret, SupportsLinkedAppNotebook);

				string errResponse = oauth.Authorize();
				if (errResponse.Length == 0)
				{
					ENCredentials credentials = new ENCredentials(SessionHost, oauth.UserId, oauth.NoteStoreUrl, oauth.WebApiUrlPrefix, oauth.Token, oauth.Expires.ToDateTime());
					_IsAuthenticated = true;
					AddCredentials(credentials);
					SetCurrentProfileNameFromHost(credentials.Host);
					SessionHost = credentials.Host;
					PrimaryAuthenticationToken = credentials.AuthenticationToken;
					if (oauth.LinkedAppNotebookSelected)
					{
						Preferences.SetObject(oauth.LinkedAppNotebookSelected, ENSessionPreferencesAppNotebookIsLinked);
					}
					AuthenticationCompleted = true;
					PerformPostAuthentication();
				}
				else
				{
					Unauthenticate();
				}
			}
			catch (Exception)
			{
				Unauthenticate();
			}
		}

		public void PerformPostAuthentication()
		{
			User edUser = null;
			try
			{
				edUser = UserStore.GetUser();
			}
			catch (Exception)
			{

			}
			if (edUser != null)
			{
				EdamUser = edUser;
				Preferences.SetObject(EdamUser, ENSessionPreferencesUser);
			}
			else
			{
				// During an initial authentication, a failure in getUser or authenticateToBusiness is considered fatal.
				// But when refreshing a session, eg on app restart, we don't want to sign out users just for network
				// errors, or transient problems.
				if (AuthenticationCompleted)
				{
					Unauthenticate();
				}
			}
		}

		private void Unauthenticate()
		{
			ENSDKLogger.ENSDKLogInfo("ENSession is unauthenticating.");

			// Revoke the primary auth token, so the app session will not appear any longer on the user's
			// security page. This is purely opportunistic, of course, hence ignoring the result.
			// Note also that this is asynchronous, but the rest of this method gets rid of all the session state,
			// so keep the user store around long enough to see it through, but keep it separate from the
			// normal session state.
			if (_IsAuthenticated)
			{
				UserStore.RevokeLongSession(PrimaryAuthenticationToken);
			}
			_IsAuthenticated = false;
			EdamUser = null;
			PrimaryAuthenticationToken = null;
			UserStore = null;
			PrimaryNoteStore = null;
			BusinessNoteStore = null;
			AuthCache = new ENAuthCache();
            if (NotebooksCache != null)
            {
                NotebooksCache.Clear();
            }
			NotebooksCacheDate = new DateTime();

			// Manually clear credentials. This ensures they're removed from the registry also.
			CredentialStore().ClearAllCredentials();
			SaveCredentialStore(CredentialStore());

			Preferences.RemoveAllObjects();
			SelectInitialSessionHost();
		}

#region ListNotebooks

		// Notes on the flow of this process, because it's somewhat byzantine:
		// 1. Get all of the user's personal notebooks.
		// 2. Get all of the user's linked notebooks. These will include business and/or shared notebooks.
		// 3. If the user is a business user:
		//      a. Get the business's shared notebooks. Some of these may match to personal linked notebooks.
		//      b. Get the business's linked notebooks. Some of these will match to shared notebooks in (a), providing a
		//         complete authorization story for the notebook.
		// 4. For any remaining linked nonbusiness notebooks, auth to each and get authorization information.
		// 5. Sort and return the full result set.
		//
		// For personal users, therefore, this will make 2 + n roundtrips, where n is the number of shared notebooks.
		// For business users, this will make 2 + 2 + n roundtrips, where n is the number of nonbusiness shared notebooks.
		[ComVisible(false)]
		public List<ENNotebook> ListNotebooks()
		{
			if (!IsAuthenticated)
			{
				throw new ENAuthExpiredException();
			}

			// Do we have a cached result that is unexpired?
			if (NotebooksCache != null && NotebooksCache.Count > 0 && DateTime.Now.Subtract(NotebooksCacheDate).TotalSeconds < ENSessionNotebooksCacheValidity)
			{
				return NotebooksCache;
			}

			NotebooksCache = null;
			NotebooksCacheDate = new DateTime();

			ENSessionListNotebooksContext context = new ENSessionListNotebooksContext();
			context.ResultNotebooks = new List<ENNotebook>();
			context = ListNotebooks_ListNotebooks(context);
			return context.ResultNotebooks;
		}

		public ENCollection ListNotebooksForCOM()
		{
			List<ENNotebook> results = ENSession.SharedSession.ListNotebooks();

			ENCollection comResults = new ENCollection();
            foreach (ENNotebook result in results)
            {
                object tempvar = result;
                object tempkey = result.Guid;
                comResults.Add(ref tempvar, ref tempkey);
            }
			return comResults;
		}

		private ENSessionListNotebooksContext ListNotebooks_ListNotebooks(ENSessionListNotebooksContext context)
		{
			List<Notebook> notebooks = PrimaryNoteStore.ListNotebooks();
			if (notebooks.Count > 0)
			{
				// Populate the result list with personal notebooks.
				foreach (Notebook book in notebooks)
				{
					ENNotebook result = new ENNotebook(book);
					context.ResultNotebooks.Add(result);
				}
				// Now get any shared notebook records for the personal account.
				context = ListNotebooks_ListSharedNotebooks(context);
			}
			else
			{
				// This must be a single notebook auth token, so try getting linked notebooks.
				context = ListNotebooks_ListLinkedNotebooks(context);
			}

			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_ListSharedNotebooks(ENSessionListNotebooksContext context)
		{
			try
			{
				// The return value "notebooks" is no longer currently being used. But we might do something later in this block and this call is not expensive so we leave it there without intentionally doing anything.
				List<SharedNotebook> notebooks = PrimaryNoteStore.ListSharedNotebooks();
				context = ListNotebooks_ListLinkedNotebooks(context);
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("Error from listSharedNotebooks in user's store: {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}
			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_ListLinkedNotebooks(ENSessionListNotebooksContext context)
		{
			try
			{
				List<LinkedNotebook> linkedNotebooks = PrimaryNoteStore.ListLinkedNotebooks();
				if (linkedNotebooks.Count == 0)
				{
					context = ListNotebooks_PrepareResults(context);
				}
				else
				{
					context.LinkedPersonalNotebooks = linkedNotebooks;
					if (BusinessNoteStore != null)
					{
						context = ListNotebooks_FetchSharedBusinessNotebooks(context);
					}
					else
					{
						context = ListNotebooks_FetchSharedNotebooks(context);
					}
				}
			}
			catch (Evernote.EDAM.Error.EDAMUserException ex)
			{
				if (ex.ErrorCode == Evernote.EDAM.Error.EDAMErrorCode.PERMISSION_DENIED)
				{
					// App has a single notebook auth token, so skip to the end.
					context = ListNotebooks_PrepareResults(context);
				}
				else
				{
					ENSDKLogger.ENSDKLogError(string.Format("Error from listLinkedNotebooks in user's store: {0}", ex.Message));
					throw new Exception(ex.Message, ex.InnerException);
				}
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("Error from listLinkedNotebooks in user's store: {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}
			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_FetchSharedBusinessNotebooks(ENSessionListNotebooksContext context)
		{
			try
			{
				List<SharedNotebook> sharedNotebooks = BusinessNoteStore.ListSharedNotebooks();
				// Run through the results, and set each notebook keyed by its shareKey, which
				// is how we'll find corresponding linked notebooks.
				context.SharedBusinessNotebooks = new Dictionary<string, SharedNotebook>();
				context.SharedBusinessNotebookGuids = new List<string>();
				foreach (SharedNotebook notebook in sharedNotebooks)
				{
					context.SharedBusinessNotebooks.Add(notebook.ShareKey, notebook);
					context.SharedBusinessNotebookGuids.Add(notebook.NotebookGuid);
				}

				// Now continue on to grab all of the linked notebooks for the business.
				context = ListNotebooks_FetchBusinessNotebooks(context);
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("Error from listSharedNotebooks in business store: {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}
			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_FetchBusinessNotebooks(ENSessionListNotebooksContext context)
		{
			try
			{
				List<Notebook> notebooks = BusinessNoteStore.ListNotebooks();
				// Run through the results, and set each notebook keyed by its guid, which
				// is how we'll find it from the shared notebook.
				context.BusinessNotebooks = new Dictionary<string, Notebook>();
				foreach (Notebook notebook in notebooks)
				{
					context.BusinessNotebooks.Add(notebook.Guid, notebook);
				}
				context = ListNotebooks_ProcessBusinessNotebooks(context);
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("Error from listNotebooks in business store: {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}
			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_ProcessBusinessNotebooks(ENSessionListNotebooksContext context)
		{
			// Postprocess our notebook sets for business notebooks. For every linked notebook in the personal
			// account, check for a corresponding business shared notebook (by shareKey). If we find it, also
			// grab its corresponding notebook object from the business notebook list.
			List<LinkedNotebook> linkedPersonalNotebooksCopy = new List<LinkedNotebook>(context.LinkedPersonalNotebooks);
			foreach (LinkedNotebook linkedNotebook in linkedPersonalNotebooksCopy)
			{
                SharedNotebook sharedNotebook = null;
                if (linkedNotebook.ShareKey != null)
                {
                    context.SharedBusinessNotebooks.TryGetValue(linkedNotebook.ShareKey, out sharedNotebook);
                    if (sharedNotebook != null)
                    {
                        // This linked notebook corresponds to a business notebook.
                        Notebook businessNotebook = null;
                        context.BusinessNotebooks.TryGetValue(sharedNotebook.NotebookGuid, out businessNotebook);
                        if (businessNotebook != null)
                        {
                            ENNotebook result = new ENNotebook(businessNotebook, sharedNotebook, linkedNotebook);
                            context.ResultNotebooks.Add(result);
                        }
                        context.LinkedPersonalNotebooks.Remove(linkedNotebook);
                    }
                }
			}

			// Any remaining linked notebooks are personal shared notebooks. No shared notebooks?
			// Then go directly to results preparation.
			if (context.LinkedPersonalNotebooks.Count == 0)
			{
				context = ListNotebooks_PrepareResults(context);
			}
			else
			{
				context = ListNotebooks_FetchSharedNotebooks(context);
			}

			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_FetchSharedNotebooks(ENSessionListNotebooksContext context)
		{
			// Fetch shared notebooks for any non-business linked notebooks remaining in the
			// array in the context. We will have already pulled out the linked notebooks that
			// were processed for business.
			context.PendingSharedNotebooks = context.LinkedPersonalNotebooks.Count;
			Dictionary<string, object> sharedNotebooks = new Dictionary<string, object>();
			context.SharedNotebooks = sharedNotebooks;


            List<LinkedNotebook> linkedPersonalNotebooksCopy = new List<LinkedNotebook>(context.LinkedPersonalNotebooks);
            foreach (LinkedNotebook linkedNotebook in linkedPersonalNotebooksCopy)
			{
				ENNoteStoreClient noteStore = NoteStoreForLinkedNotebook(linkedNotebook);
				if (linkedNotebook.ShareKey == null)
				{
					// ShareKey is null means it's a public notebook.
					try
					{
						PublicUserInfo info = UserStore.GetPublicUserInfo(linkedNotebook.Username);
						try
						{
							Notebook sharedNotebook = noteStore.GetPublicNotebook(info.UserId, linkedNotebook.Uri);
							sharedNotebooks.Add(linkedNotebook.Guid, sharedNotebook);
							context = ListNotebooks_CompletePendingSharedNotebook(context);
						}
						catch (Exception ex)
						{
							ENSDKLogger.ENSDKLogError(string.Format("Error from getSharedNotebookByAuth against a personal linked notebook: {0}", ex.Message));
							throw new Exception(ex.Message, ex.InnerException);
						}
					}
					catch (Exception ex)
					{
						ENSDKLogger.ENSDKLogError(string.Format("Error from getSharedNotebookByAuth against a personal linked notebook: {0}", ex.Message));
						throw new Exception(ex.Message, ex.InnerException);
					}
				}
				else
				{
					try
                    {
                        SharedNotebook sharedNotebook = null;
                        try
                        {
                            sharedNotebook = noteStore.GetSharedNotebookByAuth();
                            // Add the shared notebook to the map.
                            sharedNotebooks.Add(linkedNotebook.Guid, sharedNotebook);
                            context = ListNotebooks_CompletePendingSharedNotebook(context);
                        }
                        catch (Exception)
                        {
                            // Failed to get the sharedNotebook from the service.
                            // The shared notebook could be deleted from the owner.
                            // We remove the linked notebook record from the context so it won't be listed in the result.
                            ENSDKLogger.ENSDKLogError(string.Format("Failed to get shared notebook for linked notebook record {0}", linkedNotebook));
                            context.LinkedPersonalNotebooks.Remove(linkedNotebook);
                            context = ListNotebooks_CompletePendingSharedNotebook(context);
                        }
					}
					catch (Exception ex)
					{
						ENSDKLogger.ENSDKLogError(string.Format("Error from getSharedNotebookByAuth against a personal linked notebook: {0}", ex.Message));
						throw new Exception(ex.Message, ex.InnerException);
					}
				}
			}

			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_CompletePendingSharedNotebook(ENSessionListNotebooksContext context)
		{
			context.PendingSharedNotebooks--;
			if (context.PendingSharedNotebooks == 0)
			{
				context = ListNotebooks_ProcessSharedNotebooks(context);
			}
			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_ProcessSharedNotebooks(ENSessionListNotebooksContext context)
		{
			// Process the results
			foreach (LinkedNotebook linkedNotebook in context.LinkedPersonalNotebooks)
			{
				object sharedNotebook = context.SharedNotebooks[linkedNotebook.Guid];
				ENNotebook result = null;
				if (sharedNotebook.GetType() == typeof(SharedNotebook))
				{
					// Shared notebook with individuals
					result = new ENNotebook((SharedNotebook)sharedNotebook, linkedNotebook);
				}
				else
				{
					// Public notebook
					result = new ENNotebook((Notebook)sharedNotebook, linkedNotebook);
				}
				context.ResultNotebooks.Add(result);
			}

			context = ListNotebooks_PrepareResults(context);
			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_PrepareResults(ENSessionListNotebooksContext context)
		{
			// If there's only one notebook, and it's not flagged as the default notebook for the account, then
			// we must be in a single-notebook auth scenario. In this case, simply override the flag so to a caller it
			// will appear to be the default anyway. Note that we only do this if it's not already the default. If a single
			// notebook result is already marked default, then it *could* be that there really is one notebook, and we don't
			// want to have the caller persist an override flag that might be inapplicable later.
			if (context.ResultNotebooks.Count == 1)
			{
				ENNotebook soleNotebook = context.ResultNotebooks[0];
				if (!soleNotebook.IsDefaultNotebook)
				{
					soleNotebook.IsDefaultNotebookOverride = true;
				}
			}

			// Sort them by name. This is just a convenience for the caller in case they don't bother to sort them themselves.
			context.ResultNotebooks.Sort((x, y) => x.Name.CompareTo(y.Name));
			context = ListNotebooks_Complete(context);
			return context;
		}

		private ENSessionListNotebooksContext ListNotebooks_Complete(ENSessionListNotebooksContext context)
		{
			NotebooksCache = context.ResultNotebooks;
			NotebooksCacheDate = DateTime.Now;
			return context;
		}

#endregion

#region UploadNote

		public ENNoteRef UploadNote(ENNote note, ENNotebook notebook)
		{
			return UploadNote(note, UploadPolicy.Create, notebook, null);
		}

		public ENNoteRef UploadNote(ENNote note, ENSession.UploadPolicy policy, ENNotebook notebook, ENNoteRef noteToReplace)
		{
			if (note == null)
			{
				ENSDKLogger.ENSDKLogError("Must specify note");
				throw new ENInvalidDataException();
			}

			if (policy == UploadPolicy.Replace && noteToReplace == null || policy == UploadPolicy.ReplaceOrCreate && noteToReplace == null)
			{
				ENSDKLogger.ENSDKLogError("Must specify existing ID when requesting a replacement policy");
				throw new ENInvalidDataException();
			}

			if (policy == UploadPolicy.Create && noteToReplace != null)
			{
				ENSDKLogger.ENSDKLogError("Can't use create policy when specifying an existing note ref. Ignoring.");
				noteToReplace = null;
			}

			if (notebook != null && !notebook.AllowsWriting)
			{
				var errorMessage = "A specified notebook must not be readonly";
				ENSDKLogger.ENSDKLogError(errorMessage);
				throw new ArgumentException(errorMessage);
			}

			if (!IsAuthenticated)
			{
				throw new ENAuthExpiredException();
			}

			// Run size validation on any resources included with the note. This is done at upload time because
			// the sizes are a function of the user's service level, which can change.
			if (!note.ValidateForLimits())
			{
				ENSDKLogger.ENSDKLogError("Note failed limits validation. Cannot upload.");
				throw new ENLimitReachedException();
			}

			ENSessionUploadContext context = new ENSessionUploadContext();
			if (noteToReplace != null)
			{
				context.Note = note.EDAMNoteToReplaceServiceNoteGUID(noteToReplace.Guid);
			}
			else
			{
				context.Note = note.EDAMNote();
			}
			context.RefToReplace = noteToReplace;
			context.Notebook = notebook;
			context.Policy = policy;

			context = UploadNote_DetermineDestination(context);
			return context.NoteRef;
		}

		private ENSessionUploadContext UploadNote_DetermineDestination(ENSessionUploadContext context)
		{
			// Begin prepping a resulting note ref.
			context.NoteRef = new ENNoteRef();

			// If this app uses an app notebook and that notebook is linked, then no matter what the caller says,
			// we're going to need to use the explicit notebook destination to comply with shared notebook auth.
			if (AppNotebookIsLinked)
			{
				// Do we have a cached linked notebook record to use as a destination?
				LinkedNotebook linkedNotebook = (LinkedNotebook)Preferences.ObjectForKey(ENSessionPreferencesLinkedAppNotebook);
				if (linkedNotebook != null)
				{
					context.NoteStore = NoteStoreForLinkedNotebook(linkedNotebook);
					context.NoteRef.Type = ENNoteRef.ENNoteRefType.TypeShared;
					context.NoteRef.LinkedNotebook = ENLinkedNotebookRef.LinkedNotebookRefFromLinkedNotebook(linkedNotebook);

					// Because we are using a linked app notebook, and authenticating to it with the shared auth model,
					// we must provide a notebook guid in the note or face an error.
					SharedNotebook sharedNotebook = (SharedNotebook)Preferences.ObjectForKey(ENSessionPreferencesSharedAppNotebook);
					context.Note.NotebookGuid = sharedNotebook.NotebookGuid;
				}
				else
				{
					// We don't have a linked notebook record to use.  We need to go find one.
					context = UploadNote_FindLinkedAppNotebook(context);
					return context;
				}
			}

			if (context.NoteStore == null)
			{
				if (context.RefToReplace != null)
				{
					context.NoteStore = NoteStoreForNoteRef(context.RefToReplace);
					context.NoteRef.Type = context.RefToReplace.Type;
					context.NoteRef.LinkedNotebook = context.RefToReplace.LinkedNotebook;
				}
				else if (context.Notebook != null && context.Notebook.IsBusinessNotebook)
				{
					context.NoteStore = BusinessNoteStore;
					context.NoteRef.Type = ENNoteRef.ENNoteRefType.TypeBusiness;
				}
				else if (context.Notebook != null && context.Notebook.IsLinked)
				{
					context.NoteStore = NoteStoreForLinkedNotebook(context.Notebook.LinkedNotebook);
					context.NoteRef.Type = ENNoteRef.ENNoteRefType.TypeShared;
					context.NoteRef.LinkedNotebook = ENLinkedNotebookRef.LinkedNotebookRefFromLinkedNotebook(context.Notebook.LinkedNotebook);
				}
				else
				{
					// This is the normal case. Either the app has not specified a destination notebook, or the
					// notebook is personal.
					context.NoteStore = PrimaryNoteStore;
					context.NoteRef.Type = ENNoteRef.ENNoteRefType.TypePersonal;
				}
			}

			if (context.RefToReplace != null)
			{
				context = UploadNote_Update(context);
			}
			else
			{
				context = UploadNote_Create(context);
			}

			return context;
		}

		private ENSessionUploadContext UploadNote_Update(ENSessionUploadContext context)
		{
			// If we're replacing a note, fixup the update date.
			context.Note.Updated = DateTime.Now.ToEdamTimestamp();

			context.Note.Guid = context.RefToReplace.Guid;
			context.Note.Active = true;

			try
			{
				Note resultNote = context.NoteStore.UpdateNote(context.Note);
				context.NoteRef = context.RefToReplace; // The result by definition has the same ref.
			}
			catch (Evernote.EDAM.Error.EDAMNotFoundException ex)
			{
				if (ex.Message.Contains("Note.guid"))
				{
					// We tried to replace a note that isn't there anymore. Now we look at the replacement policy.
					if (context.Policy == UploadPolicy.ReplaceOrCreate)
					{
						// Can't update it, just create it anew.
						context.Note.Guid = null;
						context.Policy = UploadPolicy.Create;
						context.RefToReplace = null;

						// Go back to determining the destination before creating. We'll take into account a supplied
						// notebook at this point, which may actually be in a different place than the note we were
						// trying to replace. We don't have enough information otherwise to reliably place a new note
						// in the same notebook as the original one, so defaulting to a default notebook in a given
						// note store is less predictable than defaulting to the default overall. In practice, this
						// works out the same most of the time. (For app notebook apps, it'll end up in the app notebook
						// anyway of course.)
						context = UploadNote_DetermineDestination(context);
						return context;
					}
				}
				ENSDKLogger.ENSDKLogError(string.Format("Failed to updateNote for uploadNote: {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("Failed to updateNote for uploadNote: {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}
			return context;
		}

		private ENSessionUploadContext UploadNote_FindLinkedAppNotebook(ENSessionUploadContext context)
		{
			try
			{
				// We know the app notebook is linked. List linked notebooks; we expect to find a single result.
				List<LinkedNotebook> linkedNotebooks = PrimaryNoteStore.ListLinkedNotebooks();
				if (linkedNotebooks.Count < 1)
				{
					// Uh-oh; there's no destination to use. We have to fail the request.
					string errorMessage = "Cannot find linked app notebook. Perhaps user deleted it?";
					ENSDKLogger.ENSDKLogError(errorMessage);
					throw new Exception(errorMessage);
				}
				if (linkedNotebooks.Count > 1)
				{
					ENSDKLogger.ENSDKLogInfo(string.Format("Expected to find single linked notebook, found {0}", linkedNotebooks.Count));
				}
				// Take this notebook, and cache it.
				LinkedNotebook linkedNotebook = linkedNotebooks[0];
				if (linkedNotebook.ShareKey == null)
				{
					// The notebook is a public notebook so it's read only. Fail the request with error.
					throw new ENPermissionDeniedException();
				}
				Preferences.SetObject(linkedNotebook, ENSessionPreferencesLinkedAppNotebook);

				// Go find the shared notebook that corresponds to this.
				context = UploadNote_FindSharedAppNotebook(context);
				return context;
			}
			catch (Exception ex)
			{
				// Uh-oh; there's no destination to use. We have to fail the request.
				ENSDKLogger.ENSDKLogError(string.Format("Failed to listLinkedNotebooks for uploadNote; turning into NotFound: {0}", ex.Message));
				throw new Evernote.EDAM.Error.EDAMNotFoundException();
			}
		}

		private ENSessionUploadContext UploadNote_FindSharedAppNotebook(ENSessionUploadContext context)
		{
			LinkedNotebook linkedNotebook = (LinkedNotebook)Preferences.ObjectForKey(ENSessionPreferencesLinkedAppNotebook);
			ENNoteStoreClient linkedNoteStore = NoteStoreForLinkedNotebook(linkedNotebook);
			try
			{
				SharedNotebook sharedNotebook = linkedNoteStore.GetSharedNotebookByAuth();
				if (sharedNotebook != null)
				{
					// Persist the shared notebook record.
					Preferences.SetObject(sharedNotebook, ENSessionPreferencesSharedAppNotebook);
					// Go back and redetermine the destination.
					context = UploadNote_DetermineDestination(context);
				}
				else
				{
					// Uh-oh; there's no destination to use. We have to fail the request.
					ENSDKLogger.ENSDKLogError("getSharedNotebookByAuth for uploadNote returned empty sharedNotebook; turning into NotFound.");
					throw new Evernote.EDAM.Error.EDAMNotFoundException();
				}
			}
			catch (Exception ex)
			{
				// Uh-oh; there's no destination to use. We have to fail the request.
				ENSDKLogger.ENSDKLogError(string.Format("Failed to getSharedNotebookByAuth for uploadNote; turning into NotFound: {0}", ex.Message));
				throw new Evernote.EDAM.Error.EDAMNotFoundException();
			}
			return context;
		}

		private ENSessionUploadContext UploadNote_Create(ENSessionUploadContext context)
		{
			// Clear create and update dates. The service will set these to sensible defaults for a new note.
			context.Note.Created = 0;
			context.Note.Updated = 0;

			// Write in the notebook guid if we're providing one.
			if (context.Note.NotebookGuid == null && context.Notebook != null)
			{
				context.Note.NotebookGuid = context.Notebook.Guid;
			}

			try
			{
				Note resultNote = context.NoteStore.CreateNote(context.Note);
				context.NoteRef.Guid = resultNote.Guid;
			}
			catch (Exception ex)
			{
				context.NoteRef = null;
				ENSDKLogger.ENSDKLogError(string.Format("Failed to createNote for uploadNote: {0}", ex.Message));
				throw new Evernote.EDAM.Error.EDAMNotFoundException();
			}
			return context;
		}

#endregion

#region ShareNote

		public string ShareNote(ENNoteRef noteRef)
		{
			if (!IsAuthenticated)
			{
				throw new ENAuthExpiredException();
			}

			ENNoteStoreClient noteStore = NoteStoreForNoteRef(noteRef);
			try
			{
				string noteKey = noteStore.ShareNote(noteRef.Guid);
				string shardId = ShardIdForNoteRef(noteRef);
				return ENShareUrlHelper.ShareUrlString(noteRef.Guid, shardId, noteKey, SessionHost, null);
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("Failed to shareNote: {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}
		}

#endregion

#region DeleteNote

		public void DeleteNote(ENNoteRef noteRef)
		{
			if (!IsAuthenticated)
			{
				throw new ENAuthExpiredException();
			}

			ENNoteStoreClient noteStore = NoteStoreForNoteRef(noteRef);
			try
			{
				int usn = noteStore.DeleteNote(noteRef.Guid);
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("Failed to deleteNote: {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}
		}

#endregion

#region FindNotes
		[ComVisible(false)]
        public List<ENSessionFindNotesResult> FindNotes(ENNoteSearch noteSearch, ENNotebook notebook, SearchScope scope, SortOrder order, int maxResults)
        {
            if (!IsAuthenticated)
            {
                throw new ENAuthExpiredException();
            }

            // App notebook scope is internally just an "all" search, because we don't a priori know where the app
            // notebook is. There's some room for a fast path in this flow if we have a saved linked record to a
            // linked app notebook, but that case is likely rare enough to prevent complexifying this code for.
            if (scope.HasFlag(SearchScope.AppNotebook))
            {
                scope = SearchScope.All;
            }

            // Validate the scope and sort arguments.
            if (notebook != null && scope != SearchScope.None)
            {
                ENSDKLogger.ENSDKLogError("No search scope necessary if notebook provided.");
                scope = SearchScope.None;
            }
            else if (notebook == null && scope == SearchScope.None)
            {
                ENSDKLogger.ENSDKLogError("Search scope or notebook must be specified. Defaulting to personal scope.");
                scope = SearchScope.DefaultScope;
            }

            bool requiresLocalMerge = false;
            if (scope != SearchScope.None)
            {
                // Check for multiple scopes. Because linked scope can subsume multiple linked notebooks, that *always* triggers
                // the multiple scopes. If not, then both personal and business must be set together.
                if ((scope.HasFlag(SearchScope.Personal) && scope.HasFlag(SearchScope.Business)) || scope.HasFlag(SearchScope.PersonalLinked))
                {
                    // If we're asked for multiple scopes, relevance is not longer supportable (since we
                    // don't know how to combine relevance on the client), so default to updated date,
                    // which is probably the closest proxy to relevance.
                    if (order.HasFlag(SortOrder.Relevance))
                    {
                        ENSDKLogger.ENSDKLogError("Cannot sort by relevance across multiple search scopes. Using update date.");
                        order = (ENSession.SortOrder)EN_FLAG_CLEAR(order, SortOrder.Relevance);
                        order = (ENSession.SortOrder)EN_FLAG_SET(order, SortOrder.RecentlyUpdated);
                    }
                    requiresLocalMerge = true;
                }
            }

            NotesMetadataResultSpec resultSpec = new NotesMetadataResultSpec();
            resultSpec.IncludeNotebookGuid = true;
            resultSpec.IncludeTitle = true;
            resultSpec.IncludeCreated = true;
            resultSpec.IncludeUpdated = true;
            resultSpec.IncludeUpdateSequenceNum = true;

            NoteFilter noteFilter = new NoteFilter();
            noteFilter.Words = noteSearch.SearchString;

            if (order.HasFlag(SortOrder.Title))
            {
                noteFilter.Order = (System.Int32)NoteSortOrder.TITLE;
            }
            else if (order.HasFlag(SortOrder.RecentlyCreated))
            {
                noteFilter.Order = (System.Int32)NoteSortOrder.CREATED;
            }
            else if (order.HasFlag(SortOrder.RecentlyUpdated))
            {
                noteFilter.Order = (System.Int32)NoteSortOrder.UPDATED;
            }
            else if (order.HasFlag(SortOrder.Relevance))
            {
                noteFilter.Order = (System.Int32)NoteSortOrder.RELEVANCE;
            }

            // "Normal" sort is ascending for titles, and descending for dates and relevance.
            bool sortAscending = order.HasFlag(SortOrder.Title) ? true : false;
            if (order.HasFlag(SortOrder.Reverse))
            {
                sortAscending = !sortAscending;
            }
            noteFilter.Ascending = sortAscending;

            if (notebook != null)
            {
                noteFilter.NotebookGuid = notebook.Guid;
            }

            // Set up context
            ENSessionFindNotesContext context = new ENSessionFindNotesContext();
            context.scopeNotebook = notebook;
            context.scope = scope;
            context.order = order;
            context.noteFilter = noteFilter;
            context.resultSpec = resultSpec;
            context.maxResults = maxResults;
            context.findMetadataResults = new List<NoteMetadata>();
            context.requiresLocalMerge = requiresLocalMerge;
            context.sortAscending = sortAscending;

            // If we have a scope notebook, we already know what notebook the results will appear in.
            // If we don't have a scope notebook, then we need to query for all the notebooks to determine
            // where to search.
            if (context.scopeNotebook == null)
            {
                context = FindNotes_ListNotebooks(context);
                return context.results;
            }

            context = FindNotes_FindInPersonalScope(context);
            return context.results;
        }

		public ENCollection FindNotesForCOM(ENNoteSearch noteSearch,  ENNotebook notebook, SearchScope scope, SortOrder order, int maxResults)
		{
            System.Windows.Forms.MessageBox.Show("Finding...", "Debug", System.Windows.Forms.MessageBoxButtons.OK);
            
            List<ENSessionFindNotesResult> results = ENSession.SharedSession.FindNotes(noteSearch, notebook, scope, order, maxResults);

            System.Windows.Forms.MessageBox.Show(results.Count.ToString(),"Debug",System.Windows.Forms.MessageBoxButtons.OK);

			ENCollection comResults = new ENCollection();
            foreach (ENSessionFindNotesResult result in results)
            {
                object tempvar = result;
                comResults.Add(ref tempvar);
            }
			return comResults;
		}

		private ENSessionFindNotesContext FindNotes_ListNotebooks(ENSessionFindNotesContext context)
		{
			// XXX: We do the full listNotebooks operation here, which is overkill in all situations,
			// and could wind us up doing a bunch of extra work. Optimization is to only look at -listNotebooks
			// if we're personal scope, and -listLinkedNotebooks for linked and business, without ever
			// authenticating to other note stores. But it's also true that a findNotes may well be followed
			// quickly by a fetchNote(s), which is going to require the full notebook list anyway, and by then
			// it'll be cached.

			List<ENNotebook> notebooks = ListNotebooks();
			if (notebooks != null)
			{
				context.allNotebooks = notebooks;
				context = FindNotes_FindInPersonalScope(context);
			}
			else
			{
				string err = "findNotes: Failed to list notebooks.";
				ENSDKLogger.ENSDKLogError(err);
				throw new Exception(err);
			}

			return context;
		}

		private ENSessionFindNotesContext FindNotes_FindInPersonalScope(ENSessionFindNotesContext context)
		{
			bool skipPersonalScope = false;
			// Skip the personal scope if the scope notebook isn't personal, or if the scope
			// flag doesn't include personal.
			if (context.scopeNotebook != null)
			{
				// If the scope notebook isn't personal, skip personal.
				if (context.scopeNotebook.IsLinked)
				{
					skipPersonalScope = true;
				}
			}
			else if (!(context.scope.HasFlag(SearchScope.Personal)))
			{
				// If the caller didn't request personal scope.
				skipPersonalScope = true;
			}
			else if (AppNotebookIsLinked)
			{
				// If we know this is an app notebook scoped app, and we know the app notebook is not personal.
				skipPersonalScope = true;
			}

			// If we're skipping personal scope, proceed directly to business scope.
			if (skipPersonalScope)
			{
				context = FindNotes_FindInBusinessScope(context);
				return context;
			}

			try
			{
				List<NoteMetadata> notesMetadataList = PrimaryNoteStore.FindNotesMetadata(context.noteFilter, context.maxResults, context.resultSpec);
				context.findMetadataResults.AddRange(notesMetadataList);
				context = FindNotes_FindInBusinessScope(context);
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("findNotes: Failed to find notes (personal). {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}

			return context;
		}

		private ENSessionFindNotesContext FindNotes_FindInBusinessScope(ENSessionFindNotesContext context)
		{
			// Skip the business scope if the user is not a business user, or the scope notebook
			// is not a business notebook, or the business scope is not included.
			if (!IsBusinessUser || (context.scopeNotebook != null && !context.scopeNotebook.IsBusinessNotebook) || (context.scopeNotebook == null && !(context.scope.HasFlag(SearchScope.Business))))
			{
				context = FindNotes_FindInLinkedScope(context);
				return context;
			}

			try
			{
				List<NoteMetadata> notesMetadataList = BusinessNoteStore.FindNotesMetadata(context.noteFilter, context.maxResults, context.resultSpec);
				context.findMetadataResults.AddRange(notesMetadataList);

				// Remember which note guids came from the business. We'll use this later to
				// determine if we're worried about an inability to map back to notebooks.
				context.resultGuidsFromBusiness = notesMetadataList.Select((x) => x.Guid).Distinct();

				context = FindNotes_FindInLinkedScope(context);
			}
			catch (Evernote.EDAM.Error.EDAMUserException ex)
			{
				if (ex.ErrorCode == Evernote.EDAM.Error.EDAMErrorCode.PERMISSION_DENIED)
				{
					// This is a business user, but apparently has an app notebook restriction that's
					// not in the business. Go look in linked scope.
					context = FindNotes_FindInLinkedScope(context);
				}
				else
				{
					ENSDKLogger.ENSDKLogError(string.Format("findNotes: Failed to find notes (business). {0}", ex.Message));
					throw new Exception(ex.Message, ex.InnerException);
				}
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("findNotes: Failed to find notes (business). {0}", ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}

			return context;
		}

		private ENSessionFindNotesContext FindNotes_FindInLinkedScope(ENSessionFindNotesContext context)
		{
			// Skip linked scope if scope notebook is not a personal linked notebook, or if the
			// linked scope is not included.
			if (context.scopeNotebook != null)
			{
				if (!context.scopeNotebook.IsLinked || !context.scopeNotebook.IsBusinessNotebook)
				{
					context = FindNotes_ProcessResults(context);
					return context;
				}
			}
			else if (!(context.scope.HasFlag(SearchScope.PersonalLinked)))
			{
				context = FindNotes_ProcessResults(context);
				return context;
			}

			// Build a list of all the linked notebooks that we need to run the search against.
			context.linkedNotebooksToSearch = new List<ENNotebook>();
			if (context.scopeNotebook != null)
			{
				context.linkedNotebooksToSearch.Add(context.scopeNotebook);
			}
			else
			{
				foreach (ENNotebook notebook in context.allNotebooks)
				{
                    if (notebook.IsLinked && !notebook.IsBusinessNotebook)
					{
						context.linkedNotebooksToSearch.Add(notebook);
					}
				}
			}

			context = FindNotes_NextFindInLinkedScope(context);
			return context;
		}

		private ENSessionFindNotesContext FindNotes_NextFindInLinkedScope(ENSessionFindNotesContext context)
		{
			if (context.linkedNotebooksToSearch.Count == 0)
			{
				context = FindNotes_ProcessResults(context);
				return context;
			}

			// Pull the first notebook off the list of pending linked notebooks.
			ENNotebook notebook = context.linkedNotebooksToSearch[0];
			context.linkedNotebooksToSearch.RemoveAt(0);

			ENNoteStoreClient noteStore = NoteStoreForLinkedNotebook(notebook.LinkedNotebook);
			NoteFilter notefilter = context.noteFilter;
			if (notebook.IsJoinedPublic)
			{
				// https://dev.evernote.com/doc/reference/NoteStore.html#Fn_NoteStore_findNotesMetadata
				// to search joined public notebook, the auth token can be nil, but notebookGuid must be set
				notefilter.NotebookGuid = notebook.Guid;
			}
			try
			{
				List<NoteMetadata> notesMetadataList = noteStore.FindNotesMetadata(context.noteFilter, context.maxResults, context.resultSpec);
                if (notesMetadataList != null)
                {
                    context.findMetadataResults.AddRange(notesMetadataList);
                }
				// Do it again with the next linked notebook in the list.
				context = FindNotes_NextFindInLinkedScope(context);
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("findNotes: Failed to find notes (linked).  Notebook = {0}; {1}", notebook, ex.Message));
				throw new Exception(ex.Message, ex.InnerException);
			}

			return context;
		}

		private ENSessionFindNotesContext FindNotes_ProcessResults(ENSessionFindNotesContext context)
		{
			// OK, now we have a complete list of note refs objects. If we need to do a local sort, then do so.
			if (context.order.HasFlag(SortOrder.RecentlyCreated))
			{
				if (context.sortAscending)
				{
					context.findMetadataResults.Sort((x, y) => x.Created.CompareTo(y.Created));
				}
				else
				{
					context.findMetadataResults.Sort((x, y) => y.Created.CompareTo(x.Created));
				}
			}
			else if (context.order.HasFlag(SortOrder.RecentlyCreated))
			{
				if (context.sortAscending)
				{
					context.findMetadataResults.Sort((x, y) => x.Updated.CompareTo(y.Updated));
				}
				else
				{
					context.findMetadataResults.Sort((x, y) => y.Updated.CompareTo(x.Updated));
				}
			}
			else
			{
				if (context.sortAscending)
				{
					context.findMetadataResults.Sort((x, y) => x.Title.CompareTo(y.Title));
				}
				else
				{
					context.findMetadataResults.Sort((x, y) => y.Title.CompareTo(x.Title));
				}
			}

			// Prepare a dictionary of all notebooks by GUID so lookup below is fast.
			Dictionary<string, ENNotebook> notebooksByGuid = null;
			if (context.scopeNotebook == null)
			{
				notebooksByGuid = new Dictionary<string, ENNotebook>();
				foreach (ENNotebook notebook in context.allNotebooks)
				{
					notebooksByGuid[notebook.Guid] = notebook;
				}
			}

			// Turn the metadata list into a list of note refs.
			List<ENSessionFindNotesResult> findNotesResults = new List<ENSessionFindNotesResult>();

			foreach (NoteMetadata metadata in context.findMetadataResults)
			{
				ENNoteRef @ref = new ENNoteRef();
				@ref.Guid = metadata.Guid;

				// Figure out which notebook this note belongs to. (If there's a scope notebook, it always belongs to that one.)
				ENNotebook notebook = null;
				if (context.scopeNotebook != null)
				{
					notebook = context.scopeNotebook;
				}
				else
				{
					notebooksByGuid.TryGetValue(metadata.NotebookGuid, out notebook);
				}
				if (notebook == null)
				{
					// This is probably a business notebook that we haven't explicitly joined, so we don't have it in our list.
					if (!(context.resultGuidsFromBusiness.Contains(metadata.Guid)))
					{
						// Oh, it's not from the business. We really can't find it. This is an error.
						ENSDKLogger.ENSDKLogError(string.Format("Found note metadata but can't determine owning notebook by guid. Metadata = {0}", metadata));
					}
					continue;
				}

				if (notebook.IsBusinessNotebook)
				{
					@ref.Type = ENNoteRef.ENNoteRefType.TypeBusiness;
					@ref.LinkedNotebook = ENLinkedNotebookRef.LinkedNotebookRefFromLinkedNotebook(notebook.LinkedNotebook);
				}
				else if (notebook.IsLinked)
				{
					@ref.Type = ENNoteRef.ENNoteRefType.TypeShared;
					@ref.LinkedNotebook = ENLinkedNotebookRef.LinkedNotebookRefFromLinkedNotebook(notebook.LinkedNotebook);
				}
				else
				{
					@ref.Type = ENNoteRef.ENNoteRefType.TypePersonal;
				}

				ENSessionFindNotesResult result = new ENSessionFindNotesResult();
				result.NoteRef = @ref;
				result.Notebook = notebook;
				result.Title = metadata.Title;
				result.Created = metadata.Created.ToDateTime();
				result.Updated = metadata.Updated.ToDateTime();

				findNotesResults.Add(result);

				// If the caller specified a max result count, and we've reached it, then stop fixing up
				// results here.
				if (context.maxResults > 0 && findNotesResults.Count > context.maxResults)
				{
					break;
				}
			}

			context.results = findNotesResults;
			return context;
		}

#endregion

#region DownloadNote

		public ENNote DownloadNote(ENNoteRef noteRef)
		{
			if (noteRef == null)
			{
				ENSDKLogger.ENSDKLogError("noteRef parameter is required to get download note");
				throw new ENInvalidDataException();
			}

			if (!IsAuthenticated)
			{
				throw new ENAuthExpiredException();
			}

			// Find the note store client that works with this note.
			ENNoteStoreClient noteStore = NoteStoreForNoteRef(noteRef);

			// Fetch by guid. Always get the content and resources.
			try
			{
				Note note = noteStore.GetNote(noteRef.Guid, true, true, false, false);
				// Create an ENNote from the EDAMNote.
                if (ENSessionInterfaceType == InterfaceType.Advanced)
                {
                    return new ENNoteAdvanced(note);
                } else {
                    return new ENNote(note);
                }
			}
			catch (Exception)
			{
				return null;
			}
		}

#endregion

#region DownloadThumbnailForNote

		public byte[] DownloadThumbnailForNote(ENNoteRef noteRef, int maxDimension = 0)
		{
			if (noteRef == null)
			{
				ENSDKLogger.ENSDKLogError("noteRef parameter is required to get download thumbnail");
				throw new ENInvalidDataException();
			}

			if (!IsAuthenticated)
			{
				throw new ENAuthExpiredException();
			}

			// Clamp the maxDimension. Let 0 through as a sentinel for unspecified, and if the value is
			// already greater than the max we provide, then remove the parameter.
			if (maxDimension >= 300)
			{
				maxDimension = 0;
			}

			// Get the info we need for this note ref, then construct a standard request for the thumbnail.
			string authToken = AuthenticationTokenForNoteRef(noteRef);
			string shardId = ShardIdForNoteRef(noteRef);

			if (authToken == null || shardId == null)
			{
				return null;
			}

			// Only append the size param if we are explicitly providing one.
			string sizeParam = string.Empty;
			if (maxDimension > 0)
			{
				sizeParam = string.Format("?size={0}", maxDimension);
			}

			try
			{
				// Create a request using a URL that can receive a post. 
				WebRequest request = WebRequest.Create(string.Format("https://{0}/shard/{1}/thm/note/{2}{3}", SessionHost, shardId, noteRef.Guid, sizeParam));
				// Set the Method property of the request to POST.
				request.Method = "POST";
				// Create POST data and convert it to a byte array.
				string postData = "auth=" + authToken;
				byte[] byteArray = Encoding.Default.GetBytes(postData);
				// Set the ContentType property of the WebRequest.
				request.ContentType = "application/x-www-form-urlencoded";
				// Set the ContentLength property of the WebRequest.
				request.ContentLength = byteArray.Length;
				// Get the request stream.
				Stream dataStream = request.GetRequestStream();
				// Write the data to the request stream.
				dataStream.Write(byteArray, 0, byteArray.Length);
				// Close the Stream object.
				dataStream.Close();
				// Get the response.
				WebResponse response = request.GetResponse();
				// Get the stream containing content returned by the server.
				dataStream = response.GetResponseStream();
				// Open the stream using a StreamReader for easy access.
				StreamReader reader = new StreamReader(dataStream, Encoding.Default);
				// Read the content.
				string responseFromServer = reader.ReadToEnd();
				// Translate to byte array.
				byte[] imageBytes = Encoding.Default.GetBytes(responseFromServer);
				// Clean up the streams.
				reader.Close();
				dataStream.Close();
				response.Close();
				return imageBytes;
			}
			catch (Exception)
			{
				return null;
			}
		}

#endregion

#region Credential store

        private ENCredentialStore CredentialStore()
        {
            ENCredentialStore store = (ENCredentialStore)Preferences.ObjectForKey(ENSessionPreferencesCredentialStore);
            if (store == null)
            {
                store = new ENCredentialStore();
            }
            return store;
        }

		private ENCredentials CredentialsForHost(string host)
		{
			return CredentialStore().CredentialsForHost(host);
		}

		private void AddCredentials(ENCredentials credentials)
		{
			ENCredentialStore store = CredentialStore();
			store.AddCredentials(credentials);
			SaveCredentialStore(store);
		}

		private void SaveCredentialStore(ENCredentialStore credentialStore)
		{
			Preferences.SetObject(credentialStore, ENSessionPreferencesCredentialStore);
		}
#endregion

#region Credentials and Auth

		private ENCredentials PrimaryCredentials()
		{
			//XXX: Is here a good place to check for no credentials and trigger an unauthed state?
			return CredentialsForHost(SessionHost);
		}

		private AuthenticationResult ValidBusinessAuthenticationResult()
		{
			AuthenticationResult auth = AuthCache.AuthenticationResultForBusiness();
			if (auth == null)
			{
				auth = UserStore.AuthenticateToBusiness();
				AuthCache.SetAuthenticationResultForBusiness(auth);
			}
			return auth;
		}

#endregion

#region Store clients
		private ENUserStoreClient _UserStore;
		private ENUserStoreClient UserStore
		{
			get
			{
				if (_UserStore == null && PrimaryAuthenticationToken != null)
				{
					_UserStore = new ENUserStoreClient(UserStoreUrl(), PrimaryAuthenticationToken);
				}
				return _UserStore;
			}
			set
			{
				_UserStore = value;
			}
		}

		// Retrive an appropriate note store client to perform API operations with:
		// - The primary note store client is valid for all personal notebooks, and can also be used to authenticate with
		//   shared notebooks.
		// - The business note store client will only be non-nil if the authenticated user is a member of a business. With
		//   it, you can access the business's notebooks.
		// - Every linked notebook requires its own note store client instance to access.
		private ENNoteStoreClient _primaryNoteStore;
		internal ENNoteStoreClient PrimaryNoteStore
		{
			get
			{
				if (_primaryNoteStore == null)
				{
					if (DeveloperToken != null)
					{
						_primaryNoteStore = ENNoteStoreClient.NoteStoreClient(NoteStoreUrl, DeveloperToken);
					}
					else
					{
						ENCredentials credentials = PrimaryCredentials();
						if (credentials != null)
						{
							_primaryNoteStore = ENNoteStoreClient.NoteStoreClient(credentials.NoteStoreUrl, credentials.AuthenticationToken);
						}
					}
				}
				return _primaryNoteStore;
			}
			set
			{
				_primaryNoteStore = value;
			}
		}

		private ENNoteStoreClient _businessNoteStore;
        internal ENNoteStoreClient BusinessNoteStore
        {
            get
            {
                if (_businessNoteStore == null && IsBusinessUser)
                {
                    ENBusinessNoteStoreClient client = (ENBusinessNoteStoreClient)ENBusinessNoteStoreClient.NoteStoreClientForBusiness();
                    client.DelegateObj = this;
                    _businessNoteStore = client;
                }
                return _businessNoteStore;
            }
            set
            {
                _businessNoteStore = value;
            }
        }

        private string _businessShardId;
        private string BusinessShardId
        {
            get
            {
                if (_businessShardId == null)
                {
                    string storeUrl = BusinessNoteStore.NoteStoreUrl();
                    int startPos = storeUrl.IndexOf("shard/s");
                    int endPos = storeUrl.IndexOf("/", startPos + 6);
                    _businessShardId = storeUrl.Substring(startPos + 6, endPos - startPos - 6);
                }
                return _businessShardId;
            }
        }

        internal ENNoteStoreClient NoteStoreForLinkedNotebook(LinkedNotebook linkedNotebook)
        {
            ENLinkedNotebookRef linkedNotebookRef = ENLinkedNotebookRef.LinkedNotebookRefFromLinkedNotebook(linkedNotebook);
            ENLinkedNoteStoreClient linkedClient = (ENLinkedNoteStoreClient)ENLinkedNoteStoreClient.NoteStoreClientForLinkedNotebookRef(linkedNotebookRef);
            linkedClient.DelegateObj = (ENLinkedNoteStoreClient.IENLinkedNoteStoreClient)this;
            return linkedClient;
        }

        internal ENNoteStoreClient NoteStoreForNoteRef(ENNoteRef noteRef)
        {
            if (noteRef.Type == ENNoteRef.ENNoteRefType.TypePersonal)
            {
                return PrimaryNoteStore;
            }
            else if (noteRef.Type == ENNoteRef.ENNoteRefType.TypeBusiness)
            {
                return BusinessNoteStore;
            }
            else if (noteRef.Type == ENNoteRef.ENNoteRefType.TypeShared)
            {
                ENLinkedNoteStoreClient linkedClient = (ENLinkedNoteStoreClient)ENLinkedNoteStoreClient.NoteStoreClientForLinkedNotebookRef(noteRef.LinkedNotebook);
                linkedClient.DelegateObj = (ENLinkedNoteStoreClient.IENLinkedNoteStoreClient)this;
                return linkedClient;
            }
            return null;
        }

		internal object NoteStoreForNotebook(ENNotebook notebook)
		{
			if (notebook.IsBusinessNotebook)
			{
				return BusinessNoteStore;
			}
			else if (notebook.IsLinked)
			{
				return NoteStoreForLinkedNotebook(notebook.LinkedNotebook);
			}
			else
			{
				return PrimaryNoteStore;
			}
		}

		internal string ShardIdForNoteRef(ENNoteRef noteRef)
		{
			if (noteRef.Type == ENNoteRef.ENNoteRefType.TypePersonal)
			{
				return EdamUser.ShardId;
			}
			else if (noteRef.Type == ENNoteRef.ENNoteRefType.TypeBusiness)
			{
				return BusinessShardId;
			}
			else if (noteRef.Type == ENNoteRef.ENNoteRefType.TypeShared)
			{
				return noteRef.LinkedNotebook.ShardId;
			}
			return null;
		}

		internal string AuthenticationTokenForNoteRef(ENNoteRef noteRef)
		{
			// Note that we may need to go over the wire to get a noncached token.

			string token = null;

			// Because this method is called from outside the normal exception handlers in the user/note
			// store objects, it requires protection from EDAM and Thrift exceptions.
			try
			{
				if (noteRef.Type == ENNoteRef.ENNoteRefType.TypePersonal)
				{
					token = PrimaryAuthenticationToken;
				}
				else if (noteRef.Type == ENNoteRef.ENNoteRefType.TypeBusiness)
				{
					token = ValidBusinessAuthenticationResult().AuthenticationToken;
				}
				else if (noteRef.Type == ENNoteRef.ENNoteRefType.TypeShared)
				{
					token = AuthenticationTokenForLinkedNotebookRef(noteRef.LinkedNotebook);
				}
			}
			catch (Exception ex)
			{
				ENSDKLogger.ENSDKLogError(string.Format("Caught exception getting auth token for note ref {0}: {1}", noteRef, ex.Message));
				token = null;
			}

			return token;
		}

#endregion

#region Preferences helpers

		private string CurrentProfileName()
		{
            var profile = Preferences.ObjectForKey(ENSessionPreferencesCurrentProfileName);
            return (profile != null) ? profile.ToString() : "";
		}

		private void SetCurrentProfileNameFromHost(string host)
		{
			string profileName = null;
			if (host == ENSessionBootstrapServerBaseURLStringUS)
			{
				profileName = ENConstants.ENBootstrapProfileNameInternational;
			}
			else if (host == ENSessionBootstrapServerBaseURLStringCN)
			{
				profileName = ENConstants.ENBootstrapProfileNameChina;
			}
			Preferences.SetObject(profileName, ENSessionPreferencesCurrentProfileName);
		}

		private string UserStoreUrl()
		{
			// If the host string includes an explict port (e.g., foo.bar.com:8080), use http. Otherwise https.
			// Use a simple regex to check for a colon and port number suffix.
			var matches = Regex.Matches(SessionHost, ".*:[0-9]+", RegexOptions.IgnoreCase);
			bool hasPort = matches.Count > 0;
			string scheme = hasPort ? "http" : "https";
			return string.Format("{0}://{1}/edam/user", scheme, SessionHost);
		}

#endregion

#region ENBusinessNoteStoreClientDelegate

		string ENBusinessNoteStoreClient.IENBusinessNoteStoreClient.AuthenticationTokenForBusinessStoreClient(ENBusinessNoteStoreClient client)
		{
			return this.AuthenticationTokenForBusinessStoreClient(client);
		}

		internal string AuthenticationTokenForBusinessStoreClient(ENBusinessNoteStoreClient client)
		{
			AuthenticationResult auth = ValidBusinessAuthenticationResult();
			return auth.AuthenticationToken;
		}

		string ENBusinessNoteStoreClient.IENBusinessNoteStoreClient.NoteStoreUrlForBusinessStoreClient(ENBusinessNoteStoreClient client)
		{
			return this.NoteStoreUrlForBusinessStoreClient(client);
		}

		internal string NoteStoreUrlForBusinessStoreClient(ENBusinessNoteStoreClient client)
		{
			AuthenticationResult auth = ValidBusinessAuthenticationResult();
			return auth.NoteStoreUrl;
		}

#endregion
#region ENLinkedNoteStoreClientDelegate

		string ENLinkedNoteStoreClient.IENLinkedNoteStoreClient.AuthenticationTokenForLinkedNotebookRef(ENLinkedNotebookRef linkedNotebookRef)
		{
			return this.AuthenticationTokenForLinkedNotebookRef(linkedNotebookRef);
		}

		internal string AuthenticationTokenForLinkedNotebookRef(ENLinkedNotebookRef linkedNotebookRef)
		{
            // Use null token for joined public notebook.
	        if (linkedNotebookRef.SharedNotebookGlobalId == null) {
	            return null;
	        } 

			// See if we have auth data already for this notebook.
			AuthenticationResult auth = AuthCache.AuthenticationResultForLinkedNotebook(linkedNotebookRef.Guid);
			if (auth == null)
			{
				// Create a temporary note store client for the linked note store, with our primary auth token,
				// in order to authenticate to the shared notebook.
				ENNoteStoreClient linkedNoteStore = ENNoteStoreClient.NoteStoreClient(linkedNotebookRef.NoteStoreUrl, PrimaryAuthenticationToken);
				auth = linkedNoteStore.AuthenticateToSharedNotebook(linkedNotebookRef.SharedNotebookGlobalId);
				AuthCache.SetAuthenticationResultForLinkedNotebook(auth, linkedNotebookRef.Guid);
			}
			return auth.AuthenticationToken;
		}

#endregion

#region Default logger

		public class ENSessionDefaultLogger : ENSDKLogger.ENSDKLogging
		{
			public void EvernoteLogInfoString(string str)
			{
				Console.WriteLine("EvernoteSDK: " + str);
			}

			public void EvernoteLogErrorString(string str)
			{
				Console.WriteLine("EvernoteSDK ERROR: " + str);
			}
		}
#endregion

#region Bit twiddling macros

		private static int EN_FLAG_SET(object v, object f)
		{
            return Convert.ToInt32(Convert.ToInt32(v) | Convert.ToInt32(f));
		}

		private static int EN_FLAG_CLEAR(object v, object f)
		{
            return Convert.ToInt32(Convert.ToInt32(v) ^ Convert.ToInt32(f));
		}

#endregion

	}


	public class ENSessionFindNotesResult
	{
		public ENNoteRef NoteRef {get; set;}
		public ENNotebook Notebook {get; set;}
		public string Title {get; set;}
		public DateTime Created {get; set;}
		public DateTime Updated {get; set;}
		internal int UpdateSequenceNumber {get; set;}
	}

}