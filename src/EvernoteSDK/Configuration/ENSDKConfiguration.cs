using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace EvernoteSDK.Configuration
{
    public class ENSDKConfiguration : ConfigurationSection
    {
        #region Constants
        const string PreferencesStoreTypeDefault = "EvernoteSDK.Advanced.ENPreferencesStore,EvernoteSDK";
        #endregion

        private static Lazy<ENSDKConfiguration> _lazyInitializedConfig = new Lazy<ENSDKConfiguration>(() => GetFromConfigOrDefault());
        private static ENSDKConfiguration GetFromConfigOrDefault()
        {
            ENSDKConfiguration currentConfiguration = ConfigurationManager.GetSection("EvernoteSDK") as ENSDKConfiguration;
            if (currentConfiguration == null)
            {
                currentConfiguration = new ENSDKConfiguration();
            }
            return currentConfiguration;
        }
        public static ENSDKConfiguration Singleton { get { return _lazyInitializedConfig.Value; } }
        [ConfigurationProperty(name: "preferencesStoreType", DefaultValue = PreferencesStoreTypeDefault)]
        public string PreferencesStoreType
        {
            get
            {
                return (string)this["preferencesStoreType"];
            }
            set
            {
                this["preferencesStoreType"] = value;
            }
        }
    }
}
