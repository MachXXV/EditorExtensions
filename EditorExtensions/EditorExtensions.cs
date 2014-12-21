using System;
using UnityEngine;
using KSP.IO;
using System.Reflection;
using System.IO;

namespace EditorExtensions
{
	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public class EditorExtensions : MonoBehaviour
	{
		public static EditorExtensions Instance { get; private set; }
		public bool Visible {get;set;}

		#region member vars

		//look into loading game keymaps for applying alt+shift modifiers

		//old vars
		//const string launchSiteName_LaunchPad = "LaunchPad";
		//const string launchSiteName_Runway = "Runway";
		//bool ignoreHotKeys = false;
		//bool inVAB = false;

		const string ConfigFileName = "config.xml";
		const string degreesSymbol = "\u00B0";

		EditorLogic editor;
		Version pluginVersion;
		ConfigData cfg;
		string _pluginDirectory;
		string _configFilePath;
		int _symmetryMode = 0;

		bool enableHotkeys = true;
		//bool _abort = false;

		#endregion

		/// <summary>
		/// ctor - gets plugin info and initializes config
		/// </summary>
		public EditorExtensions ()
		{
			try {
				//get location and version info of the plugin
				Assembly execAssembly = Assembly.GetExecutingAssembly ();
				pluginVersion = execAssembly.GetName ().Version;
				_pluginDirectory = Path.GetDirectoryName (execAssembly.Location);

				//dll's path + filename for the config file
				_configFilePath = Path.Combine (_pluginDirectory, ConfigFileName);

				//check if the config file is there and create if its missing
				if (ConfigManager.FileExists (_configFilePath)) {

					cfg = ConfigManager.LoadConfig (_configFilePath);

					if (cfg == null) {
						//failed to load config, create new
						cfg = CreateDefaultConfig ();
					} else {
						//check config file version
						Version fileVersion = new Version ();

						if (cfg.FileVersion != null) {
							Log.Debug ("Config v" + cfg.FileVersion + " Mod v" + pluginVersion.ToString ());

							try {
								fileVersion = new Version (cfg.FileVersion);
							} catch (Exception ex) {
								Log.Error ("Error parsing version from config file: " + ex.Message);
							}
						}

#if DEBUG
						//for debug, replace if version isn't exactly the same
						bool versionMismatch = (cfg.FileVersion == null || fileVersion != pluginVersion);
#else
						//replace if x.x doesn't match
						bool versionMismatch = (cfg.FileVersion == null || fileVersion.Major < pluginVersion.Major || (fileVersion.Major == pluginVersion.Major && fileVersion.Minor < pluginVersion.Minor));
#endif

						if (versionMismatch) {
							Log.Info ("Config file version mismatch, replacing with new defaults");
							cfg = CreateDefaultConfig ();
						} else {
							Log.Debug ("Config file is current");
						}
					}

				} else {
					cfg = CreateDefaultConfig ();
					Log.Info ("No existing config found, created new default config");
				}

				Log.Debug ("Initializing version " + pluginVersion.ToString ());
			} catch (Exception ex) {
				Log.Debug ("FATAL ERROR - Unable to initialize: " + ex.Message);
				//_abort = true;
				return;
			}
		}

		/// <summary>
		/// Creates a new config file with defaults
		/// will replace any existing file
		/// </summary>
		/// <returns>New config object with default settings</returns>
		private ConfigData CreateDefaultConfig ()
		{
			try {
				ConfigData defaultConfig = new ConfigData () {
					AngleSnapValues = new float[]{ 0.0f, 1.0f, 5.0f, 15.0f, 22.5f, 30.0f, 45.0f, 60.0f, 90.0f },
					MaxSymmetry = 99,
					FileVersion = pluginVersion.ToString (),
					OnScreenMessageTime = 1.5f
				};

				KeyMaps defaultKeys = new KeyMaps () {
					AngleSnap = KeyCode.C,
					AttachmentMode = KeyCode.T,
					PartClipping = KeyCode.Z,
					ResetCamera = KeyCode.Space,
					Symmetry = KeyCode.X
				};
				defaultConfig.KeyMap = defaultKeys;

				if(ConfigManager.SaveConfig (defaultConfig, _configFilePath))
					Log.Debug ("Created default config");
				else
					Log.Error("Failed to save default config");

				return defaultConfig;
			} catch (Exception ex) {
				Log.Debug ("Error defaulting config: " + ex.Message);
				return null;
			}
		}
	
		//Unity initialization call
		public void Awake ()
		{
			//get current editor instance
			editor = EditorLogic.fetch;

			Instance = this;
	
			InitializeGUI ();
		}

		//Broken
		const string VABGameObjectName = "interior_vehicleassembly";
		const string SPHGameObjectName = "xport_sph3";
		/// <summary>
		/// embiggen the hangar space
		/// currently broken
		/// </summary>
		/// <param name="editor">Editor.</param>
		void AlterEditorSpace (EditorLogic editor)
		{	
			// Modify cameras/available interior space
			if (HighLogic.LoadedScene == GameScenes.EDITOR) {
				Log.Debug ("Updating VAB dimensions and camera");
	
				VABCamera VABcam = Camera.main.GetComponent<VABCamera> ();
				VABcam.maxHeight = 2000;
				VABcam.maxDistance = 2000;
	
				GameObject interior = GameObject.Find (VABGameObjectName);
				interior.transform.localScale = new Vector3 (2.2f, 1.8f, 1.8f);
				interior.transform.position = new Vector3 (59f, 51.5f, 12);
			}
//			else if (HighLogic.LoadedScene == GameScenes.SPH)
//			{
//				Log.Debug ("Updating SPH dimensions and camera");
//	
//				SPHCamera SPHcam = Camera.main.GetComponent<SPHCamera>();
//				SPHcam.maxHeight = 2000;
//				SPHcam.maxDistance = 2000;
//				SPHcam.maxDisplaceX = 2000;
//				SPHcam.maxDisplaceZ = 2000;
//	
//				GameObject interior = GameObject.Find(SPHGameObjectName);
//				interior.transform.localScale = new Vector3(12, 6, 12);
//				interior.transform.position = new Vector3(-24.9f, -0.3f, 22.8f);
//			}
		}

		bool altKeyDown;
		bool shiftKeyDown;
		/// <summary>
		/// Fired by Unity event loop
		/// </summary>
		public void Update ()
		{		
			//Disable shortcut keys when ship name textarea has focus
			//if(ignoreHotKeys || editor.editorScreen != EditorLogic.EditorScreen.Parts)
			//    return;
	
			//may need to go away from this and do explicit editor.editorType calls 
			//inVAB = (editor.editorType == EditorLogic.EditorMode.VAB);

			//look into fuel crossfeed toggle
	
			//check for the various alt/mod etc keypresses
			altKeyDown = Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt) || Input.GetKey (KeyCode.AltGr);
			//check for shift key
			shiftKeyDown = Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift);

