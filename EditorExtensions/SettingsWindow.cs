using System;
using UnityEngine;

namespace EditorExtensions
{
	public class SettingsWindow
	{
		ConfigData cfg;
		string _configFilePath;

		public delegate void Cancel();

		Cancel _cancelCallback;

		public SettingsWindow (ConfigData currentConfig, string configFilePath, Cancel cancelCallback)
		{
			cfg = currentConfig;
			_configFilePath = configFilePath;
			_cancelCallback = cancelCallback;
		}

		public void CreateContent(int windowID)
		{
			GUILayout.BeginVertical();

			//_toolbarInt = GUILayout.Toolbar (_toolbarInt, _toolbarStrings);

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Version:");
			GUILayout.Label (cfg.FileVersion, "TextField");
			GUILayout.EndHorizontal ();


			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Current key:");
			//GUILayout.Label (_lastKeyDown.ToString(), "TextField");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Current key:");
			GUILayout.Label (EditorLogic.SelectedPart ? EditorLogic.SelectedPart.name : "none");
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Message time (s):");
			string osdTimeString = GUILayout.TextField (cfg.OnScreenMessageTime.ToString());
			float newOsdTime = cfg.OnScreenMessageTime;
			if (float.TryParse (osdTimeString, out newOsdTime)) {
				cfg.OnScreenMessageTime = newOsdTime;
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Max symmetry:");
			string maxSym = GUILayout.TextField (cfg.MaxSymmetry.ToString());
			int newMaxSym = cfg.MaxSymmetry;
			if (Int32.TryParse (maxSym, out newMaxSym)) {
				cfg.MaxSymmetry = newMaxSym;
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if(GUILayout.Button("Cancel")){
				_cancelCallback ();
				//cfg = ModConfig.LoadConfig (_configFilePath);
				//_showSettings = false;
			}
			if(GUILayout.Button("Defaults")){
				//cfg = CreateDefaultConfig ();
			}
			if(GUILayout.Button("Save")){
				ModConfig.SaveConfig (cfg, _configFilePath);
				//_showSettings = false;
			}
			GUILayout.EndHorizontal();


			GUILayout.EndVertical();

			GUI.DragWindow();
		}
	}
}

