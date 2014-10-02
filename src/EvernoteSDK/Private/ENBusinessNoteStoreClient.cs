
using EvernoteSDK.Advanced;

namespace EvernoteSDK
{
	public class ENBusinessNoteStoreClient : ENNoteStoreClient
	{
		internal interface IENBusinessNoteStoreClient
		{
			string NoteStoreUrlForBusinessStoreClient(ENBusinessNoteStoreClient client);
			string AuthenticationTokenForBusinessStoreClient(ENBusinessNoteStoreClient client);
		}

		internal IENBusinessNoteStoreClient DelegateObj {get; set;}

		protected internal override string NoteStoreUrl()
		{
			return DelegateObj.NoteStoreUrlForBusinessStoreClient(this);
		}

		protected internal override string AuthenticationToken()
		{
			return DelegateObj.AuthenticationTokenForBusinessStoreClient(this);
		}

		internal static object NoteStoreClientForBusiness()
		{
			return new ENBusinessNoteStoreClient();
		}

	}

}