			//look into skewing camera	

			//hotkeyed editor functions
			if (enableHotkeys) {

				//Space - when no part is selected, reset camera
				if (Input.GetKeyDown (cfg.KeyMap.ResetCamera) && !EditorLogic.SelectedPart) {
					//if (HighLogic.LoadedSceneIsEditor) {
					VABCamera VABcam = Camera.main.GetComponent<VABCamera> ();
					VABcam.camPitch = 0;
					VABcam.camHdg = 0;
					//VABcam.ResetCamera ();

					SPHCamera SPHcam = Camera.main.GetComponent<SPHCamera> ();
					SPHcam.camPitch = 0;
					SPHcam.camHdg = 0;
					//SPHcam.ResetCamera();
					//}
				}
	
				//Broken, api doesnt respond
				// V - Vertical alignment toggle
//			if (Input.GetKeyDown (KeyCode.V)) {
//				//Log.Debug ("Toggling vertical snap");
//				GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ^= true;
//	
//				//if normal radial angle snap is currently off, vertical snap will have no effect unless it is re-enabled
//				//automatically set aangle snap to minimum - some people thought vert snap was broken in this situation, the game doesn't appear to allow it
//				if (GameSettings.VAB_USE_ANGLE_SNAP == false && GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL == true) {
//					Log.Debug ("Enabling angle snap to allow vertical snap to work");
//					//angle snap needs be > 0, otherwise log is spammed with DivideByZero errors
//					if (editor.srfAttachAngleSnap == 0)
//						editor.srfAttachAngleSnap = 1;
//					GameSettings.VAB_USE_ANGLE_SNAP = true;
//				}
//				OSDMessage ("Vertical snap " + (GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ? "enabled" : "disabled"), 1);
//				Log.Debug ("Vertical snap " + (GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ? "enabled" : "disabled"));
//				return;
//			}
	
				// T: Surface attachment toggle
				if (Input.GetKeyDown (cfg.KeyMap.AttachmentMode)) {

					if (EditorLogic.SelectedPart) {
						//Toggle surface attachment for selected part
						EditorLogic.SelectedPart.attachRules.srfAttach ^= true;

						Log.Debug ("Toggling srfAttach for " + EditorLogic.SelectedPart.name);
						OSDMessage (String.Format ("Surface attachment {0} for {1}"
							, EditorLogic.SelectedPart.attachRules.srfAttach ? "enabled" : "disabled"
							, EditorLogic.SelectedPart.name
						));
					}

					/* avoiding this approach of messing with global toggles for now
					Part selectedPart = EditorLogic.SelectedPart;
					if (selectedPart) {
						//Toggle surface attachment for selected part
						selectedPart.attachRules.srfAttach ^= true;

						//set global toggling to match
						editor.allowSrfAttachment = selectedPart.attachRules.srfAttach;
						editor.allowNodeAttachment = !selectedPart.attachRules.srfAttach;

						Log.Debug ("Toggling srfAttach for " + EditorLogic.SelectedPart.name);
						OSDMessage (String.Format ("Surface attachment {0} \n Node attachment {1} \n for {2}"
						, selectedPart.attachRules.srfAttach ? "enabled" : "disabled"
						, editor.allowNodeAttachment ? "enabled" : "disabled"
						, selectedPart.name
						), 1);
					}
					else {
						//just toggle global surface attachment, parts whose config do not allow it are unaffected
						editor.allowSrfAttachment ^= true;
						editor.allowNodeAttachment = !editor.allowSrfAttachment;
						OSDMessage (String.Format ("Surface attachment {0} \n Node attachment {1}"
						, editor.allowSrfAttachment ? "enabled" : "disabled"
						, editor.allowNodeAttachment ? "enabled" : "disabled"
						), 1);
					}
					*/
				}
	
				// ALT+Z : Toggle part clipping (From cheat options)
				if (altKeyDown && Input.GetKeyDown (cfg.KeyMap.PartClipping)) {
					CheatOptions.AllowPartClipping ^= true;
					Log.Debug ("AllowPartClipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"));
					OSDMessage ("Part clipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"), 1);
					return;
				}
	
				// C, Shift+C : Increment/Decrement Angle snap
				if (Input.GetKeyDown (cfg.KeyMap.AngleSnap)) {
	
					if (!altKeyDown) {
						Log.Debug ("Starting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString ());
	
						int currentAngleIndex = Array.IndexOf (cfg.AngleSnapValues, editor.srfAttachAngleSnap);
	
						Log.Debug ("currentAngleIndex: " + currentAngleIndex.ToString ());
	
						//rotate through the angle snap values
						float newAngle;
						if (shiftKeyDown) {
							//lower snap
							newAngle = cfg.AngleSnapValues [currentAngleIndex == 0 ? cfg.AngleSnapValues.Length - 1 : currentAngleIndex - 1];
						} else {
							//higher snap
							//Log.Debug ("new AngleIndex: " + (currentAngleIndex == angleSnapValues.Length - 1 ? 0 : currentAngleIndex + 1).ToString ());
							newAngle = cfg.AngleSnapValues [currentAngleIndex == cfg.AngleSnapValues.Length - 1 ? 0 : currentAngleIndex + 1];
						}
	
						Log.Debug ("Setting srfAttachAngleSnap to " + newAngle.ToString ());
						editor.srfAttachAngleSnap = newAngle;
					} else {
						Log.Debug ("Resetting srfAttachAngleSnap to 0");
						editor.srfAttachAngleSnap = 0;
					}
	
					//at angle snap 0, turn off angle snap and show stock circle sprite
					if (editor.srfAttachAngleSnap == 0) {
						GameSettings.VAB_USE_ANGLE_SNAP = false;
						//set playanim index and unhide stock sprite
						//editor.angleSnapSprite.PlayAnim (0);
						//editor.angleSnapSprite.Hide (false);
					} else {
						GameSettings.VAB_USE_ANGLE_SNAP = true;
						//angle snap is on, re-hide stock sprite
						//editor.angleSnapSprite.Hide (true);
					}
	
					Log.Debug ("Exiting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString ());
					return;
	
				}
	
				// X, Shift+X : Increment/decrement symmetry mode
				if (Input.GetKeyDown (cfg.KeyMap.Symmetry)) {

					//only inc/dec symmetry in radial mode, mirror is just 1&2
					if (editor.symmetryMethod == SymmetryMethod.Radial) {
						if (altKeyDown || (_symmetryMode < 2 && shiftKeyDown)) {
							//Alt+X or Symmetry is at 1(index 2) or lower
							_symmetryMode = 0;
						} else if (_symmetryMode > cfg.MaxSymmetry - 2 && !shiftKeyDown) {
							//Stop adding at max symmetry
							_symmetryMode = cfg.MaxSymmetry - 1;
						} else {
							//inc/dec symmetry
							_symmetryMode = _symmetryMode + (shiftKeyDown ? -1 : 1);
						}
						editor.symmetryMode = _symmetryMode;
						Log.Debug ("Setting symmetry to " + _symmetryMode.ToString ());
					} else {
						//editor.symmetryMethod == SymmetryMethod.Mirror
						//update var with stock action's result
						_symmetryMode = editor.symmetryMode;
					}
				}

			}//end if(enableHotKeys)
		}

		#region GUI

		private Rect _settingsWindowRect;
		GUISkin skin = null;
		/// <summary>
		/// Init styles & rects for GUI items
		/// </summary>
		void InitializeGUI ()
		{
			//use KSP's unity skin
			skin = HighLogic.Skin;

			_settingsWindowRect = new Rect (){
				xMin = Screen.width - 400,
				xMax = Screen.width - 100,
				yMin = Screen.height/2,
				yMax = Screen.height/2 //0 height, GUILayout resizes it
			};

			GUIStyle osdLabel = new GUIStyle () {
				stretchWidth = true,
				stretchHeight = true,
				alignment = TextAnchor.MiddleCenter,
				fontSize = 22,
				fontStyle = FontStyle.Bold,
				name = "OSDLabel"
			};
			osdLabel.normal.textColor = Color.yellow;

			GUIStyle symmetryLabel = new GUIStyle () {
				stretchWidth = true,
				stretchHeight = true,
				alignment = TextAnchor.MiddleCenter,
				fontSize = 18,
				fontStyle = FontStyle.Bold,
				name = "SymmetryLabel"
			};
			symmetryLabel.normal.textColor = Color.yellow;

			skin.customStyles = new GUIStyle[]{ osdLabel, symmetryLabel };
		}

		KeyCode _lastKeyDown = KeyCode.None;
		/// <summary>
		/// Unity GUI paint event, fired every screen refresh
		/// </summary>
		public void OnGUI ()
		{	
			//apply skin
			GUI.skin = skin;

			//show on-screen messages
			DisplayOSD ();

			//show and update the angle snap and symmetry mode labels
			ShowSnapLabels ();

			//GUI test
			if (Event.current.type == EventType.Layout) {
				ShowWindows();
			}

			if (GUI.changed)
			{
				Log.Debug ("GUI.Changed");
			}
				
			//get current keypress
			if (Event.current.isKey) {

				_lastKeyDown = Event.current.keyCode;
			}

		}
			
		private bool _showSettings = false;
		void ShowWindows()
		{
			_showSettings = this.Visible;

			if (_showSettings) {
				_settingsWindowRect = GUILayout.Window (500, _settingsWindowRect, SettingsWindowContent, "Editor Extensions Settings");
			}
		}

		//private int _toolbarInt = 0;
		//private string[] _toolbarStrings = {"Toolbar1", "Toolbar2", "Toolbar3"};
		void SettingsWindowContent (int windowID) {

			GUILayout.BeginVertical();

			//_toolbarInt = GUILayout.Toolbar (_toolbarInt, _toolbarStrings);

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Version:");
			GUILayout.Label (cfg.FileVersion);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Current key:");
			GUILayout.Label (_lastKeyDown.ToString(), "TextField");
			GUILayout.EndHorizontal ();

			if (EditorLogic.SelectedPart) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Selected Part:");
				GUILayout.Label (EditorLogic.SelectedPart ? EditorLogic.SelectedPart.name : "none");
				GUILayout.EndHorizontal ();

				GUILayout.Label ("allowSrfAttach " + (EditorLogic.SelectedPart.attachRules.allowSrfAttach ? "enabled" : "disabled"));
				GUILayout.Label ("srfAttach " + (EditorLogic.SelectedPart.attachRules.srfAttach ? "enabled" : "disabled"));
				GUILayout.Label ("allowCollision " + (EditorLogic.SelectedPart.attachRules.allowCollision ? "enabled" : "disabled"));
				GUILayout.Label ("allowStack " + (EditorLogic.SelectedPart.attachRules.allowStack ? "enabled" : "disabled"));
				GUILayout.Label ("allowDock " + (EditorLogic.SelectedPart.attachRules.allowDock ? "enabled" : "disabled"));
				GUILayout.Label ("allowRotate " + (EditorLogic.SelectedPart.attachRules.allowRotate ? "enabled" : "disabled"));
				GUILayout.Label ("stack " + (EditorLogic.SelectedPart.attachRules.stack ? "enabled" : "disabled"));
			}

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Message delay:");
			if (GUILayout.Button ("-")) {
				cfg.OnScreenMessageTime -= 0.5f;
			}
			GUILayout.Label (cfg.OnScreenMessageTime.ToString(), "TextField");
			if (GUILayout.Button ("+")) {
				cfg.OnScreenMessageTime += 0.5f;
			}
			GUILayout.EndHorizontal ();

