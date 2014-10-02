
namespace EvernoteSDK
{
	public class ENNoteContentForCOM
	{

		public ENNoteContent NoteContentWithString(string contentString)
		{
			return ENNoteContent.NoteContentWithString(contentString);
		}

		public ENNoteContent NoteContentWithSanitizedHTML(string html)
		{
			return ENNoteContent.NoteContentWithSanitizedHTML(html);
		}

	}

}