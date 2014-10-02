

namespace EvernoteSDK
{
	namespace Advanced
	{
		internal static class String_EDAMNullAdditions
		{
			// Extension methods on 'string' to deal with Evernote API data.

			public static bool EnIsEqualToStringWithEmptyEqualToNull(this string s, string stringToCompare)
			{
				if (string.IsNullOrEmpty(s))
				{
					return string.IsNullOrEmpty(stringToCompare);
				}
				else
				{
					return string.Equals(s, stringToCompare);
				}
			}

		}
	}

}