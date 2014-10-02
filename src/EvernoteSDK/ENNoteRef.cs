using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace EvernoteSDK
{
	[Serializable]
	public class ENNoteRef
	{
		// An opaque reference to an existing note in the service. It can be used to
		// share or update that same note at a later time.

		public enum ENNoteRefType
		{
			TypePersonal,
			TypeBusiness,
			TypeShared
		}

		public ENNoteRefType Type {get; set;}
		public string Guid {get; set;}
		public ENLinkedNotebookRef LinkedNotebook {get; set;}

		public byte[] AsData()
		{
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();
			bf.Serialize(ms, this);
			return ms.ToArray();
		}

		public static ENNoteRef NoteRefFromData(byte[] data)
		{
			MemoryStream memStream = new MemoryStream();
			BinaryFormatter binForm = new BinaryFormatter();
			memStream.Write(data, 0, data.Length);
			memStream.Seek(0, SeekOrigin.Begin);
			ENNoteRef obj = new ENNoteRef();
			obj = (ENNoteRef)binForm.Deserialize(memStream);
			return obj;
		}

        public override bool Equals(object Object)
        {
            if (this == Object)
            {
                return true;
            }

            if (Object == null)
            {
                return false;
            }

            if (Object.GetType() != typeof(ENNoteRef))
            {
                return false;
            }

            ENNoteRef other = (ENNoteRef)Object;
            if (other.Type == this.Type && this.Guid == other.Guid  && (this.LinkedNotebook == other.LinkedNotebook || other.LinkedNotebook.IsEqual(this.LinkedNotebook)))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + (int)this.Type;
            result = prime * result + this.Guid.GetHashCode();
            result = prime * result + this.LinkedNotebook.Hash();
            return result;
        }

         public string Description()
        {
            StringBuilder str = null;
            string typeStr = null;
            switch (this.Type)
            {
            case ENNoteRefType.TypePersonal :
                typeStr = "personal";
                break;
            case ENNoteRefType.TypeBusiness :
                typeStr = "business";
                break;
            case ENNoteRefType.TypeShared :
                typeStr = "shared";
                break;
            }

            str.AppendFormat("<{0}: {1}; guid = {2}; type = {3}", typeof(ENNoteRef), this, this.Guid, typeStr);
            if (this.LinkedNotebook != null)
            {
                str.AppendFormat("; link shard = {0}", this.LinkedNotebook.ShardId);
            }

            str.Append(">");
            return str.ToString();
        }

	}

}