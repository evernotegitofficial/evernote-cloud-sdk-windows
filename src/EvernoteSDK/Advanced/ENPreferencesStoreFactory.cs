using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvernoteSDK.Advanced.Utilities;
using EvernoteSDK.Configuration;

namespace EvernoteSDK.Advanced
{
    public class ENPreferencesStoreFactory
    {
        private static Lazy<ENPreferencesStore> _lazyInitializedPreferenceStore = new Lazy<ENPreferencesStore>(() =>
         {
             ENPreferencesStore store = ObjectFactory.CreateObject<ENPreferencesStore>(ENSDKConfiguration.Singleton.PreferencesStoreType);
             return store;
         });
        public static ENPreferencesStore GetENPreferencesStore()
        {
            return _lazyInitializedPreferenceStore.Value;

        }
    }
}
