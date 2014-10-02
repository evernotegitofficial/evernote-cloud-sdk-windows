using Evernote.EDAM.Type;

namespace EvernoteSDK
{
	public class ENNotebook
	{
		private Notebook Notebook {get; set;}
		internal LinkedNotebook LinkedNotebook {get; set;}
		private SharedNotebook SharedNotebook {get; set;}
		internal bool IsDefaultNotebookOverride {get; set;}

		// Default constructor. Don't use this constructor; this is just here so ENNotebookAdvanced can inherit this class.
		internal ENNotebook()
		{
		}

		// Constructor for a regular notebook
		internal ENNotebook(Notebook notebook)
		{
			Notebook = notebook;
			SharedNotebook = null;
			LinkedNotebook = null;
		}

		// Constructor for a Linked notebook
		internal ENNotebook(SharedNotebook sharedNotebook, LinkedNotebook linkedNotebook)
		{
			Notebook = null;
			SharedNotebook = sharedNotebook;
			LinkedNotebook = linkedNotebook;
		}

		// Constructor for a Public notebook
		internal ENNotebook(Notebook publicNotebook, LinkedNotebook linkedNotebook)
		{
			Notebook = publicNotebook;
			SharedNotebook = null;
			LinkedNotebook = linkedNotebook;
		}

		// Constructor for a Business notebook
		internal ENNotebook(Notebook notebook, SharedNotebook sharedNotebook, LinkedNotebook linkedNotebook)
		{
			Notebook = notebook;
			SharedNotebook = sharedNotebook;
			LinkedNotebook = linkedNotebook;
		}

		public string Name
		{
			get
			{
				if (Notebook != null)
				{
					return Notebook.Name;
				}
				else if (LinkedNotebook != null)
				{
					return LinkedNotebook.ShareName;
				}
				else
				{
					return null;
				}
			}
		}

		public string OwnerDisplayName
		{
			get
			{
				string ownerName = null;
				if (IsBusinessNotebook)
				{
					ownerName = Notebook.Contact.Name;
					if (ownerName.Length == 0)
					{
						ownerName = ENSession.SharedSession.BusinessDisplayName;
					}
				}
				else if (LinkedNotebook != null)
				{
					ownerName = LinkedNotebook.Username;
				}
				else
				{
					ownerName = ENSession.SharedSession.UserDisplayName;
				}
				return ownerName;
			}
		}

		internal string Guid
		{
			get
			{
				// Personal notebooks have a native guid, and if we've stashed a public/business-native notebook here, then we can look at that
				// as well.
				if (Notebook != null)
				{
					return Notebook.Guid;
				}
				else if (SharedNotebook != null)
				{
					return SharedNotebook.NotebookGuid;
				}
				else
				{
					return null;
				}
			}
		}

		public bool IsShared
		{
			get
			{
				return IsOwnShared || IsJoinedShared;
			}
		}

		public bool IsOwnShared
		{
			get
			{
				return !IsLinked && Notebook.SharedNotebookIds.Count > 0;
			}
		}

		public bool IsJoinedShared
		{
			get
			{
				return IsLinked;
			}
		}

		public bool IsLinked
		{
			get
			{
				return LinkedNotebook != null;
			}
		}

		public bool IsPublic
		{
			get
			{
				return IsOwnPublic || IsJoinedPublic;
			}
		}

		public bool IsJoinedPublic
		{
			get
			{
				return IsLinked && string.IsNullOrEmpty(LinkedNotebook.ShareKey);
			}
		}

		public bool IsOwnPublic
		{
			get
			{
				return !IsLinked && Notebook.Publishing.Uri.Length > 0;
			}
		}

		public bool IsBusinessNotebook
		{
			get
			{
				// Business notebooks are the only ones that have a combination of a linked notebook and normal
				// notebook being set. In this case, the normal notebook represents the notebook inside the business.
				// Additionally, checking linked notebook record is actually pointing to a shared notebook record so it's not a public notebook.
				return Notebook != null && LinkedNotebook != null && LinkedNotebook.ShareKey != null;
			}
		}

		public bool IsOwnedByUser
		{
			get
			{
				if (LinkedNotebook == null)
				{
					// If there's no linked record, the notebook exists in the primary account, which means owned by user.
					return true;
				}
				else if (IsBusinessNotebook)
				{
					// If it's not a business notebook, but it is linked, then it's definitely NOT owned by the user.
					return false;
				}
				else
				{
					// Business notebooks are a little trickier. They are always linked, because technically the business owns
					// them. What we really want to know is whether the contact user is the same as the current user.
					return Notebook.Contact.Id == ENSession.SharedSession.UserID;
				}
			}
		}

		public bool IsDefaultNotebook
		{
			get
			{
				if (IsDefaultNotebookOverride)
				{
					return true;
				}
				else if (Notebook != null && !IsJoinedPublic)
				{
					return Notebook.DefaultNotebook;
				}
				else
				{
					return false;
				}
			}
		}

		public bool AllowsWriting
		{
			get
			{
				if (LinkedNotebook == null)
				{
					// All personal notebooks are readwrite.
					return true;
				}

				if (IsJoinedPublic)
				{
					// All public notebooks are readonly.
					return false;
				}

				int privilege = (int)SharedNotebook.Privilege;
				if (privilege == (int)SharedNotebookPrivilegeLevel.GROUP)
				{
					// Need to consult the business notebook object privilege.
					privilege = (int)Notebook.BusinessNotebook.Privilege;
				}

				if (privilege == (int)SharedNotebookPrivilegeLevel.MODIFY_NOTEBOOK_PLUS_ACTIVITY || privilege == (int)SharedNotebookPrivilegeLevel.FULL_ACCESS || privilege == (int)SharedNotebookPrivilegeLevel.BUSINESS_FULL_ACCESS)
				{
					return true;
				}

				return false;
			}
		}

		internal string Description
		{
			get
			{
				string owner = null;
				if (IsOwnedByUser)
				{
					owner = string.Format("\"{0}\"", OwnerDisplayName) + " (me)";
				}
				else
				{
					owner = string.Format("\"{0}\"", OwnerDisplayName);
				}
				return string.Format("<name = \"{0}\"; business = {1}; shared = {2}; owner = {3}; access = {4}>", Name, (IsBusinessNotebook ? "YES" : "NO"), (IsShared ? "YES" : "NO"), owner, (AllowsWriting ? "R/W" : "R/O"));
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(GetType() == obj.GetType()))
			{
				return false;
			}

			return Guid == ((ENNotebook)obj).Guid;
		}

		public override int GetHashCode()
		{
			return Guid.GetHashCode();
		}

	}

}