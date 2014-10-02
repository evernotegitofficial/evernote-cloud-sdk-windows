using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Evernote.EDAM.Type;

namespace EvernoteSDK
{
	namespace Advanced
	{
		public class ENMLWriter : XmlWriter
		{
			private XmlWriter writer;
			public Utf8StringWriter Contents;

			public ENMLWriter()
			{
				Contents = new Utf8StringWriter();
				writer = XmlWriter.Create(Contents);
				writer.WriteDocType("en-note", null, "http://xml.evernote.com/pub/enml2.dtd", null);
			}

			private static bool ValidateURLComponents(Uri url)
			{
				// TODO: Review this function - we don't really need some of this code in .NET
				if (url == null)
				{
					return false;
				}

				string scheme = url.Scheme;
				if (scheme.Contains("script"))
				{
					return false;
				}
				else if (scheme == "file")
				{
					return true;
				}
				else if (scheme == "x-apple-data-detectors")
				{
					return false;
				}
				else if (scheme == "tel")
				{
					return true;
				}

				bool result = true;
				scheme = url.GetComponents(UriComponents.Scheme, UriFormat.SafeUnescaped);
				if (!(scheme.EnIsEqualToStringWithEmptyEqualToNull(url.Scheme)))
				{
					Console.WriteLine("Scheme '%@' does not match scheme '%@'", scheme, url.Scheme);
					result = false;
				}

				string authority = url.GetComponents(UriComponents.StrongAuthority, UriFormat.SafeUnescaped);
				StringBuilder hrefAuthority = new StringBuilder();
				string user = null;
				string password = null;
				if (url.UserInfo != null && url.UserInfo.Length > 0)
				{
					user = url.UserInfo.Split(':').First();
					password = url.UserInfo.Split(':').Last();
				}
				if (user != null || password != null)
				{
					if (user != null)
					{
						hrefAuthority = hrefAuthority.Append(user);
					}
					if (password != null)
					{
						hrefAuthority.Append(":");
						hrefAuthority.Append(password);
					}
					hrefAuthority.Append("@");
				}

				if (url.Host != null)
				{
					hrefAuthority.Append(url.Host);
				}

				if (url.Port != 0)
				{
					hrefAuthority.Append(":");
					hrefAuthority.Append(url.Port.ToString());
				}

				if (!(authority.EnIsEqualToStringWithEmptyEqualToNull(hrefAuthority.ToString())))
				{
					Console.WriteLine("Authority '%@' does not match authority '%@'", authority, hrefAuthority);
					result = false;
				}

				string path = url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
				string urlPath = url.AbsolutePath;
				if (!(path.EnIsEqualToStringWithEmptyEqualToNull(urlPath)))
				{
					if (!(scheme.Equals("mailto", StringComparison.OrdinalIgnoreCase)))
					{
						string resourceSpecifier = url.Host + url.PathAndQuery;
						if (!(path.EnIsEqualToStringWithEmptyEqualToNull(resourceSpecifier)))
						{
							Console.WriteLine("Path '%@' does not match resource specifier '%@'", path, resourceSpecifier);
							result = false;
						}
					}
					else if (path.EndsWith("/"))
					{
						path = path.Remove(path.Length - 1);
						if (!(path.EnIsEqualToStringWithEmptyEqualToNull(urlPath)))
						{
							Console.WriteLine("Path '%@' does not match path '%@'", path, urlPath);
							result = false;
						}
					}
					else
					{
						Console.WriteLine("Path '%@' does not match path '%@'", path, urlPath);
						result = false;
					}
				}

				string query = url.GetComponents(UriComponents.Query, UriFormat.SafeUnescaped);
				if (!(query.EnIsEqualToStringWithEmptyEqualToNull(url.Query)))
				{
					Console.WriteLine("Query '%@' does not match query '%@'", query, url.Query);
					result = false;
				}

				string fragment = url.GetComponents(UriComponents.Fragment, UriFormat.SafeUnescaped);
				if (!(fragment.EnIsEqualToStringWithEmptyEqualToNull(url.Fragment)))
				{
					Console.WriteLine("Fragment '%@' does not match fragment '%@'", fragment, url.Fragment);
					result = false;
				}

				return result;
			}

			public static string EmptyNote()
			{
				ENMLWriter noteWriter = new ENMLWriter();
				noteWriter.WriteStartDocument();
				noteWriter.WriteEndDocument();
				return noteWriter.Contents.ToString();
			}

			private Dictionary<string, string> ValidateURLAttribute(string attributekey, Dictionary<string, string> attributes)
			{
				string urlString = attributes[attributekey];
				if (string.IsNullOrEmpty(urlString))
				{
					return attributes;
				}

				Uri url = new Uri(urlString);
				Dictionary<string, string> newAttributes = new Dictionary<string, string>(attributes);
				if (ValidateURLComponents(url))
				{
					newAttributes.Add(attributekey, url.AbsoluteUri);
				}
				else
				{
					ENSDKLogger.ENSDKLogError(string.Format("Unable to validate URL:{0} in attributes:{1}", urlString, attributes));
					newAttributes.Remove(attributekey);
				}
				return newAttributes;
			}

			public override WriteState WriteState
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public void WriteStartDocumentWithAttributes(Dictionary<string, string> attributes)
			{
				WriteStartElementWithAttributes(ENMLConstants.ENMLTagNote, attributes);
			}

