using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace EvernoteSDK.Configuration
{
    public class ENSDKConfiguration:ConfigurationSection
    {
        private static Lazy<ENSDKConfiguration> _lazyInitializedConfig = new Lazy<ENSDKConfiguration>(() => ConfigurationManager.GetSection("EvernoteSDK") as ENSDKConfiguration);

        public static ENSDKConfiguration Singleton { get { return _lazyInitializedConfig.Value; } }
        [ConfigurationProperty(name:"PreferencesStoreType",DefaultValue= "EvernoteSDK.Advanced.ENPreferencesStore,EvernoteSDK")]
        public string PreferencesStoreType { get; set; }
    }
}
