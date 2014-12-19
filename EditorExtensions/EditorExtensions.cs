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
		#region member vars

		//current vars
		string pluginDirectory;
		KeyCode keyMapResetCamera = KeyCode.Space;
		KeyCode keyMapSurfaceAttachment = KeyCode.T;
		//look into loading game keymaps for applying alt+shift modifiers
		const string degreesSymbol = "\u00B0";	
		int _symmetryMode = 0;
		int maxSymmetryMode = 99;
		static float[] angleSnapValues = { 0, 1, 5, 15, 30, 45, 60, 90 };

		//old vars
		//const string launchSiteName_LaunchPad = "LaunchPad";
		//const string launchSiteName_Runway = "Runway";	
		//bool ignoreHotKeys = false;	
		//bool inVAB = false;
	
		EditorLogic editor;
	
		Rect symmetryLabelRect;
		Rect angleSnapLabelRect;

		#endregion

		#region logging

		//set debug flag to toggle debugging messages
		#if DEBUG
		const bool debug = true;
		#else
		const bool debug = false;
		#endif

		void DebugMsg (string message)
		{
			if (debug)
				print ("EditorExtensions: " + message);
		}

		void ErrorMsg (string message)
		{
			print ("EditorExtensions: " + message);
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="EditorExtensions.EditorExtensions"/> class.
		/// </summary>
		public EditorExtensions ()
		{
			//Get plugin's dll version
			try {
				Assembly execAssembly = Assembly.GetExecutingAssembly ();
				string assemblyVersion = execAssembly.GetName ().Version.ToString ();
				pluginDirectory = Path.GetDirectoryName (execAssembly.Location);

				DebugMsg ("Initializing version " + assemblyVersion);
			} catch (Exception ex) {
				DebugMsg ("Unable to get assembly version: " + ex.Message);
			}
		}

		void InitConfig(){
			PluginConfiguration cfg = PluginConfiguration.CreateForType<EditorExtensions> ();
			try {
				cfg.load ();

				ConfigNode cfgNode = new ConfigNode ("settings");
				cfgNode.AddValue ("nodeName", "nodeValue");

				cfgNode.Save ("EditorExtensions.cfg");

				//Check to see what is returned when config isnt there
				if (cfg == null) {
					DebugMsg ("cfg is null");
				} else {
					DebugMsg ("cfg is not null");
				}

				DebugMsg ("Loaded Config: typeof" + cfg.GetType ().ToString ());

			} catch (Exception ex) {
				ErrorMsg ("Error loading config: " + ex.Message);
			}

			try {
				cfg ["stringsetting"] = "string setting value";
				cfg ["SomeVector"] = new Vector3d (0, 1, 2);
				cfg ["testarray"] = new int[]{ 42, 42, 42 };
				cfg.SetValue ("testarray2", new int[]{ 11, 22, 33 });
				cfg.save ();

				DebugMsg ("Saved config");

			} catch (Exception ex) {
				ErrorMsg ("Error saving config: " + ex.Message);
			}

			try {
				//check what is returned when a setting doesnt exist. null?
				string val = string.Empty;
				val = cfg.GetValue<string> ("notthere");

				if (val == null) {
					DebugMsg ("Val is null");
				} else {
					DebugMsg ("Get value: '" + val + "' type: " + val.GetType ().ToString ());
				}



			} catch (Exception ex) {
				ErrorMsg ("Error getting config value: " + ex.Message);
			}
		}
	
		//Unity initialization call
		public void Awake ()
		{
			DebugMsg ("Awake() initializing");
	
			editor = EditorLogic.fetch;
	
			editor.symmetrySprite.Hide (true);
			editor.mirrorSprite.Hide (true);
	
			//Rects for symmetry/angle snap labels
			symmetryLabelRect = new Rect (150, Screen.height - 65, 57, 57);
			angleSnapLabelRect = new Rect (206, Screen.height - 63, 46, 46);
	
			InitStyles ();
	
			//Disable shortcut keys when ship name textarea has focus
		}

		//Broken
		const string VABGameObjectName = "interior_vehicleassembly";
		const string SPHGameObjectName = "xport_sph3";

		void AlterEditorSpace (EditorLogic editor)
		{	
			// Modify cameras/available interior space
			if (HighLogic.LoadedScene == GameScenes.EDITOR) {
				DebugMsg ("Updating VAB dimensions and camera");
	
				VABCamera VABcam = Camera.main.GetComponent<VABCamera> ();
				VABcam.maxHeight = 2000;
				VABcam.maxDistance = 2000;
	
				GameObject interior = GameObject.Find (VABGameObjectName);
				interior.transform.localScale = new Vector3 (2.2f, 1.8f, 1.8f);
				interior.transform.position = new Vector3 (59f, 51.5f, 12);
			}
//			else if (HighLogic.LoadedScene == GameScenes.SPH)
//			{
//				DebugMsg ("Updating SPH dimensions and camera");
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

		public void Update ()
		{		
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
	
			//Space - when no part is selected, reset camera
			if (Input.GetKeyDown (keyMapResetCamera) && !EditorLogic.SelectedPart) {
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
//				//DebugMsg ("Toggling vertical snap");
//				GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ^= true;
//	
//				//if normal radial angle snap is currently off, vertical snap will have no effect unless it is re-enabled
//				//automatically set aangle snap to minimum - some people thought vert snap was broken in this situation, the game doesn't appear to allow it
//				if (GameSettings.VAB_USE_ANGLE_SNAP == false && GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL == true) {
//					DebugMsg ("Enabling angle snap to allow vertical snap to work");
//					//angle snap needs be > 0, otherwise log is spammed with DivideByZero errors
//					if (editor.srfAttachAngleSnap == 0)
//						editor.srfAttachAngleSnap = 1;
//					GameSettings.VAB_USE_ANGLE_SNAP = true;
//				}
//				OSDMessage ("Vertical snap " + (GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ? "enabled" : "disabled"), 1);
//				DebugMsg ("Vertical snap " + (GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ? "enabled" : "disabled"));
//				return;
//			}
	
			//look into also toggling  editor.allowNodeAttachment
			//EditorLogic.SelectedPart.attachRules.allowStack

			// T: Surface attachment and node attachment toggle
			if (Input.GetKeyDown (keyMapSurfaceAttachment)) {

				Part selectedPart = EditorLogic.SelectedPart;

				if (selectedPart) {
					//Toggle surface attachment for selected part
					selectedPart.attachRules.srfAttach ^= true;

					//set global toggling to match
					editor.allowSrfAttachment = selectedPart.attachRules.srfAttach;
					editor.allowNodeAttachment = !selectedPart.attachRules.srfAttach;

					DebugMsg ("Toggling srfAttach for " + EditorLogic.SelectedPart.name);
					OSDMessage (String.Format ("Surface attachment {0} \n Node attachment {1} \n for {2}"
						, selectedPart.attachRules.srfAttach ? "enabled" : "disabled"
						, editor.allowNodeAttachment ? "enabled" : "disabled"
						, selectedPart.name
					), 1);
				} else {
					//just toggle global surface attachment, parts whose config do not allow it are unaffected
					editor.allowSrfAttachment ^= true;
					editor.allowNodeAttachment = !editor.allowSrfAttachment;
					OSDMessage (String.Format ("Surface attachment {0} \n Node attachment {1}"
						, editor.allowSrfAttachment ? "enabled" : "disabled"
						, editor.allowNodeAttachment ? "enabled" : "disabled"
					), 1);
				}
			}
	
			// ALT+Z : Toggle part clipping (From cheat options)
			if (altKeyDown && Input.GetKeyDown (KeyCode.Z)) {
				CheatOptions.AllowPartClipping ^= true;
				DebugMsg ("AllowPartClipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"));
				OSDMessage ("Part clipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"), 1);
				return;
			}
	
			// C, Shift+C : Increment/Decrement Angle snap
			if (Input.GetKeyDown (KeyCode.C)) {
	
				if (!altKeyDown) {
					//GameSettings.VAB_USE_ANGLE_SNAP = false;
					DebugMsg ("Starting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString ());
	
					int currentAngleIndex = Array.IndexOf (angleSnapValues, editor.srfAttachAngleSnap);
	
					DebugMsg ("currentAngleIndex: " + currentAngleIndex.ToString ());
	
					float newAngle;
					if (shiftKeyDown) {
						newAngle = angleSnapValues [currentAngleIndex == 0 ? angleSnapValues.Length - 1 : currentAngleIndex - 1];
					} else {
						DebugMsg ("new AngleIndex: " + (currentAngleIndex == angleSnapValues.Length - 1 ? 0 : currentAngleIndex + 1).ToString ());
						newAngle = angleSnapValues [currentAngleIndex == angleSnapValues.Length - 1 ? 0 : currentAngleIndex + 1];
					}
	
					DebugMsg ("Setting srfAttachAngleSnap to " + newAngle.ToString ());
					editor.srfAttachAngleSnap = newAngle;
				} else {
					DebugMsg ("Resetting srfAttachAngleSnap to 0");
					editor.srfAttachAngleSnap = 0;
				}
	
				if (editor.srfAttachAngleSnap == 0) {
					GameSettings.VAB_USE_ANGLE_SNAP = false;
					//Vertical snap doesn't work when angle snap is disabled.
					//Resetting it here so that the toggle logic for vert snap maintains state
					GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL = false;
					editor.angleSnapSprite.PlayAnim (0);
				} else {
					GameSettings.VAB_USE_ANGLE_SNAP = true;
				}
	
				DebugMsg ("Exiting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString ());
				return;
	
			}
	
			// X, Shift+X : Increment/decrement symmetry mode
			if (Input.GetKeyDown (KeyCode.X)) {

				//only inc/dec symmetry in radial mode, mirror is just 1&2
				if (editor.symmetryMethod == SymmetryMethod.Radial) {
					if (altKeyDown || (_symmetryMode < 2 && shiftKeyDown)) {
						//Alt+X or Symmetry is at 1(index 2) or lower
						_symmetryMode = 0;
					} else if (_symmetryMode > maxSymmetryMode - 2 && !shiftKeyDown) {
						//Stop adding at max symmetry
						_symmetryMode = maxSymmetryMode - 1;
					} else {
						//inc/dec symmetry
						_symmetryMode = _symmetryMode + (shiftKeyDown ? -1 : 1);
					}
					editor.symmetryMode = _symmetryMode;
					DebugMsg ("Setting symmetry to " + _symmetryMode.ToString ());
				} else {
					//editor.symmetryMethod == SymmetryMethod.Mirror
					//update var with stock action's result
					_symmetryMode = editor.symmetryMode;
				}
			}
		}

		#region GUI

		GUIStyle windowStyle, osdLabelStyle, labelStyle;

		void InitStyles ()
		{
			//windowStyle = new GUIStyle(HighLogic.Skin.Window);
			DebugMsg ("InitStyles()");
	
			windowStyle = new GUIStyle ();
			windowStyle.fixedWidth = 250f;		
	
			osdLabelStyle = new GUIStyle ();
			osdLabelStyle.stretchWidth = true;
			osdLabelStyle.alignment = TextAnchor.MiddleCenter;
			osdLabelStyle.fontSize = 24;
			osdLabelStyle.fontStyle = FontStyle.Bold;
			osdLabelStyle.normal.textColor = Color.yellow;
	
			labelStyle = new GUIStyle ("Label");
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.fontSize = 22;
			//labelStyle.fontStyle = FontStyle.Bold;
			labelStyle.normal.textColor = XKCDColors.DarkYellow;
		}

		float messageCutoff = 0;
		string messageText = "";

		/// <summary>
		/// Set a on screen display message
		/// </summary>
		/// <param name="message">Message string</param>
		/// <param name="delay">Amount of time to display the message</param>
		void OSDMessage (string message, float delay)
		{
			messageCutoff = Time.time + delay;
			messageText = message;
			DebugMsg (String.Format ("OSD messageCutoff = {0}, messageText = {1}", messageCutoff.ToString (), messageText));
		}

		/// <summary>
		/// check for On Screen Display message
		/// </summary>
		void DisplayOSD ()
		{
			if (Time.time < messageCutoff) {
				GUILayout.BeginArea (new Rect (0, (Screen.height / 4), Screen.width, 200), osdLabelStyle);
				GUILayout.Label (messageText, osdLabelStyle);
				GUILayout.EndArea ();			
			}
		}
	
		/*
		void OnWindow(int windowId)
		{
			GUILayout.BeginHorizontal(GUILayout.Width(250f));
			GUILayout.Label("this is the label");
			GUILayout.EndHorizontal();
			GUI.DragWindow();
		}
		*/
	
		string symmetryLabelValue = string.Empty;

		public void OnGUI ()
		{	
			DisplayOSD ();

			//Only show angle/symmetry sprites on parts tab
			if (editor.editorScreen == EditorScreen.Parts) {

				//symmetryLabelRect = new Rect (150, Screen.height - 65, 57, 57);
				//angleSnapLabelRect = new Rect (206, Screen.height - 63, 46, 46);

//				angleSnapLabelRect = new Rect () {
//					xMin = 206,
//					xMax = 206 + 46,
//					yMin = Screen.height - 63,
//					yMax = Screen.height - 63 + 46
//				};

				//need to shift labels in advanced mode to the right
				if (EditorLogic.Mode == EditorLogic.EditorModes.ADVANCED) {
					symmetryLabelRect.xMin = 184;
					symmetryLabelRect.xMax = 184 + 57;
					angleSnapLabelRect.xMin = 241;
					angleSnapLabelRect.xMax = 241 + 46;
				} else {
					//EditorLogic.EditorModes.SIMPLE
					symmetryLabelRect.xMin = 150;
					symmetryLabelRect.xMax = 150 + 57;
					angleSnapLabelRect.xMin = 207;
					angleSnapLabelRect.xMax = 207 + 46;
				}

				//Radial mode number+R, mirror mode is M/MM
				if (editor.symmetryMethod == SymmetryMethod.Radial) {
					symmetryLabelValue = (editor.symmetryMode + 1) + "R";
				} else if (editor.symmetryMethod == SymmetryMethod.Mirror) {
					symmetryLabelValue = (editor.symmetryMode == 0) ? "M" : "MM";
				}

				// Show Symmetry level
				GUI.Label (symmetryLabelRect, symmetryLabelValue, labelStyle);
	
				// Show angle snap amount
				editor.angleSnapSprite.Hide (GameSettings.VAB_USE_ANGLE_SNAP);
				editor.symmetrySprite.Hide (true);

				//show stock circle sprite when angle snap is off (0 degrees)
				if (GameSettings.VAB_USE_ANGLE_SNAP) {
					GUI.Label (angleSnapLabelRect, editor.srfAttachAngleSnap + degreesSymbol, labelStyle);
				}
			}
		}

		#endregion
	}
}
