using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using Sgml;

namespace EvernoteSDK
{
	public class ENHTMLtoENMLConverter
	{
		//
		// Input properties - to be set by user if desired.
		//
		// If the user's HTML contains references to external stylesheets with relative URL references, 
		// the user must pass the Base URL of the stylesheets so that these relative references can be resolved
		public string BaseUrlForCSSLinks {get; set;}
		// If the user wants to supply their own XSLT file, the full pathname to that file is passed. 
		// If no file is supplied, the internal HTML2ENML XSLT transform is used.
		public string XSLTPath {get; set;}
		// Set to True to validate the generated ENML against the Evernote ENML DTD, or false to skip validation.  (Default is True)
		public bool Validate {get; set;}

		//
		// Output properties - set by the routine.
		//
		// If DTD validation was performed (i.e. Validate = true), this reports whether the ENML is valid or not.
		public bool ENMLIsValid {get; set;}
		// If the ENML is not valid, this contains a list of the validation errors found.
		public List<ValidationError> ValidationErrorList {get; set;}

		public ENHTMLtoENMLConverter()
		{
			// Set default values.
			BaseUrlForCSSLinks = string.Empty;
			XSLTPath = string.Empty;
			Validate = true;
			ENMLIsValid = true;
			ValidationErrorList = new List<ValidationError>();
		}

		public string ENMLFromHTMLContent(string htmlContent)
		{
            // Make sure we have an XHTML header or the XSLT below won't work rght.
            if (!htmlContent.Contains("<html"))
            {
                htmlContent = "<html>" + htmlContent + "</html>";
            }
            if (htmlContent.Contains("<html>"))
            {
                htmlContent = htmlContent.Replace("<html>", "<html xmlns=\"http://www.w3.org/1999/xhtml\">");
            }

			// Inline any external CSS Stylesheets.
			string cssResolvedContent = ResolveCSSLinks(htmlContent, BaseUrlForCSSLinks);
			cssResolvedContent = cssResolvedContent.Replace("::", "");
			PreMailer.Net.InlineResult inlinedContent = PreMailer.Net.PreMailer.MoveCssInline(cssResolvedContent, true);
			string inlinedHtml = FixDoctypeAndXhtml(inlinedContent.Html);
			StringReader reader = new StringReader(inlinedHtml);

			// Convert the HTML to an XML document.
			SgmlReader sgmlReader = new SgmlReader();
			sgmlReader.DocType = "HTML";
			sgmlReader.WhitespaceHandling = WhitespaceHandling.All;
			sgmlReader.CaseFolding = Sgml.CaseFolding.ToLower;
			sgmlReader.IgnoreDtd = true;
			sgmlReader.InputStream = reader;
			XmlDocument document = new XmlDocument();
			document.PreserveWhitespace = true;
			document.XmlResolver = null;
			document.Load(sgmlReader);

			// Transform it to ENML using the XSLT.
			XslCompiledTransform transform = new XslCompiledTransform();
			StringBuilder resultString = new StringBuilder();

			XmlWriter writer = XmlWriter.Create(resultString);
            if (XSLTPath.Length > 0)
			{
				transform.Load(XSLTPath);
			}
			else
			{
				transform.Load(typeof(HTML2ENML));
			}
			transform.Transform(document, writer);

			if (Validate)
			{
				// Write the Evernote ENML DTD to a temporary file.
				// An external DTD is required because .NET prohibits !ENTITY declarations in internal DTD.
				string tempPath = Path.GetTempPath();
                WriteResourceToFile("Private.Supporting_Files.enml2full.dtd", tempPath + "enml2full.dtd");
				// Add a reference to the external DTD file into the XML result string.
				string resultXml = resultString.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<!DOCTYPE en-note SYSTEM \"" + tempPath + "enml2full.dtd\">" + System.Environment.NewLine);
                // Convert the string to a memory stream for input to the XmlReader.                
                MemoryStream mstm = new MemoryStream(Encoding.Unicode.GetBytes(resultXml));
				// Set up and create the XmlReader for DTD validation.
				XmlReaderSettings settings = new XmlReaderSettings();
				settings.ValidationType = ValidationType.DTD;
				settings.DtdProcessing = DtdProcessing.Parse;
				settings.ValidationEventHandler += MyValidationEventHandler;
				XmlReader validatingReader = XmlReader.Create(mstm, settings);
				// Perform the validation.
				while (validatingReader.Read())
				{
				}
			}

