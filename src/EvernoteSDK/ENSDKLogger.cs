
namespace EvernoteSDK
{
	internal class ENSDKLogger
	{

		// The SDK sends some info and error messages to a log output. By default, these will just use Debug.WriteLine.
		// You can plug into your app's own logging infrastructure if you wish by setting the shared ENSession's
		// logger property to any object that implements this simple interface. You can also suppress output
		// entirely by setting the property to null.

		public interface ENSDKLogging
		{
			void EvernoteLogInfoString(string str);
			void EvernoteLogErrorString(string str);
		}


		// These are shortcut methods for calling the logging routines.

		public static void ENSDKLogInfo(string evernoteLogInfoString)
		{
			ENSession.SharedSession.Logger.EvernoteLogInfoString(evernoteLogInfoString);
		}

		public static void ENSDKLogError(string evernoteLogErrorString)
		{
			ENSession.SharedSession.Logger.EvernoteLogErrorString(evernoteLogErrorString);
		}

	}

}