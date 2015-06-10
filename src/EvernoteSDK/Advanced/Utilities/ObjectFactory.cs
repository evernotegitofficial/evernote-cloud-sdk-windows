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
            ObjectType result = default(ObjectType);
            Type requestedType = Type.GetType(fullyQualifiedTypeName);
            result = Activator.CreateInstance<ObjectType>();
            return result;
        }
    }
}
