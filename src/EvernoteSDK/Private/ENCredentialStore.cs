using System;
using System.Collections.Generic;

namespace EvernoteSDK
{
	[Serializable]
	public class ENCredentialStore
	{

		// Permanent store of Evernote credentials.
		// Credentials are unique per (host,consumer key) tuple.

		private Dictionary<string, ENCredentials> Store {get; set;}

		internal ENCredentialStore()
		{
			Store = new Dictionary<string, ENCredentials>();
		}

		// Add credentials to the store.
		// Also saves the authentication token to the keychain.
		public void AddCredentials(ENCredentials credentials)
		{
			// Save auth token to registry.
			credentials.SaveToRegistry();

			// Add it to our host => credentials dictionary.
			Store[credentials.Host] = credentials;
		}

		// Look up the credentials for the given host.
		public ENCredentials CredentialsForHost(string host)
		{
			ENCredentials credentials = null;
            if (Store == null)
            {
                return null;
            }
			Store.TryGetValue(host, out credentials);
			if (credentials != null && !credentials.AreValid())
			{
				RemoveCredentials(credentials);
				return null;
			}

			return credentials;
		}

		// Remove credentials from the store.
		// Also deletes the credentials' auth token from the keychain.
		public void RemoveCredentials(ENCredentials credentials)
		{
			// Delete auth token from registry.
			credentials.DeleteFromRegistry();

			// Update user defaults.
			Store.Remove(credentials.Host);
		}

		// Remove all credentials from the store.
		// Also deletes the credentials' auth tokens from the keychain.
		public void ClearAllCredentials()
		{
			foreach (var entry in Store)
			{
				entry.Value.DeleteFromRegistry();
			}

			Store.Clear();
		}

	}

}