using System;
using UnityEngine;
using KSP.IO;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace EditorExtensions
{
	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public class EditorExtensions : MonoBehaviour
	{
		public static EditorExtensions Instance { get; private set; }

		public bool Visible { get; set; }

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
					AngleSnapValues = new List<float>{ 0.0f, 1.0f, 5.0f, 15.0f, 22.5f, 30.0f, 45.0f, 60.0f, 90.0f },
					MaxSymmetry = 20,
					FileVersion = pluginVersion.ToString (),
					OnScreenMessageTime = 1.5f
				};

				KeyMaps defaultKeys = new KeyMaps () {
					AngleSnap = KeyCode.C,
					AttachmentMode = KeyCode.T,
					PartClipping = KeyCode.Z,
					ResetCamera = KeyCode.Space,
					Symmetry = KeyCode.X,
					VerticalSnap = KeyCode.V,
					HorizontalSnap = KeyCode.H
				};
				defaultConfig.KeyMap = defaultKeys;

				if (ConfigManager.SaveConfig (defaultConfig, _configFilePath))
					Log.Debug ("Created default config");
				else
					Log.Error ("Failed to save default config");

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

		private Part GetPartUnderCursor ()
		{
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			EditorLogic ed = EditorLogic.fetch;

			if (ed != null && Physics.Raycast (ray, out hit)) {
				return ed.ship.Parts.Find (p => p.gameObject == hit.transform.gameObject);
			}
			return null;
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

			if (editor.shipNameField.Focused || editor.shipDescriptionField.Focused)
				return;
	
			//check for the various alt/mod etc keypresses
			altKeyDown = Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt) || Input.GetKey (KeyCode.AltGr);
			//check for shift key
			shiftKeyDown = Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift);

			//look into skewing camera	

			//hotkeyed editor functions
			if (enableHotkeys) {
			
				// V - Vertically align part under cursor with the part it is attached to
				if (Input.GetKeyDown (cfg.KeyMap.VerticalSnap)) {

					try {
						Part sp = GetPartUnderCursor ();

						if (sp != null && sp.srfAttachNode != null && sp.srfAttachNode.attachedPart != null) {

							Part ap = sp.srfAttachNode.attachedPart;
							List<Part> symParts = sp.symmetryCounterparts;

							Log.Debug ("symmetryCounterparts to move: " + symParts.Count.ToString ());

							//move hovered part
							sp.transform.position = new Vector3 (sp.transform.position.x, ap.transform.position.y, sp.transform.position.z);
							sp.attPos0.y = ap.transform.position.y;

							//move any symmetry siblings/counterparts
							foreach (Part symPart in symParts) {
								symPart.transform.position = new Vector3 (symPart.transform.position.x, ap.transform.position.y, symPart.transform.position.z);
								symPart.attPos0.y = ap.transform.position.y;
							}
						}
					} catch (Exception ex) {
						Log.Error ("Error trying to vertically align: " + ex.Message);
					}

					//OSDMessage ("Vertical snap " + (GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ? "enabled" : "disabled"), 1);
					//Log.Debug ("Vertical snap " + (GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ? "enabled" : "disabled"));
					return;
				}

				// H - Horizontally align part under cursor with the part it is attached to
				if (Input.GetKeyDown (cfg.KeyMap.HorizontalSnap)) {

					try {
						Part sp = GetPartUnderCursor ();

						if (sp != null && sp.srfAttachNode != null && sp.srfAttachNode.attachedPart != null) {

							Part ap = sp.srfAttachNode.attachedPart;
							List<Part> symParts = sp.symmetryCounterparts;

							Log.Debug ("symmetryCounterparts to move: " + symParts.Count.ToString ());

							//move selected part
							sp.transform.position = new Vector3 (sp.transform.position.x, sp.transform.position.y, ap.transform.position.z);
							sp.attPos0.z = ap.transform.position.z;

							//move any symmetry siblings/counterparts
							foreach (Part symPart in symParts) {
								symPart.transform.position = new Vector3 (symPart.transform.position.x, symPart.transform.position.y, ap.transform.position.z);
								symPart.attPos0.z = ap.transform.position.z;
							}
						}
					} catch (Exception ex) {
						Log.Error ("Error trying to Horizontally align: " + ex.Message);
					}
					return;
				}


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
					OSDMessage ("Part clipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"));
					return;
				}
	
				// C, Shift+C : Increment/Decrement Angle snap
				if (Input.GetKeyDown (cfg.KeyMap.AngleSnap)) {
	
					if (!altKeyDown) {
						Log.Debug ("Starting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString ());
	
						int currentAngleIndex = cfg.AngleSnapValues.IndexOf (editor.srfAttachAngleSnap);
	
						Log.Debug ("currentAngleIndex: " + currentAngleIndex.ToString ());
	
						//rotate through the angle snap values
						float newAngle;
						if (shiftKeyDown) {
							//lower snap
							newAngle = cfg.AngleSnapValues [currentAngleIndex == 0 ? cfg.AngleSnapValues.Count - 1 : currentAngleIndex - 1];
						} else {
							//higher snap
							//Log.Debug ("new AngleIndex: " + (currentAngleIndex == angleSnapValues.Length - 1 ? 0 : currentAngleIndex + 1).ToString ());
							newAngle = cfg.AngleSnapValues [currentAngleIndex == cfg.AngleSnapValues.Count - 1 ? 0 : currentAngleIndex + 1];
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

			_settingsWindowRect = new Rect () {
				xMin = Screen.width - 350,
				xMax = Screen.width - 50,
				yMin = 50,
				yMax = 50 //0 height, GUILayout resizes it
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

		KeyCode lastKeyPressed = KeyCode.None;

		/// <summary>
		/// Unity GUI paint event, fired every screen refresh
		/// </summary>
		public void OnGUI ()
		{	
			//apply skin
			GUI.skin = skin;

			//get current keypress

			if (Event.current.isKey) { 
				lastKeyPressed = Event.current.keyCode;
			} else {
				//currentKey = KeyCode.None;
			}

			//show on-screen messages
			DisplayOSD ();

			//show and update the angle snap and symmetry mode labels
			ShowSnapLabels ();

			//show windows, only on Unity Layout pass
			if (Event.current.type == EventType.Layout) {
				ShowWindows ();
			}
		}

		//private bool _showSettings = false;

		void ShowWindows ()
		{
			//_showSettings = this.Visible;

			string windowTitle = string.Format ("Editor Extensions v{0}.{1}", pluginVersion.Major.ToString (), pluginVersion.Minor.ToString ());

			if (this.Visible) {
				//collapse window height so that it resizes to content
				_settingsWindowRect.yMax = _settingsWindowRect.yMin;
				_settingsWindowRect = GUILayout.Window (500, _settingsWindowRect, SettingsWindowContent, windowTitle);
			}
		}

		private int toolbarInt = 0;
		private string[] _toolbarStrings = { "Settings", "Angle Snap", "Debug" };
		string keyMapToUpdate = string.Empty;
		string newAngleString = string.Empty;
		public int angleGridIndex = -1;
		public string[] angleStrings = new string[] { string.Empty };
		object anglesLock = new object ();
		GUILayoutOption[] settingsLabelLayout = new GUILayoutOption[] { GUILayout.MinWidth (150) };

		void SettingsWindowContent (int windowID)
		{
			toolbarInt = GUILayout.Toolbar (toolbarInt, _toolbarStrings);

			GUILayout.BeginVertical ("box");

			//settings
			if (toolbarInt == 0) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Message delay:", settingsLabelLayout);
				if (GUILayout.Button ("-")) {
					cfg.OnScreenMessageTime -= 0.5f;
				}
				GUILayout.Label (cfg.OnScreenMessageTime.ToString (), "TextField");
				if (GUILayout.Button ("+")) {
					cfg.OnScreenMessageTime += 0.5f;
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Max symmetry:", settingsLabelLayout);
				if (GUILayout.Button ("-")) {
					cfg.MaxSymmetry--;
				}
				GUILayout.Label (cfg.MaxSymmetry.ToString (), "TextField");
				if (GUILayout.Button ("+")) {
					cfg.MaxSymmetry++;
				}
				GUILayout.EndHorizontal ();

				if (keyMapToUpdate == string.Empty) {
					GUILayout.Label ("Click button and press key to change");
				} else {
					GUILayout.Label ("Waiting for key");
				}

#if DEBUG
				GUILayout.Label ("lastKeyPressed: " + lastKeyPressed.ToString ());
#endif

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Surface attachment:", settingsLabelLayout);
				if (keyMapToUpdate == "am" && lastKeyPressed != KeyCode.None) {
					cfg.KeyMap.AttachmentMode = lastKeyPressed;
					keyMapToUpdate = string.Empty;
				}
				if (GUILayout.Button (cfg.KeyMap.AttachmentMode.ToString ())) {
					lastKeyPressed = KeyCode.None;
					keyMapToUpdate = "am";
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Vertical snap:", settingsLabelLayout);
				if (keyMapToUpdate == "vs" && lastKeyPressed != KeyCode.None) {
					cfg.KeyMap.VerticalSnap = lastKeyPressed;
					keyMapToUpdate = string.Empty;
				}
				if (GUILayout.Button (cfg.KeyMap.VerticalSnap.ToString ())) {
					lastKeyPressed = KeyCode.None;
					keyMapToUpdate = "vs";
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Horizontal snap:", settingsLabelLayout);
				if (keyMapToUpdate == "hs" && lastKeyPressed != KeyCode.None) {
					cfg.KeyMap.HorizontalSnap = lastKeyPressed;
					keyMapToUpdate = string.Empty;
				}
				if (GUILayout.Button (cfg.KeyMap.HorizontalSnap.ToString ())) {
					lastKeyPressed = KeyCode.None;
					keyMapToUpdate = "hs";
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Reset camera:", settingsLabelLayout);
				if (keyMapToUpdate == "rc" && lastKeyPressed != KeyCode.None) {
					cfg.KeyMap.ResetCamera = lastKeyPressed;
					keyMapToUpdate = string.Empty;
				}
				if (GUILayout.Button (cfg.KeyMap.ResetCamera.ToString ())) {
					lastKeyPressed = KeyCode.None;
					keyMapToUpdate = "rc";
				}
				GUILayout.EndHorizontal ();
			}//end settings

			//angle snap values settings
			if (toolbarInt == 1) {

				try {
					//float[] tmpAngles;
					//cfg.AngleSnapValues.CopyTo (tmpAngles);

					lock (anglesLock) {
						foreach (float a in cfg.AngleSnapValues) {
							if (a != 0.0f) {
								GUILayout.BeginHorizontal ();
								GUILayout.Label (a.ToString (), settingsLabelLayout);
								if (GUILayout.Button ("Remove")) {
									cfg.AngleSnapValues.Remove (a);
								}
								GUILayout.EndHorizontal ();
							}
						}
					}

					//above creating this error when list is modified
					//[EXC 15:41:29.330] InvalidOperationException: Collection was modified; enumeration operation may not execute.
					//System.Collections.Generic.List`1+Enumerator[System.Single].VerifyState ()
					//System.Collections.Generic.List`1+Enumerator[System.Single].MoveNext ()
					//EditorExtensions.EditorExtensions.SettingsWindowContent (Int32 windowID)
					//UnityEngine.GUILayout+LayoutedWindow.DoWindow (Int32 windowID)
					//UnityEngine.GUI.CallWindowDelegate (UnityEngine.WindowFunction func, Int32 id, UnityEngine.GUISkin _skin, Int32 forceRect, Single width, Single height, UnityEngine.GUIStyle style)

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Add angle: ");
					newAngleString = GUILayout.TextField (newAngleString);
					if (GUILayout.Button ("Add")) {
						float newAngle = 0.0f;

						if (!string.IsNullOrEmpty (newAngleString) && float.TryParse (newAngleString, out newAngle)) {
							lock (anglesLock) {
								if (newAngle > 0.0f && newAngle <= 90.0f && cfg.AngleSnapValues.IndexOf (newAngle) == -1) {
									cfg.AngleSnapValues.Add (newAngle);
									cfg.AngleSnapValues.Sort ();
								}
							}
						}
					
					}
					GUILayout.EndHorizontal ();

				} catch (Exception ex) {
					//potential for some intermittent locking/threading issues here
					//just ignore the error and continue since it's non-critical
#if DEBUG
					//Debug only to avoid log spam
					Log.Error ("Error updating AngleSnapValues: " + ex.Message);
#endif
				}
			}//end angles



//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Surface attachment:");
//				GUI.SetNextControlName ("AttachmentMode");
//				string tmpAttachmentMode = cfg.KeyMap.AttachmentMode.ToString ();
//				tmpAttachmentMode = GUILayout.TextField (tmpAttachmentMode);
//
//				if (tmpAttachmentMode.Length == 1) {
//					try {
//						KeyCode newAttachmentMode = (KeyCode)Enum.Parse (typeof(KeyCode), tmpAttachmentMode);
//						cfg.KeyMap.AttachmentMode = newAttachmentMode;
//					} catch {
//						//ignore
//						Log.Error ("Invalid value for cfg.KeyMap.AttachmentMode: " + tmpAttachmentMode);
//					}
//				}
//
//				GUILayout.EndHorizontal ();

			//			GUILayout.BeginHorizontal ();
			//			GUILayout.Label ("Max symmetry:");
			//			string maxSym = GUILayout.TextField (cfg.MaxSymmetry.ToString());
			//			int newMaxSym = cfg.MaxSymmetry;
			//			if (Int32.TryParse (maxSym, out newMaxSym)) {
			//				cfg.MaxSymmetry = newMaxSym;
			//			}
			//			GUILayout.EndHorizontal ();
		
	

			//debug info
			if (toolbarInt == 2) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Version:", settingsLabelLayout);
				GUILayout.Label (cfg.FileVersion);
				GUILayout.EndHorizontal ();

				//Get selected part, mouseover part if none is active
				Part sp = EditorLogic.SelectedPart;
				if (sp == null)
					sp = GetPartUnderCursor ();

				if (sp != null) {

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Current Part:");
					GUILayout.Label (sp ? sp.name : "none");
					GUILayout.EndHorizontal ();

					//GUILayout.Label ("allowSrfAttach " + (EditorLogic.SelectedPart.attachRules.allowSrfAttach ? "enabled" : "disabled"));
					GUILayout.Label ("srfAttach " + (sp.attachRules.srfAttach ? "enabled" : "disabled"));
					//GUILayout.Label ("allowCollision " + (EditorLogic.SelectedPart.attachRules.allowCollision ? "enabled" : "disabled"));
					//GUILayout.Label ("allowStack " + (EditorLogic.SelectedPart.attachRules.allowStack ? "enabled" : "disabled"));
					//GUILayout.Label ("allowDock " + (EditorLogic.SelectedPart.attachRules.allowDock ? "enabled" : "disabled"));
					//GUILayout.Label ("allowRotate " + (EditorLogic.SelectedPart.attachRules.allowRotate ? "enabled" : "disabled"));
					//GUILayout.Label ("stack " + (EditorLogic.SelectedPart.attachRules.stack ? "enabled" : "disabled"));

					foreach (var child in sp.children) {
						GUILayout.Label ("child: " + child.name);
					}

					GUILayout.Label ("localPosition " + sp.transform.localPosition.ToString ());
					GUILayout.Label ("position " + sp.transform.position.ToString ());
					GUILayout.Label ("rotation " + sp.transform.rotation.ToString ());
					GUILayout.Label ("attRotation: " + sp.attRotation.ToString ());
					GUILayout.Label ("attRotation0: " + sp.attRotation0.ToString ());
					//attPos doesnt seem to be used
					GUILayout.Label ("attPos: " + sp.attPos.ToString ());
					GUILayout.Label ("attPos0: " + sp.attPos0.ToString ());
					GUILayout.Label ("isAttached " + sp.isAttached.ToString ());

					if (sp.srfAttachNode != null) {
						GUILayout.Label ("srfAttachNode.position: " + sp.srfAttachNode.position.ToString ());

						GUILayout.BeginVertical ("box");
						GUILayout.Label ("Attached part:");
						if (sp.srfAttachNode.attachedPart != null) {
							GUILayout.Label ("attPos0: " + sp.srfAttachNode.attachedPart.attPos0.ToString ());
							GUILayout.Label ("localPosition " + sp.srfAttachNode.attachedPart.transform.localPosition.ToString ());
							GUILayout.Label ("position " + sp.srfAttachNode.attachedPart.transform.position.ToString ());
							GUILayout.Label ("rotation " + sp.srfAttachNode.attachedPart.transform.rotation.ToString ());
							GUILayout.Label ("up " + sp.srfAttachNode.attachedPart.transform.up.ToString ());

							AttachNode an = sp.srfAttachNode.attachedPart.attachNodes [0];
							GUILayout.Label ("attachNode " + an.position.ToString ());
							//sp.attPos0.y = sp.srfAttachNode.attachedPart.attPos0.y;
						}

						GUILayout.EndVertical ();
					}
		
				}
			}

			GUILayout.EndVertical ();//end main content

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Close")) {
				cfg = ConfigManager.LoadConfig (_configFilePath);
				this.Visible = false;
			}
			//don't show on debug tab
			if (toolbarInt != 2) {
				if (GUILayout.Button ("Defaults")) {
					cfg = CreateDefaultConfig ();
				}
				if (GUILayout.Button ("Save")) {
					ConfigManager.SaveConfig (cfg, _configFilePath);
					this.Visible = false;
				}
			}
			GUILayout.EndHorizontal ();

			GUI.DragWindow ();
		}

		float messageCutoff = 0;
		string messageText = "";

		/// <summary>
		/// Set a on screen display message
		/// </summary>
		/// <param name="message">Message string</param>
		/// <param name="delay">Amount of time to display the message in seconds</param>
		void OSDMessage (string message)
		{
			messageCutoff = Time.time + cfg.OnScreenMessageTime;
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
