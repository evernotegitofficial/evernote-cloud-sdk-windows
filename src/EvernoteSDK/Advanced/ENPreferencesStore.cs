using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace EvernoteSDK
{
	namespace Advanced
	{
		public class ENPreferencesStore
		{
			private string Pathname {get; set;}
			private Dictionary<string, object> Store {get; set;}

			private static object PathnameForStoreFilename(string filename)
			{
				FileVersionInfo vi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
				var companyKey = vi.CompanyName;
				var productKey = vi.ProductName;
				if (companyKey.Length == 0)
				{
					companyKey = productKey;
				}
				return string.Format("{0}\\{1}\\{2}\\{3}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), companyKey, productKey, filename);
			}

			public ENPreferencesStore(string filename)
			{
				Pathname = PathnameForStoreFilename(filename).ToString();
				Store = new Dictionary<string, object>();
				Load();
			}

			public object ObjectForKey(string key)
			{
				object value = null;
				Store.TryGetValue(key, out value);
				return value;
			}

			public void SetObject(object objectToStore, string key)
			{
				if (objectToStore != null)
				{
					Store[key] = objectToStore;
				}
				else
				{
					Store.Remove(key);
				}
				Save();
			}

            private void Save()
            {
                string dir = Path.GetDirectoryName(Pathname);
                if (!(Directory.Exists(dir)))
                {
                    Directory.CreateDirectory(dir);
                }

                FileStream stream = new FileStream(Pathname, FileMode.OpenOrCreate);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, Store);
                stream.Close();
            }

			public void RemoveAllObjects()
			{
				Store.Clear();
				Save();
			}

            private void Load()
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;

                currentDomain.AssemblyResolve += MyResolveEventHandler;

                if (File.Exists(Pathname))
                {
                    FileStream stream = new FileStream(Pathname, FileMode.Open);
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    Store = (Dictionary<string, object>)formatter.Deserialize(stream);
                    stream.Close();
                }
            }

            private Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
            {
                Assembly[] ayAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly item in ayAssemblies)
                {
                    if (item.FullName == args.Name)
                    {
                        return item;
                    }
                }

                return null;
            }

		}
	}

}