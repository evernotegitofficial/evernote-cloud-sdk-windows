
namespace EvernoteSDK
{
	public class ValidationError
	{

		public string Message {get; set;}
		public int LineNumber {get; set;}
		public int LinePosition {get; set;}

		public ValidationError(string errMessage, int errLineNumber, int errLinePosition)
		{
			Message = errMessage;
			LineNumber = errLineNumber;
			LinePosition = errLinePosition;
		}

	}

}