			public override void WriteStartDocument()
			{
				WriteStartDocumentWithAttributes(null);
			}

			public override void WriteStartDocument(bool standalone)
			{
				throw new NotImplementedException();
			}

			public override void WriteEndDocument()
			{
				WriteEndElement(); // ENMLTagNote
				writer.WriteEndDocument();
                writer.Flush();
			}

			public override void WriteDocType(string name, string pubid, string sysid, string subset)
			{
				throw new NotImplementedException();
			}

			public void WriteElementWithAttributes(string element, Dictionary<string, string> attributes, string content)
			{
				WriteStartElementWithAttributes(element, attributes);
				WriteString(content);
				WriteEndElement();
			}

			public void WriteStartElementWithAttributes(string localName, Dictionary<string, string> attributes)
			{
				if (localName == "a")
				{
					Dictionary<string, string> newAttributes = new Dictionary<string, string>(attributes);
					Dictionary<string, string>.KeyCollection attributeKeys = attributes.Keys;
					foreach (var aKey in attributeKeys)
					{
						if (aKey.StartsWith("x-apple-"))
						{
							newAttributes.Remove(aKey);
						}
					}
					attributes = ValidateURLAttribute("href", newAttributes);
				}
				else if (localName == "img")
				{
					attributes = ValidateURLAttribute("src", attributes);
				}

				writer.WriteStartElement(localName);
				if (attributes != null)
				{
					foreach (string key in attributes.Keys)
					{
						writer.WriteAttributeString(key, attributes[key]);
					}
				}
			}

			public override void WriteStartElement(string prefix, string localName, string ns)
			{
				writer.WriteStartElement(localName);
			}
			public override void WriteEndElement()
			{
				writer.WriteEndElement();
			}
			public override void WriteFullEndElement()
			{
				throw new NotImplementedException();
			}
			public override void WriteStartAttribute(string prefix, string localName, string ns)
			{
				throw new NotImplementedException();
			}
			public override void WriteEndAttribute()
			{
				throw new NotImplementedException();
			}
			public override void WriteCData(string text)
			{
				throw new NotImplementedException();
			}
			public override void WriteComment(string text)
			{
				throw new NotImplementedException();
			}
			public override void WriteProcessingInstruction(string name, string text)
			{
				throw new NotImplementedException();
			}
			public override void WriteEntityRef(string name)
			{
				throw new NotImplementedException();
			}
			public override void WriteCharEntity(char ch)
			{
				throw new NotImplementedException();
			}
			public override void WriteWhitespace(string ws)
			{
				throw new NotImplementedException();
			}
			public override void WriteString(string text)
			{
				writer.WriteString(text);
			}
			public override void WriteSurrogateCharEntity(char lowChar, char highChar)
			{
				throw new NotImplementedException();
			}
			public override void WriteChars(char[] buffer, int index, int count)
			{
				throw new NotImplementedException();
			}
			public override void WriteRaw(char[] buffer, int index, int count)
			{
				throw new NotImplementedException();
			}
			public override void WriteRaw(string data)
			{
				throw new NotImplementedException();
			}
			public override void WriteBase64(byte[] buffer, int index, int count)
			{
				throw new NotImplementedException();
			}
			public override void Flush()
			{
				writer.Flush();
			}
			public override void Close()
			{
				writer.Close();
			}
			public override string LookupPrefix(string ns)
			{
				throw new NotImplementedException();
			}

			public void WriteResourceWithDataHash(byte[] dataHash, string mime, Dictionary<string, string> attributes)
			{
				Dictionary<string, string> mediaAttributes = new Dictionary<string, string>();
				if (attributes != null)
				{
					mediaAttributes = attributes;
				}
				if (string.IsNullOrEmpty(mime))
				{
					mime = ENMLConstants.ENMIMETypeOctetStream;
				}
				mediaAttributes.Add("type", mime);
				mediaAttributes.Add("hash", dataHash.EnlowercaseHexDigits());
				WriteElementWithAttributes(ENMLConstants.ENMLTagMedia, mediaAttributes, null);
			}

			public void WriteResource(Resource resource)
			{
				Data resourceData = resource.Data;
				WriteResourceWithDataHash(resourceData.BodyHash, resource.Mime, null);
			}

			public void WriteEncryptedInfo(ENEncryptedContentInfo encryptedInfo)
			{
				Dictionary<string, string> cryptAttributes = new Dictionary<string, string>();
				cryptAttributes.Add("cipher", encryptedInfo.Cipher);
				cryptAttributes.Add("length", encryptedInfo.KeyLength.ToString());
				string hint = encryptedInfo.Hint;
				if (hint != null)
				{
					cryptAttributes.Add("hint", encryptedInfo.Hint);
				}
				WriteElementWithAttributes(ENMLConstants.ENMLTagCrypt, cryptAttributes, encryptedInfo.CipherText);
			}

			public void WriteTodoWithCheckedState(bool checkedState)
			{
				Dictionary<string, string> attributes = new Dictionary<string, string>();
				if (checkedState)
				{
					attributes.Add("checked", "true");
				}
				WriteElementWithAttributes(ENMLConstants.ENMLTagTodo, attributes, null);
			}
		}

		public class Utf8StringWriter : StringWriter
		{
			public override Encoding Encoding
			{
				get
				{
					return System.Text.Encoding.UTF8;
				}
			}
		}
	}

}