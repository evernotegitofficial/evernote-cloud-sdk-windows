using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvernoteSDK.Advanced.Utilities
{
    internal class ObjectFactory
    {
        public static ObjectType CreateObject<ObjectType>(string fullyQualifiedTypeName)
        {
            
            Type requestedType = Type.GetType(fullyQualifiedTypeName);
            ObjectType result = (ObjectType)Activator.CreateInstance(requestedType);
            return result;
        }
    }
}
