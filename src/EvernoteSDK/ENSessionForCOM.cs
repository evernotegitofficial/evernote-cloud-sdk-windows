
namespace EvernoteSDK
{
	public class ENSessionForCOM
	{

		public void SetSharedSessionConsumerKey(string sessionConsumerKey, string sessionConsumerSecret, string sessionHost = null)
		{
			ENSession.SetSharedSessionConsumerKey(sessionConsumerKey, sessionConsumerSecret, sessionHost);
		}

        public void SetSharedSessionDeveloperToken(string sessionDeveloperToken, string sessionNoteStoreUrl)
        {
            ENSession.SetSharedSessionDeveloperToken(sessionDeveloperToken, sessionNoteStoreUrl);
        }

        public ENSession SharedSession()
        {
            return ENSession.SharedSession;
        }

	}

}