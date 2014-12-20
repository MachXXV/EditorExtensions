using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace EditorExtensions
{

	public static class ModConfig
	{

		static ModConfig ()
		{
		}
			
		public static bool FileExists (string filePath)
		{
			try {
				FileInfo file = new FileInfo (filePath);
				return file.Exists;
			} catch (Exception ex) {
				Log.Error ("Failed to verify file " + filePath + " Error: " + ex.Message);
				return false;
			}
		}
			
		public static void SaveConfig (ConfigData configData, string configFilePath)
		{
			try {
				XmlSerializer serializer = new XmlSerializer (typeof(ConfigData));
				using (TextWriter writer = new StreamWriter (configFilePath)) {
					serializer.Serialize (writer, configData); 
				} 
			} catch (Exception ex) {
				Log.Error ("Error saving config file: " + ex.Message);
			}
		}

		public static ConfigData LoadConfig (string configFilePath)
		{
			try {
				XmlSerializer deserializer = new XmlSerializer (typeof(ConfigData));

				ConfigData data;
				using (TextReader reader = new StreamReader (configFilePath)) {
					object obj = deserializer.Deserialize (reader);
					data = (ConfigData)obj;
				}
				return data;
			} catch (Exception ex) {
				Log.Error ("Error loading config file: " + ex.Message);
				return null;
			}
		}

	}
}

