using System.Collections.Generic;

namespace EvernoteSDK
{
	public class ENNoteContent
	{

		private string Emml {get; set;}


		internal ENNoteContent()
		{
		}

		internal ENNoteContent(string enml)
		{
			Emml = enml;
		}

		public static ENNoteContent NoteContentWithString(string contentString)
		{
			return new ENPlaintextNoteContent(contentString);
		}

		public static ENNoteContent NoteContentWithSanitizedHTML(string html)
		{
			return new ENHTMLNoteContent(html);
		}

		internal static ENNoteContent NoteContentWithENML(string enml)
		{
			return new ENNoteContent(enml);
		}

		internal virtual string EnmlWithResources(List<ENResource> resources)
		{
			// If we are using precooked ENML, we assume the resources have already been validly written
			// into the document.
			return Emml;
		}

		internal string Enml()
		{
			return EnmlWithResources(null);
		}

	}

}