//			GUILayout.BeginHorizontal ();
//			GUILayout.Label ("Max symmetry:");
//			string maxSym = GUILayout.TextField (cfg.MaxSymmetry.ToString());
//			int newMaxSym = cfg.MaxSymmetry;
//			if (Int32.TryParse (maxSym, out newMaxSym)) {
//				cfg.MaxSymmetry = newMaxSym;
//			}
//			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Max symmetry:");
			if (GUILayout.Button ("-")) {
				cfg.MaxSymmetry--;
			}
			GUILayout.Label (cfg.MaxSymmetry.ToString(), "TextField");
			if (GUILayout.Button ("+")) {
				cfg.MaxSymmetry++;
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if(GUILayout.Button("Cancel")){
				cfg = ConfigManager.LoadConfig (_configFilePath);
				_showSettings = false;
			}
			if(GUILayout.Button("Defaults")){
				cfg = CreateDefaultConfig ();
			}
			if(GUILayout.Button("Save")){
				ConfigManager.SaveConfig (cfg, _configFilePath);
				_showSettings = false;
			}
			GUILayout.EndHorizontal();


			GUILayout.EndVertical();

			GUI.DragWindow();
		}

		/// <summary>
		/// Set a on screen display message with the default duration
		/// </summary>
		/// <param name="message">Message string</param>
		void OSDMessage (string message)
		{
			OSDMessage (message, cfg.OnScreenMessageTime);
		}

		float messageCutoff = 0;
		string messageText = "";
		/// <summary>
		/// Set a on screen display message
		/// </summary>
		/// <param name="message">Message string</param>
		/// <param name="delay">Amount of time to display the message in seconds</param>
		void OSDMessage (string message, float delay)
		{
			messageCutoff = Time.time + delay;
			messageText = message;
			Log.Debug (String.Format ("OSD messageCutoff = {0}, messageText = {1}", messageCutoff.ToString (), messageText));
		}

		/// <summary>
		/// check for On Screen Display message
		/// </summary>
		void DisplayOSD ()
		{
			if (Time.time < messageCutoff) {
				GUILayout.BeginArea (new Rect (0, (Screen.height / 4), Screen.width, 200));
				GUILayout.Label (messageText, "OSDLabel");
				GUILayout.EndArea ();			
			}
		}

		string symmetryLabelValue = string.Empty;

		//symmetry & angle sprite/label size and position
		const int advancedModeOffset = 34;
		const int angleSnapLabelSize = 43;
		const int angleSnapLabelLeftOffset = 209;
		const int angleSnapLabelBottomOffset = 61;
		const int symmetryLabelSize = 56;
		const int symmetryLabelLeftOffset = 152;
		const int symmetryLabelBottomOffset = 63;
		Rect angleSnapLabelRect = new Rect () {
			xMin = angleSnapLabelLeftOffset,
			xMax = angleSnapLabelLeftOffset + angleSnapLabelSize,
			yMin = Screen.height - angleSnapLabelBottomOffset,
			yMax = Screen.height - angleSnapLabelBottomOffset + angleSnapLabelSize
		};
		Rect symmetryLabelRect = new Rect () {
			xMin = symmetryLabelLeftOffset,
			xMax = symmetryLabelLeftOffset + symmetryLabelSize,
			yMin = Screen.height - symmetryLabelBottomOffset,
			yMax = Screen.height - symmetryLabelBottomOffset + symmetryLabelSize
		};

		/// <summary>
		/// Hides the stock angle & symmetry sprites and replaces with textual labels
		/// </summary>
		private void ShowSnapLabels ()
		{
			//Only show angle/symmetry sprites on parts tab
			if (editor.editorScreen == EditorScreen.Parts) {
				if (EditorLogic.Mode == EditorLogic.EditorModes.ADVANCED) {
					//in advanced mode, shift labels to the right
					angleSnapLabelRect.xMin = angleSnapLabelLeftOffset + advancedModeOffset;
					angleSnapLabelRect.xMax = angleSnapLabelLeftOffset + angleSnapLabelSize + advancedModeOffset;
					symmetryLabelRect.xMin = symmetryLabelLeftOffset + advancedModeOffset;
					symmetryLabelRect.xMax = symmetryLabelLeftOffset + symmetryLabelSize + advancedModeOffset;
				} else {
					//EditorLogic.EditorModes.SIMPLE
					//in simple mode, set back to left position
					angleSnapLabelRect.xMin = angleSnapLabelLeftOffset;
					angleSnapLabelRect.xMax = angleSnapLabelLeftOffset + angleSnapLabelSize;
					symmetryLabelRect.xMin = symmetryLabelLeftOffset;
					symmetryLabelRect.xMax = symmetryLabelLeftOffset + symmetryLabelSize;
				}

				//Radial mode 'number+R', mirror mode is 'M'/'MM'
				if (editor.symmetryMethod == SymmetryMethod.Radial) {
					symmetryLabelValue = (editor.symmetryMode + 1) + "R";
				} else if (editor.symmetryMethod == SymmetryMethod.Mirror) {
					symmetryLabelValue = (editor.symmetryMode == 0) ? "M" : "MM";
				}

				//always hide stock symmetry and mirror sprites
				editor.symmetrySprite.Hide (true);
				editor.mirrorSprite.Hide (true);

				// Show Symmetry label
				GUI.Label (symmetryLabelRect, symmetryLabelValue, "SymmetryLabel");

				//if angle snap is on hide stock sprite
				if (GameSettings.VAB_USE_ANGLE_SNAP) {
					editor.angleSnapSprite.Hide (true);
					GUI.Label (angleSnapLabelRect, editor.srfAttachAngleSnap + degreesSymbol, "SymmetryLabel");

				} else {
					//angle snap is off, show stock sprite
					editor.angleSnapSprite.PlayAnim (0);
					editor.angleSnapSprite.Hide (false);
				}
			}
		}

		#endregion
	}
}
