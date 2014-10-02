using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Evernote.EDAM.Type;
using EvernoteSDK;

internal class ENMLtoHTMLConverter
{
    internal static string HTMLFromENMLContent(string content, List<Resource> resources)
    {
        string contentResult = null;
        try
        {
            const string tagEnd = "/>";
            int tagEndLength = tagEnd.Length;
            contentResult = content.Replace("\"", "'");
            contentResult = contentResult.Replace(Microsoft.VisualBasic.Strings.Chr(13).ToString(), " ");
            contentResult = contentResult.Replace(Microsoft.VisualBasic.Strings.Chr(10).ToString(), " ");
            contentResult = contentResult.Replace(Microsoft.VisualBasic.Strings.Chr(160).ToString(), "&nbsp;");
            contentResult = contentResult.Replace("en-note", "body");
            contentResult = contentResult.Replace("HREF=", "target=\"_blank\" HREF=");
            contentResult = contentResult.Replace("href=", "target=\"_blank\" href=");
            int mediaTagStart = contentResult.IndexOf("<en-media");
            while (mediaTagStart > 0)
            {
                int mediaTagEnd = contentResult.IndexOf(tagEnd, mediaTagStart);
                int mediaEndLength = 2;
                if (!(mediaTagEnd > 0))
                {
                    mediaTagEnd = contentResult.IndexOf("/en-media>", mediaTagStart);
                    mediaEndLength = 10;
                }
                string mediaString = contentResult.Substring(mediaTagStart, (mediaTagEnd - mediaTagStart) + mediaEndLength);
                if (mediaString.IndexOf("type='image/") > 0)
                {
                    string hashCode = GetImageAttribute(mediaString, "hash");
                    if (hashCode.Length > 0)
                    {
                        byte[] bodyHash = ConvertStringToByteArray(hashCode);
                        foreach (Resource resource in resources)
                        {
                            if (StructuralComparisons.StructuralEqualityComparer.Equals(bodyHash, resource.Data.BodyHash))
                            {
                                string resourceContent = Convert.ToBase64String(resource.Data.Body);
                                string height = GetImageAttribute(mediaString, "height");
                                string width = GetImageAttribute(mediaString, "width");
                                contentResult = contentResult.Insert(mediaTagEnd + mediaEndLength, String.Format("<img src=\"data:{0};base64,{1}\" height=\"{2}\" width=\"{3}\" />", resource.Mime, resourceContent, height, width));
                                break;
                            }
                        }
                    }
                }
                if (mediaTagEnd > 0)
                {
                    contentResult = contentResult.Remove(mediaTagStart, (mediaTagEnd - mediaTagStart) + mediaEndLength);
                }
                mediaTagStart = contentResult.IndexOf("<en-media");
            }

            int checkTagStart = contentResult.IndexOf("<en-todo");
            while (checkTagStart > 0)
            {
                int checkTagEnd = contentResult.IndexOf(tagEnd, checkTagStart);
                string checkString = contentResult.Substring(checkTagStart, (checkTagEnd - checkTagStart) + tagEndLength);
                bool @checked;
                if (checkString.IndexOf("checked='true'") > 0)
                    @checked = true;
                else
                    @checked = false;
                int checkValueEnd = contentResult.IndexOf("<", checkTagEnd + 1);
                string checkValue = contentResult.Substring(checkTagEnd + 2, (checkValueEnd - checkTagEnd) - 2);
                if (checkTagEnd > 0)
                {
                    contentResult = contentResult.Remove(checkTagStart, (checkValueEnd - checkTagStart) + tagEndLength - 2);
                    if (@checked)
                    {
                        contentResult = contentResult.Insert(checkTagStart, String.Format("<label><input type=\"checkbox\" name=\"checkbox\" checked=\"checked\" value=\"value\"> {0}</label>", checkValue));
                    }
                    else
                    {
                        contentResult = contentResult.Insert(checkTagStart, String.Format("<label><input type=\"checkbox\" name=\"checkbox\" disabled=\"disabled\" value=\"value\"> {0}</label>", checkValue));
                    }
                }
                checkTagStart = contentResult.IndexOf("<en-todo");
            }
        }
        catch (Exception)
        {
            throw;
        }

        return contentResult;
    }

    private static byte[] ConvertStringToByteArray(string stringToConvert)
    {
		int length = stringToConvert.Length;
		int upperBound = length / 2;
		if (length % 2 == 0)
		{
			upperBound -= 1;
		}
		else
		{
			stringToConvert = "0" + stringToConvert;
		}
		byte[] bytes = new byte[upperBound + 1];
		for (int i = 0; i <= upperBound; i++)
		{
			bytes[i] = Convert.ToByte(stringToConvert.Substring(i * 2, 2), 16);
		}
		return bytes;
	}

    static string GetImageAttribute(string mediaString, string attribute)
    {
        string attributeString = null;
        int attributeOffset = attribute.Length + 2;
        int attributeStart = mediaString.IndexOf(attribute + "='") + attributeOffset;
        if (attributeStart > attributeOffset)
        {
            int attributeEnd = mediaString.IndexOf("'", attributeStart);
            attributeString = mediaString.Substring(attributeStart, (attributeEnd - attributeStart));
        }
        return attributeString;
    }

    internal static string HTMLFromENMLContent(string content, List<ENResource> resources)
    {
        List<Resource> edamResources = new List<Resource>();
        foreach (ENResource resource in resources)
        {
            edamResources.Add(resource.EDAMResource());
        }
        return HTMLFromENMLContent(content, edamResources);
    }

    internal static string HTMLFromENMLContent(string content, ENCollection resources)
    {
        List<Resource> edamResources = new List<Resource>();
        foreach (Resource resource in resources)
        {
            edamResources.Add(resource);
        }
        return HTMLFromENMLContent(content, edamResources);
    }

    internal static string HTMLToText(string HTMLCode)
    {
        try
        {
            // Remove new lines since they are not visible in HTML
            HTMLCode = HTMLCode.Replace("\n", " ");

            // Remove tab spaces
            HTMLCode = HTMLCode.Replace("\t", " ");

            // Remove multiple white spaces from HTML
            HTMLCode = Regex.Replace(HTMLCode, "\\s+", " ");

            // Remove HEAD tag
            HTMLCode = Regex.Replace(HTMLCode, "<head.*?</head>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any JavaScript
            HTMLCode = Regex.Replace(HTMLCode, "<script.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Replace special characters like &, <, >, " etc.
            StringBuilder sbHTML = new StringBuilder(HTMLCode);
            // Note: There are many more special characters, these are just
            // most common. You can add new characters in this arrays if needed
            string[] OldWords = { "&nbsp;", "&amp;", "&quot;", "&lt;", "&gt;", "&reg;", "&copy;", "&bull;", "&trade;" };
            string[] NewWords = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢" };
            for (int i = 0; i < OldWords.Length; i++)
            {
                sbHTML.Replace(OldWords[i], NewWords[i]);
            }

            // Check if there are line breaks (<br>) or paragraph (<p>)
            sbHTML.Replace("<br>", "\n<br>");
            sbHTML.Replace("<br ", "\n<br ");
            sbHTML.Replace("<p ", "\n<p ");

            // Finally, remove all HTML tags and return plain text
            return System.Text.RegularExpressions.Regex.Replace(sbHTML.ToString(), "<[^>]*>", "");
        }
        catch (Exception)
        {
            throw;
        }
    }

}
