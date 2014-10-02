using System;
using System.Security.Cryptography;

namespace EvernoteSDK
{
	namespace Advanced
	{
		internal static class ByteArray_EvernoteSDK
		{
			// Extension methods on 'byte array' to deal with Evernote API data.

			// Compute and return the byte array's MD5 hash.
			public static byte[] Enmd5(this byte[] bytes)
			{
				MD5 md5Hash = MD5.Create();
				return md5Hash.ComputeHash(bytes);
			}

			public static string EnlowercaseHexDigits(this byte[] bytes)
			{
				return BitConverter.ToString(bytes).Replace("-", "").ToLower();
			}

		}
	}

}