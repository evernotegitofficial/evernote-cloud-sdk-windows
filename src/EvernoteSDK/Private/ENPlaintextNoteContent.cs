using System;
using System.Collections.Generic;
using EvernoteSDK.Advanced;

namespace EvernoteSDK
{
	public class ENPlaintextNoteContent : ENNoteContent
	{
		private string _contentString {get; set;}

		public ENPlaintextNoteContent(string contentString)
		{
			_contentString = contentString;
		}

		internal override string EnmlWithResources(List<ENResource> resources)
		{
			// Wrap each line in a div. Empty lines get <br/>
			// From: http://dev.evernote.com/doc/articles/enml.php "representing plaintext notes"
			ENMLWriter writer = new ENMLWriter();
			writer.WriteStartDocument();
            //string[] lines = _contentString.Split("\\n".ToCharArray());
            string[] lines = _contentString.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None);
            foreach (string line in lines)
			{
				writer.WriteStartElement("div");
				if (line.Length == 0)
				{
					writer.WriteElementWithAttributes("br", null, null);
				}
				else
				{
					writer.WriteString(line);
				}
				writer.WriteEndElement();
			}
			foreach (ENResource resource in resources)
			{
				writer.WriteResourceWithDataHash(resource.DataHash, resource.MimeType, null);
			}
			writer.WriteEndDocument();
			writer.Flush();
			return writer.Contents.ToString();
		}

	}

}