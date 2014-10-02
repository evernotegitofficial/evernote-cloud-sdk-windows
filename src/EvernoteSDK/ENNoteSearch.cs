//
//  A note search represents a search query for the Evernote service used in finding notes.
//


namespace EvernoteSDK
{
	public class ENNoteSearch
	{

		//
		// The search in the Evernote search grammar.
		//
		public string SearchString {get; set;}

        //internal ENNoteSearch()
        //{
        //}

		//
		// The designated initializer for a note search, from a raw search string.
		// You can use the full search grammar as described at http://dev.evernote.com/doc/articles/search_grammar.php
		//
		// @param searchString A search string.
		//
		// @return An initialized note search object.
		//
		public ENNoteSearch(string search)
		{
			SearchString = search;
		}

		//
		// Class method to get a new search object from a raw search string. 
		// You can use the full search grammar as described at http://dev.evernote.com/doc/articles/search_grammar.php
		//
		// @param searchString A search string.
		//
		// @return A note search object.
		//
		public static ENNoteSearch NoteSearch(string searchString)
		{
			if (searchString == null)
			{
				return null;
			}
			return new ENNoteSearch(searchString);
		}

		//
		// Class method to get a new search object that represents all notes created by this application.
		// "This application" is based on the sourceApplication property on ENSession.
		//
		// @return A note search object.
		//
		public static ENNoteSearch NoteSearchCreatedByThisApplication()
		{
			string search = string.Format("sourceApplication:{0}", ENSession.SharedSession.SourceApplication);
			return new ENNoteSearch(search);
		}

	}

}