using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;

namespace EvernoteSDK
{
	namespace Advanced
	{
		public class ENRegistry
		{
			public string CompanyKey {get; set;}
			public string ProductKey {get; set;}

			public ENRegistry()
			{
				FileVersionInfo vi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
				CompanyKey = vi.CompanyName;
				ProductKey = vi.ProductName;
				if (CompanyKey.Length == 0)
				{
					CompanyKey = ProductKey;
				}
			}

			private RegistryKey AppRegistryKey(string service)
			{
				return Registry.CurrentUser.CreateSubKey("Software").CreateSubKey(CompanyKey).CreateSubKey(ProductKey).CreateSubKey(service);
			}

			public string GetValue(string service, string valueName)
			{
				try
				{
                    return Convert.ToString(AppRegistryKey(service).GetValue(valueName));
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message);
				}
			}

			public void SetValue(string service, string valueName, string value)
			{
				AppRegistryKey(service).SetValue(valueName, value);
			}

			public void DeleteValue(string service, string valueName)
			{
				try
				{
					AppRegistryKey(service).DeleteValue(valueName);
				}
				catch (Exception)
				{
				}
			}

		}

	}

}