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
             return ObjectFactory.CreateObject<ENPreferencesStore>(ENSDKConfiguration.Singleton.PreferencesStoreType);
         });
        public static ENPreferencesStore GetENPreferencesStore()
        {
            return _lazyInitializedPreferenceStore.Value;

        }
    }
}
