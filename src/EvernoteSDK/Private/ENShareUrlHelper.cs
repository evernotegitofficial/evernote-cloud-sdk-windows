using System;
using System.Text.RegularExpressions;

namespace EvernoteSDK
{
	public class ENShareUrlHelper
	{

		internal static string ShareUrlString(string guid, string shardId, string shareKey, string serviceHost, string encodedAdditionalString)
		{
			// All of the standard pieces of information come from the service in string format. We need their underlying bytes/numeric
			// values to create a packed byte array. Get the correct values for each, and if any of them appear not to be in the expected
			// format, fall back to the long form share URL.

			// Shard ID to number
			int shardNumber = -1;
			if (shardId.Length > 1 && shardId.StartsWith("s"))
			{
				// Ignore the leading "s" character.
				string shardString = shardId.Substring(1);
				shardString = Regex.Replace(shardString, "[^0-9]+", string.Empty);
				if (shardString.Length < 1)
				{
					shardNumber = -1;
				}
				if (shardNumber > UInt16.MaxValue)
				{
					shardNumber = -1;
				}
			}

			// Note guid to UUID
			Guid noteUUID = new Guid(guid);

			// Share key to binary value. First truncate it to initial 16 characters (it's normally 32).
			byte[] shareKeyData = null;
			if (shareKey.Length >= 16)
			{
				shareKeyData = StringToByteArray(shareKey);
			}

			// Check that all our values appear valid. If not, return the old style share URL instead.
			if (shardNumber < 0 || noteUUID == new Guid() || shareKeyData == null)
			{
				return string.Format("https://{0}/shard/{1}/sh/{2}/{3}", serviceHost, shardId, guid, shareKey);
			}

			// Until this function is completed, for now just return the old style share URL.
			return string.Format("https://{0}/shard/{1}/sh/{2}/{3}", serviceHost, shardId, guid, shareKey);

			// TODO: Need to complete this function to produce the new "shortened" URL format

		}

        public static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[(int)((double)NumberChars / 2 - 1 + 1)];
            for (int i = 0; i <= NumberChars - 1; i += 2)
            {
                bytes[(int)((double)i / 2)] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

	}

}