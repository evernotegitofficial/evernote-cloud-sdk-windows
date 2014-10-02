
using EvernoteSDK.Advanced;

namespace EvernoteSDK
{
	public class ENLinkedNoteStoreClient : ENNoteStoreClient
	{
		internal interface IENLinkedNoteStoreClient
		{
			string AuthenticationTokenForLinkedNotebookRef(ENLinkedNotebookRef linkedNotebookRef);
		}

		internal IENLinkedNoteStoreClient DelegateObj {get; set;}
		private ENLinkedNotebookRef LinkedNotebookRef {get; set;}

		protected internal override string NoteStoreUrl()
		{
			return LinkedNotebookRef.NoteStoreUrl;
		}

		protected internal override string AuthenticationToken()
		{
			return DelegateObj.AuthenticationTokenForLinkedNotebookRef(LinkedNotebookRef);
		}

		internal static object NoteStoreClientForLinkedNotebookRef(ENLinkedNotebookRef linkedNotebookRef)
		{
			ENLinkedNoteStoreClient client = new ENLinkedNoteStoreClient();
			client.LinkedNotebookRef = linkedNotebookRef;
			return client;
		}

	}

}