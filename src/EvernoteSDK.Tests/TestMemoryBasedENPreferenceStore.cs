using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvernoteSDK.Advanced;

namespace EvernoteSDK.Tests
{
    public class TestMemoryBasedENPreferenceStore: ENPreferencesStore
    {
        private Dictionary<string, object> _Store { get; set; }
        public TestMemoryBasedENPreferenceStore()
        {
            _Store = new Dictionary<string, object>();
        }
        public override object ObjectForKey(string key)
        {
            object value = null;
            _Store.TryGetValue(key, out value);
            return value;
        }
        public override void RemoveAllObjects()
        {
            _Store.Clear();
        }
        public override void SetObject(object objectToStore, string key)
        {
            if (objectToStore != null)
            {
                _Store[key] = objectToStore;
            }
            else
            {
                _Store.Remove(key);
            }
        }
    }
}
