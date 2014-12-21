using System;
using System.Xml.Serialization;
using UnityEngine;

namespace EditorExtensions
{
	[XmlRoot("ConfigData")]
	public class ConfigData
	{
		[XmlElement("FileVersion")]
		public string FileVersion { get; set; }

		[XmlElement("KeyMap")]
		public KeyMaps KeyMap { get; set; }

		[XmlArray("AngleSnapValues")]
		public float[] AngleSnapValues { get; set; }

		[XmlElement("MaxSymmetry")]
		public int MaxSymmetry { get; set; }

		/// <summary>
		/// Time, in seconds, that the on screen messages will display
		/// </summary>
		[XmlElement("OnScreenMessageTime")]
		public float OnScreenMessageTime { get; set; }

		public ConfigData()
		{
			this.KeyMap = new KeyMaps ();
		}
	}

	public class KeyMaps
	{
		[XmlElement("ResetCamera")]
		public KeyCode ResetCamera { get; set; }

		[XmlElement("AttachmentMode")]
		public KeyCode AttachmentMode { get; set; }

		[XmlElement("AngleSnap")]
		public KeyCode AngleSnap { get; set; }

		[XmlElement("Symmetry")]
		public KeyCode Symmetry { get; set; }

		[XmlElement("PartClipping")]
		public KeyCode PartClipping { get; set; }
	}
}

