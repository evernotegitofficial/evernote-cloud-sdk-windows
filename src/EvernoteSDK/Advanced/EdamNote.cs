using System.Collections.Generic;
using Evernote.EDAM.Type;

namespace EvernoteSDK.Advanced
{
    public class EdamNote
    {
        public string Guid { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public byte[] ContentHash { get; set; }
        public int ContentLength { get; set; }
        public long Created { get; set; }
        public long Updated { get; set; }
        public long Deleted { get; set; }
        public bool Active { get; set; }
        public int UpdateSequenceNum { get; set; }
        public string NotebookGuid { get; set; }
        internal List<string> TagGuidsList { get; set; }
        public List<Resource> ResourcesList { get; set; }
        public NoteAttributes Attributes { get; set; }
        public List<string> TagNamesList { get; set; }

        public ENCollection TagGuids
        {
            get
            {
                ENCollection comResults = new ENCollection();
                if (TagGuidsList != null)
                {
                    foreach (string tag in TagGuidsList)
                    {
                        object tempvar = tag;
                        comResults.Add(ref tempvar);
                    }
                }
                return comResults; 
            }
        }

        public ENCollection Resources
        {
            get
            {
                ENCollection comResults = new ENCollection();
                foreach (Resource resource in ResourcesList)
                {
                    object tempvar = resource;
                    comResults.Add(ref tempvar);
                }
                return comResults;
            }
        }

        public ENCollection TagNames
        {
            get
            {
                ENCollection comResults = new ENCollection();
                foreach (string tag in TagNamesList)
                {
                    object tempvar = tag;
                    comResults.Add(ref tempvar);
                }
                return comResults;
            }
        }

    }
}