            string returnString = resultString.ToString();
            returnString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">" + returnString.Substring(returnString.IndexOf("<en-note>"));
            return returnString;
		}

		public string ENMLFromContentsOfHTMLFile(string htmlFile)
		{
			string html = File.ReadAllText(htmlFile);
			// TODO: Add some code to catch errors like "File Not Found"
			return ENMLFromHTMLContent(html);
		}

		/// <summary>
		/// Replaces any &lt;link rel="stylesheet"&gt; tags with the actual css source.
		/// </summary>
		/// <param name="html">The html to be converted</param>
        /// <param name="baseUrl">In case the links are relative, this specifies their base url</param>
		/// <returns></returns>
		private static string ResolveCSSLinks(string html, string baseUrl = "")
		{

			Dictionary<string, string> linkedCssCache = new Dictionary<string, string>();

			//find link tags that are of type stylesheet
			var matches = Regex.Matches(html, "<link.*\\s+rel=.stylesheet..*/>", RegexOptions.IgnoreCase | RegexOptions.Multiline);

			foreach (Match match in matches)
			{
				//get href, we do this in a second step to allow the href attribute to appear before or after "rel" attribute
				var matchHref = Regex.Matches(match.Value, "<link.*\\s+href=[\"']([^\"]+)[\"'].*/>", RegexOptions.IgnoreCase | RegexOptions.Multiline);

				string href = null;
				if (matchHref[0].Groups[1].Value.StartsWith("http"))
				{
					href = matchHref[0].Groups[1].Value;
				}
				else
				{
					href = baseUrl.TrimEnd('/') + "/" + matchHref[0].Groups[1].Value.TrimStart('/');
				}
				try
				{
					string css = null;
					if (!(linkedCssCache.ContainsKey(href)))
					{
						var webRequest = HttpWebRequest.Create(href);
						var webResponse = webRequest.GetResponse();
						using (Stream stream = webResponse.GetResponseStream())
						{
							StreamReader sr = new StreamReader(stream);
							css = RemovePremailerNetBrokenSelectorModifiers(sr.ReadToEnd());
							linkedCssCache.Add(href, css);
						}
					}
					else
					{
						css = linkedCssCache[href];
					}

					html = html.Replace(match.Value, string.Format("<style type=\"text/css\">{0}</style>", css));
				}
				catch
				{
				}
			}

			return html;
		}

		private static string RemovePremailerNetBrokenSelectorModifiers(string css)
		{
			//Remove browser specific selector modifiers
			css = Regex.Replace(css, "[:]+-.*{.*}", string.Empty, RegexOptions.IgnoreCase);

			//Some complex media queries cause CSQuery to blow up
			css = Regex.Replace(css, "@media\\s.*{.*}", string.Empty, RegexOptions.IgnoreCase);
			return css;
		}

		private static string FixDoctypeAndXhtml(string html)
		{
			html = html.Replace("<!DOCTYPE html \"", "<!DOCTYPE html PUBLIC \"");
			html = html.Replace("<!DOCTYPE html>", "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			if (!(html.Contains("<html xmlns=\"http://www.w3.org/1999/xhtml\"")))
			{
				html = html.Replace("http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\">");
			}
			return html;
		}

		public void WriteResourceToFile(string resourceName, string fileName)
		{
			using (var resource = GetType().Assembly.GetManifestResourceStream(GetType().Assembly.FullName.Split(',')[0] + "." + resourceName))
            {
				using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
				{
					resource.CopyTo(file);
				}
			}
		}

		public void MyValidationEventHandler(object sender, ValidationEventArgs args)
		{
			ENMLIsValid = false;
			ValidationErrorList.Add(new ValidationError(args.Exception.Message, args.Exception.LineNumber, args.Exception.LinePosition));
		}

	}

    public class StringWriterWithEncoding : StringWriter
    {
        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding) : base(sb)
        {
            m_Encoding = encoding;
        }
        private readonly Encoding m_Encoding;
        public override Encoding Encoding
        {
            get
            {
                return m_Encoding;
            }
        }
    } 

}