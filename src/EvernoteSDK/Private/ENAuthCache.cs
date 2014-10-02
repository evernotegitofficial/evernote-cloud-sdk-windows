using System;
using System.Collections.Generic;
using Evernote.EDAM.UserStore;

namespace EvernoteSDK
{
	public class ENAuthCache
	{

		private class ENAuthCacheEntry
		{
			internal AuthenticationResult AuthResult {get; set;}
			internal DateTime CachedDate {get; set;}

			internal static ENAuthCacheEntry EntryWithResult(AuthenticationResult result)
			{
				if (result == null)
				{
					return null;
				}
				ENAuthCacheEntry entry = new ENAuthCacheEntry();
				entry.AuthResult = result;
				entry.CachedDate = DateTime.Now;
				return entry;
			}

			internal bool IsValid()
			{
				TimeSpan age = DateTime.Now.Subtract(CachedDate).Duration();
				long expirationAge = (AuthResult.Expiration - AuthResult.CurrentTime) / 1000;
				// We're okay if the token is within 90% of the expiration time.
				if (age.Ticks > (0.9 * expirationAge))
				{
					return false;
				}
				return true;
			}
		}

		private Dictionary<string, ENAuthCacheEntry> LinkedCache {get; set;}
		private ENAuthCacheEntry BusinessCache {get; set;}

		internal ENAuthCache()
		{
			LinkedCache = new Dictionary<string, ENAuthCacheEntry>();
		}

		internal void SetAuthenticationResultForLinkedNotebook(AuthenticationResult result, string guid)
		{
			if (result == null)
			{
				return;
			}

			ENAuthCacheEntry entry = ENAuthCacheEntry.EntryWithResult(result);
			LinkedCache[guid] = entry;
		}

		internal AuthenticationResult AuthenticationResultForLinkedNotebook(string guid)
		{
			AuthenticationResult result = null;
			ENAuthCacheEntry entry = null;
			LinkedCache.TryGetValue(guid, out entry);
			if (entry != null && !entry.IsValid())
			{
				// This auth result has already expired, so evict it.
				LinkedCache.Remove(guid);
				entry = null;
			}
			else if (entry != null)
			{
				result = entry.AuthResult;
			}
			return result;
		}

		internal void SetAuthenticationResultForBusiness(AuthenticationResult result)
		{
			if (result == null)
			{
				return;
			}

			ENAuthCacheEntry entry = ENAuthCacheEntry.EntryWithResult(result);
			BusinessCache = entry;
		}

		internal AuthenticationResult AuthenticationResultForBusiness()
		{
			AuthenticationResult result = null;
			ENAuthCacheEntry entry = BusinessCache;
			if (entry != null && !entry.IsValid())
			{
				// This auth result has already expired, so evict it.
				BusinessCache = null;
				entry = null;
			}
			if (entry != null)
			{
				result = entry.AuthResult;
			}
			return result;
		}

	}

}