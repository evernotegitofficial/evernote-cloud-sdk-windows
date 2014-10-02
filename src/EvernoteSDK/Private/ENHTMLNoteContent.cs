using System.Collections.Generic;

namespace EvernoteSDK
{
	public class ENHTMLNoteContent : ENNoteContent
	{
		private string _html {get; set;}

		public ENHTMLNoteContent(string html)
		{
			_html = html;
		}

		internal override string EnmlWithResources(List<ENResource> resources)
		{
			// Doesn't handle resources (yet?)
			var converter = new ENHTMLtoENMLConverter();
			string enml = converter.ENMLFromHTMLContent(_html);
			return enml;
		}

	}

}