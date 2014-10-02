using System;
using System.Collections.Generic;
using Evernote.EDAM.Type;

namespace EvernoteSDK
{
	public class ENNote
	{

#region ENNote

		private string _Title;
		public string Title
		{
			get
			{
				return _Title;
			}
			set
			{
				_Title = value.EnScrubUsingRegex(Evernote.EDAM.Limits.Constants.EDAM_NOTE_TITLE_REGEX, Evernote.EDAM.Limits.Constants.EDAM_NOTE_TITLE_LEN_MIN, Evernote.EDAM.Limits.Constants.EDAM_NOTE_TITLE_LEN_MAX);
			}
		}

		private ENNoteContent _Content;
		public ENNoteContent Content
		{
			get
			{
				return _Content;
			}
			set
			{
				_Content = value;
			}
		}

		private List<string> _TagNames;
		public List<string> TagNames
		{
			get
			{
				return _TagNames;
			}
			set
			{
				List<string> tags = new List<string>();
				foreach (string tagName in value)
				{
					var scrubbedTag = tagName.EnScrubUsingRegex(Evernote.EDAM.Limits.Constants.EDAM_TAG_NAME_REGEX, Evernote.EDAM.Limits.Constants.EDAM_TAG_NAME_LEN_MIN, Evernote.EDAM.Limits.Constants.EDAM_TAG_NAME_LEN_MAX);
					if (scrubbedTag != null)
					{
						tags.Add(scrubbedTag);
					}
				}
				_TagNames = (tags.Count > 0) ? tags : null;
			}
		}

		public bool IsReminder {get; set;}

		private List<ENResource> _Resources;
		public List<ENResource> Resources
		{
			get
			{
				return _Resources;
			}
			internal set
			{
				_Resources = value;
			}
		}

		internal string SourceUrl {get; set;}
		private string CachedENMLContent {get; set;}
        private string CachedHTMLContent { get; set; }
        private string CachedTextContent { get; set; }
        private Note ServiceNote { get; set; }
		internal Dictionary<string, object> EdamAttributes {get; set;}

		public ENNote()
		{
			_Resources = new List<ENResource>();
			EdamAttributes = new Dictionary<string, object>();
		}

		// Initialize a new ENNote from an EDAMNote.
		internal ENNote(Note edamNote)
		{
			// Copy the fields that can be edited at this level.
			_Title = edamNote.Title;
			_Content = ENNoteContent.NoteContentWithENML(edamNote.Content);
			IsReminder = edamNote.Attributes.ReminderOrder != 0;
			SourceUrl = edamNote.Attributes.SourceURL;
			_TagNames = edamNote.TagNames; //This is usually null, unfortunately, on notes that come from the service.

			// Resources to ENResources
			_Resources = new List<ENResource>();
			if (edamNote.Resources != null)
			{
				foreach (Resource serviceResource in edamNote.Resources)
				{
					ENResource resource = ENResource.ResourceWithServiceResource(serviceResource);
					if (resource != null)
					{
						_Resources.Add(resource);
					}
				}
			}

			// Keep a copy of the service note around with all of its extra properties
			// in case we have to convert back to an EDAMNote later.
			ServiceNote = edamNote;

			// Get rid of these references here; they take up memory and we can let them be potentially cleaned up.
			ServiceNote.Content = null;
			ServiceNote.Resources = null;
		}

#endregion

#region Protected Methods

		internal void InvalidateCachedContent()
		{
			CachedENMLContent = null;
            CachedHTMLContent = null;
		}

		public void AddResource(ENResource resource)
		{
			if (resource != null)
			{
				if (Resources.Count >= Evernote.EDAM.Limits.Constants.EDAM_NOTE_RESOURCES_MAX)
				{
					ENSDKLogger.ENSDKLogInfo(string.Format("Too many resources already on note. Ignoring {0}. Note {1}.", resource, this));
				}
				else
				{
					InvalidateCachedContent();
					Resources.Add(resource);
				}
			}
		}

		public void RemoveAllResources()
		{
			Resources.Clear();
		}

		internal string EnmlContent()
		{
			if (string.IsNullOrEmpty(CachedENMLContent))
			{
				CachedENMLContent = Content.EnmlWithResources(Resources);
			}
			return CachedENMLContent;
		}

        public string HtmlContent
        {
			get
			{
                if (string.IsNullOrEmpty(CachedHTMLContent))
                {
                    try
                    {
                        CachedHTMLContent = ENMLtoHTMLConverter.HTMLFromENMLContent(Content.Enml(), Resources);
                    }
                    catch (Exception)
                    {
                        ENSDKLogger.ENSDKLogError(string.Format("Unable to convert content to HTML for note {0}", Title));
                        CachedHTMLContent = null;
                    }
                }
                return CachedHTMLContent;
            }
		}

        public string TextContent
        {
            get
            {
                if (string.IsNullOrEmpty(CachedTextContent))
                {
                    if (string.IsNullOrEmpty(CachedHTMLContent))
                    {
                        try
                        {
                            CachedHTMLContent = ENMLtoHTMLConverter.HTMLFromENMLContent(Content.Enml(), Resources);
                        }
                        catch (Exception)
                        {
                            ENSDKLogger.ENSDKLogError(string.Format("Unable to convert content to Text (error in HTML conversion) for note {0}", Title));
                            CachedHTMLContent = null;
                        }
                    }
                    try
                    {
                        CachedTextContent = ENMLtoHTMLConverter.HTMLToText(CachedHTMLContent);
                    }
                    catch (Exception)
                    {
                        ENSDKLogger.ENSDKLogError(string.Format("Unable to convert content to Text for note {0}", Title));
                        CachedTextContent = null;
                    }
                }
                return CachedTextContent;
            }
        }

