using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.CustomMarshalers;

namespace EvernoteSDK
{
    //[ClassInterface(ClassInterfaceType.AutoDual)]
    [ClassInterface(ClassInterfaceType.None)]
    public class ENCollection : _Collection
    {
        private Microsoft.VisualBasic.Collection m_Collection = new Microsoft.VisualBasic.Collection();

        public void Add(ref object Item, ref object Key, ref object Before)
        {
            object tempVar = null;
            Add(ref Item, ref Key, ref Before, ref tempVar);
        }

        public void Add(ref object Item, ref object Key)
        {
            object tempVar = null;
            object tempVar2 = null;
            Add(ref Item, ref Key, ref tempVar, ref tempVar2);
        }

        public void Add(ref object Item)
        {
            object tempVar = null;
            object tempVar2 = null;
            object tempVar3 = null;
            Add(ref Item, ref tempVar, ref tempVar2, ref tempVar3);
        }

        public void Add(ref object Item, ref object Key, ref object Before, ref object After)
        {
            string sKey = null;
            if (!(Key is System.Reflection.Missing) && Key != null)
            {
                sKey = Key.ToString();
            }

            object oBefore = null;
            if (IsNumeric(Before))
            {
                oBefore = Convert.ToInt32(Before);
            }
            else if (!(Before is System.Reflection.Missing) && Before != null)
            {
                oBefore = Before.ToString();
            }

            object oAfter = null;
            if (IsNumeric(After))
            {
                oAfter = Convert.ToInt32(After);
            }
            else if (!(After is System.Reflection.Missing) && After != null)
            {
                oAfter = After.ToString();
            }

            m_Collection.Add(Item, sKey, oBefore, oAfter);
        }

        public int Count()
        {
            return m_Collection.Count;
        }

        public object Item(ref object Index)
        {
            if (IsNumeric(Index))
            {
                return m_Collection[Convert.ToInt32(Index)];
            }
            else if (m_Collection.Contains(Index.ToString()))
            {
                return m_Collection[Index.ToString()];
            }
            else
            {
                throw new Exception(String.Format("Item '{0}' not in collection.", Index));
            }
        }

        public void Remove(ref object Index)
        {
            if (IsNumeric(Index))
            {
                m_Collection.Remove(Convert.ToInt32(Index));
            }
            else
            {
                m_Collection.Remove(Index.ToString());
            }
        }

        [System.Runtime.CompilerServices.IndexerName("MyItem")]
        public object this[object Index]
        {
            get
            {
                return Item(ref Index);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return m_Collection.GetEnumerator();
        }

        private static bool IsNumeric(object expression)
        {
            if (expression == null)
                return false;

            double testDouble;
            if (double.TryParse(expression.ToString(), out testDouble))
                return true;

            bool testBool;
            if (bool.TryParse(expression.ToString(), out testBool))
                return true;

            return false;
        }

    }

    [ComImport(), TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FHidden), Guid("A4C46780-499F-101B-BB78-00AA00383CBB"), DefaultMember("Item")]
    //[ComImport(), TypeLibType(TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FHidden), Guid("A4C46780-499F-101B-BB78-00AA00383CBB"), DefaultMember("Item")]
    public interface _Collection : IEnumerable
    {
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)]
        object Item([In(), MarshalAs(UnmanagedType.Struct)] ref object Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
        void Add([In(), MarshalAs(UnmanagedType.Struct)] ref object Item, [In(), MarshalAs(UnmanagedType.Struct)] ref object Key, [In(), MarshalAs(UnmanagedType.Struct)] ref object Before);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
        void Add([In(), MarshalAs(UnmanagedType.Struct)] ref object Item, [In(), MarshalAs(UnmanagedType.Struct)] ref object Key);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
        void Add([In(), MarshalAs(UnmanagedType.Struct)] ref object Item);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
        void Add([In(), MarshalAs(UnmanagedType.Struct)] ref object Item, [In(), MarshalAs(UnmanagedType.Struct)] ref object Key, [In(), MarshalAs(UnmanagedType.Struct)] ref object Before, [In(), MarshalAs(UnmanagedType.Struct)] ref object After);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
        int Count();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)]
        void Remove([In(), MarshalAs(UnmanagedType.Struct)] ref object Index);
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "", MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie = "")]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-4)]
        new IEnumerator GetEnumerator();
    }

}
