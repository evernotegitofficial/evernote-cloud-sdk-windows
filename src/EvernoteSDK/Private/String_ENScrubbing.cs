
using System.Text;
using System.Text.RegularExpressions;

namespace EvernoteSDK
{
	internal static class String_ENScrubbing
	{

		public static string EnScrubUsingRegex(this string s, string regexPattern, int minLength, int maxLength, string invalidCharacterReplacement)
		{
			if (s.Length < minLength)
			{
				return null;
			}

			if (s.Length > maxLength)
			{
				s = s.Substring(0, maxLength);
			}
			Regex regex = new Regex(regexPattern);
			MatchCollection matches = regex.Matches(s);
			if (matches.Count == 0)
			{
				StringBuilder newString = new StringBuilder(s.Length);
				for (int i = 0; i <= s.Length; i++)
				{
					string oneCharSubString = s.Substring(i, 1);
					matches = regex.Matches(oneCharSubString);
					if (matches.Count > 0)
					{
						newString.Append(oneCharSubString);
					}
					else if (invalidCharacterReplacement != null)
					{
						newString.Append(invalidCharacterReplacement);
					}
				}
				s = newString.ToString();
			}

			if (s.Length < minLength)
			{
				return null;
			}

			return s;
		}

		public static string EnScrubUsingRegex(this string s, string regexPattern, int minLength, int maxLength)
		{
			return s.EnScrubUsingRegex(regexPattern, minLength, maxLength, null);
		}


	}


}