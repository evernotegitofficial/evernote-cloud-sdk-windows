using System;
using System.Security.Cryptography;
using Evernote.EDAM.UserStore;
using EvernoteSDK.Advanced;

namespace EvernoteSDK
{
	[Serializable]
	public class ENCredentials
	{
		internal string Host {get; set;}
		internal string EdamUserId {get; set;}
		internal string NoteStoreUrl {get; set;}
		internal string WebApiUrlPrefix {get; set;}
		internal DateTime ExpirationDate {get; set;}

		[NonSerialized]
		private string _authenticationToken;
		internal string AuthenticationToken
		{
			get
			{
				try
				{
					string token = CipherUtility.Decrypt<AesManaged>(RegistryStore.GetValue(Host, EdamUserId), EdamUserId, Host);
					return token;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error getting authentication token from registry: " + ex.Message);
					return null;
				}
			}
			set
			{
				_authenticationToken = value;
			}
		}

		[NonSerialized]
		private ENRegistry _registryStore;
		private ENRegistry RegistryStore
		{
			get
			{
				if (_registryStore == null)
				{
					_registryStore = new ENRegistry();
				}
				return _registryStore;
			}
		}

		internal ENCredentials(string host, string edamUserId, string noteStoreUrl, string webApiUrlPrefix, string authenticationToken, DateTime expirationDate)
		{
			Host = host;
			EdamUserId = edamUserId;
			NoteStoreUrl = noteStoreUrl;
			WebApiUrlPrefix = webApiUrlPrefix;
			AuthenticationToken = authenticationToken;
			ExpirationDate = expirationDate;
		}

		internal ENCredentials(string host, AuthenticationResult authenticationResult)
		{
			Host = host;
			EdamUserId = authenticationResult.User.Id.ToString();
			NoteStoreUrl = authenticationResult.NoteStoreUrl;
			WebApiUrlPrefix = authenticationResult.WebApiUrlPrefix;
			AuthenticationToken = authenticationResult.AuthenticationToken;
			ExpirationDate = authenticationResult.Expiration.ToDateTime();
		}

		internal void SaveToRegistry()
		{
			// Auth token gets encrypted and saved to the registry.
			RegistryStore.SetValue(Host, EdamUserId, CipherUtility.Encrypt<AesManaged>(_authenticationToken, EdamUserId, Host));
		}

		internal void DeleteFromRegistry()
		{
			RegistryStore.DeleteValue(Host, EdamUserId);
		}

		internal bool AreValid()
		{
			// Not all credentials are guaranteed to have a valid expiration. If none is present,
			// then assume it's valid.
			if (ExpirationDate == new DateTime())
			{
				return true;
			}

			// Check the expiration date.
			if (DateTime.Now > ExpirationDate)
			{
				return false;
			}

			return true;
		}

	}

}