		internal Note EDAMNote()
		{
			return EDAMNoteToReplaceServiceNoteGUID(null);
		}

		internal Note EDAMNoteToReplaceServiceNoteGUID(string guid)
		{
			// Turn the ENNote into an EDAMNote. Use the cached EDAMNote as a starting point if we have one
			// and if it matches the note GUID we're given. This way we can preserve the characteristics
			// of the "original" note we might be replacing, but not propagate those properties to a
			// a completely fresh note.
			Note edNote = null;
			if (ServiceNote != null && ServiceNote.Guid == guid)
			{
				// TODO: Make sure we don't need a copy here like the iOS SDK
				edNote = ServiceNote;
				// Don't preserve these. Our caller will either rewrite them or leave them blank.
				edNote.Guid = null;
				edNote.NotebookGuid = null;
				edNote.UpdateSequenceNum = 0;
			}
			else
			{
				edNote = new Note();
			}

			edNote.Content = EnmlContent();
			if (string.IsNullOrEmpty(edNote.Content))
			{
				ENNoteContent emptyContent = ENNoteContent.NoteContentWithString("");
				edNote.Content = emptyContent.EnmlWithResources(Resources);
			}
			// Invalidate the derivative content fields.
			edNote.ContentHash = null;
			edNote.ContentLength = 0;

			edNote.Title = Title;
			if (string.IsNullOrEmpty(edNote.Title))
			{
				// Only use a dummy title if we couldn't get a real one inside limits.
				edNote.Title = "Untitled Note";
			}

			// Set up note attributes.
			if (edNote.Attributes == null)
			{
				edNote.Attributes = new NoteAttributes();
			}
			var sourceApplication = ENSession.SharedSession.SourceApplication;

			// Write sourceApplication and source on all notes.
			edNote.Attributes.SourceApplication = sourceApplication;
			edNote.Attributes.Source = "";

			// If reminder is flagged on, set reminderOrder to the current UNIX timestamp by convention.
			// (Preserve existing reminderOrder if present)
			if (IsReminder)
			{
				if (edNote.Attributes.ReminderOrder == 0)
				{
					edNote.Attributes.ReminderOrder = DateTime.Now.ToEdamTimestamp();
				}
			}

			if (SourceUrl != null)
			{
				edNote.Attributes.SourceURL = SourceUrl;
			}

			// Move tags over if present.
			if (TagNames != null)
			{
				edNote.TagNames = TagNames;
			}

			// Turn any ENResources on the note into EDAMResources.
			List<Resource> edResources = new List<Resource>();
			foreach (var localResource in Resources)
			{
				Resource rs = localResource.EDAMResource();
				if (rs != null)
				{
					edResources.Add(rs);
				}
			}

			// Always set the resources array, even if empty. If we end up using this EDAMNote to
			// update an existing note, we always desire the intention of removing any existing resources.
			edNote.Resources = edResources;

			// Set EDAM attributes if EdamAttributes dictionary is not null.
			if (EdamAttributes != null)
			{
				foreach (string key in EdamAttributes.Keys)
				{
					var value = EdamAttributes[key];
					try
					{
						var piInstance = typeof(NoteAttributes).GetProperty(key);
						piInstance.SetValue(edNote.Attributes, value, null);
					}
					catch (KeyNotFoundException)
					{
						ENSDKLogger.ENSDKLogError(string.Format("Key {0} not found on EDAMNote.Attributes", key));
					}
					catch (Exception)
					{
						ENSDKLogger.ENSDKLogError(string.Format("Unable to set value {0} for key {1} on EDAMNote.Attributes", value, key));
					}
				}
			}

			return edNote;
		}

		internal bool ValidateForLimits()
		{
			if (EnmlContent().Length < Evernote.EDAM.Limits.Constants.EDAM_NOTE_CONTENT_LEN_MIN || EnmlContent().Length > Evernote.EDAM.Limits.Constants.EDAM_NOTE_CONTENT_LEN_MAX)
			{
				ENSDKLogger.ENSDKLogInfo(string.Format("Note fails validation for content length: {0}", this));
				return false;
			}

			var maxResourceSize = Evernote.EDAM.Limits.Constants.EDAM_RESOURCE_SIZE_MAX_FREE;
			if (ENSession.SharedSession.IsPremiumUser)
			{
				maxResourceSize = Evernote.EDAM.Limits.Constants.EDAM_RESOURCE_SIZE_MAX_PREMIUM;
			}

			foreach (var rs in Resources)
			{
				if (rs.Data.Length > maxResourceSize)
				{
					ENSDKLogger.ENSDKLogInfo(string.Format("Note fails validation for resource length: {0}", this));
					return false;
				}
			}

			return true;
		}

#endregion
#region For private subclasses to override

		protected string GenerateENMLContent()
		{
			// This is a no-op in the base class. Subclasses use this entry point to generate ENML from
			// whatever they natively understand.
			return EnmlContent();
		}

#endregion

	}

}