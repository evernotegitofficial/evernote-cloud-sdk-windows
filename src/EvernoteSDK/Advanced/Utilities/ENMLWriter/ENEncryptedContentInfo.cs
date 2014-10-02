
namespace EvernoteSDK
{
	public class ENEncryptedContentInfo
	{

		public string Hint {get; set;}
		public string Cipher {get; set;}
		public int KeyLength {get; set;}
		public string CipherText {get; set;}

		public ENEncryptedContentInfo()
		{
			Cipher = "RC2";
			KeyLength = 64;
		}

	}

}