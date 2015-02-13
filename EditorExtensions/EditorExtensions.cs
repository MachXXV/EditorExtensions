using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using KSP.IO;
using UnityEngine;

namespace EditorExtensions
{

	[KSPAddon (KSPAddon.Startup.EditorAny, false)]
	public class EditorExtensions : MonoBehaviour
	{
		public static EditorExtensions Instance { get; private set; }

		public bool Visible { get; set; }

		#region member vars

		const string ConfigFileName = "config.xml";
		const string degreesSymbol = "\u00B0";

		EditorLogic editor;
		Version pluginVersion;
		ConfigData cfg;
		string _pluginDirectory;
		string _configFilePath;
		int _symmetryMode = 0;

		SettingsWindow _settingsWindow = null;
		PartInfoWindow _partInfoWindow = null;
		StrutWindow _strutWindow = null;

		bool enableHotkeys = true;
		//bool _gizmoActive = false;

		#endregion

		public EditorExtensions (){}
		
		//Unity initialization call
		public void Awake ()
		{
			editor = EditorLogic.fetch;
			Instance = this;
			InitConfig ();
			InitializeGUI ();

			//part.parent.Modules.Contains("WingManipulator")
			//bool FARactive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase));
		}

		void InitConfig ()
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
						cfg = ConfigManager.CreateDefaultConfig (_configFilePath, pluginVersion.ToString ());
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
							cfg = ConfigManager.CreateDefaultConfig (_configFilePath, pluginVersion.ToString ());
						} else {
							Log.Debug ("Config file is current");
						}
					}

				} else {
					cfg = ConfigManager.CreateDefaultConfig (_configFilePath, pluginVersion.ToString ());
					Log.Info ("No existing config found, created new default config");
				}

				Log.Debug ("Initializing version " + pluginVersion.ToString ());
			} catch (Exception ex) {
				Log.Debug ("FATAL ERROR - Unable to initialize: " + ex.Message);
				//_abort = true;
				return;
			}
		}

		//Unity OnDestroy
		void OnDestroy ()
		{
			Log.Debug ("OnDestroy()");
			//if (_settingsWindow != null)
			//	_settingsWindow.enabled = false;
			//if (_partInfoWindow != null)
			//	_partInfoWindow.enabled = false;
		}

		bool GizmoActive ()
		{
			try {
				if (HighLogic.FindObjectsOfType<EditorGizmos.GizmoOffset> ().Length > 0 || HighLogic.FindObjectsOfType<EditorGizmos.GizmoRotate> ().Length > 0) {
					return true;
				} else {
					return false;
				}
#if DEBUG
			} catch (Exception ex) {
				Log.Error ("Error getting active Gizmos: " + ex.Message);
#else
			} catch (Exception) {
#endif
				return false;
			}
		}

		void HighlightPart(Part p){
			// old highlighter. Not necessary, but looks nice in combination
			p.SetHighlightDefault();
			p.SetHighlightType(Part.HighlightType.AlwaysOn);
			p.SetHighlight(true, false);
			p.SetHighlightColor(Color.red);

			// New highlighter
			HighlightingSystem.Highlighter hl; // From Assembly-CSharp-firstpass.dll
			hl = p.FindModelTransform("model").gameObject.AddComponent<HighlightingSystem.Highlighter>();
			hl.ConstantOn(XKCDColors.Rust);
			hl.SeeThroughOn();
		}

		void AlignCompoundPart(CompoundPart part)
		{
			bool hasHit = false;
			Vector3 dir = part.direction;
			Vector3 newDirection = Vector3.zero;

			if (dir.y < -0.125f) {
				//more than -22.5deg vert
				newDirection = new Vector3 (-0.5f, -0.5f, 0.0f);
				Log.Debug ("Aligning compoundPart 45deg down");
			} else if (dir.y > 0.125f) {
				//more than +22.5deg vert
				newDirection = new Vector3 (-0.5f, 0.5f, 0.0f);
				Log.Debug ("Aligning compoundPart 45deg up");
			} else {
				//straight ahead
				newDirection = new Vector3 (-1.0f, 0.0f, 0.0f);
				Log.Debug ("Aligning compoundPart level");
			}

			hasHit = part.raycastTarget (newDirection);

			if (!hasHit){
				//try just leveling the strut
				Log.Debug ("Original align failed, Aligning compoundPart level");
				part.direction = new Vector3 (dir.x, 0.0f, dir.z);
				hasHit = part.raycastTarget (newDirection);
			}

			List<Part> symParts = part.symmetryCounterparts;
			//move any symmetry siblings/counterparts
			foreach (CompoundPart symPart in symParts) {
				symPart.raycastTarget (newDirection);
			}

			Log.Debug ("CompoundPart target hit: " + hasHit.ToString ());
			editor.SetBackup ();
		}

		//Unity update
		void Update ()
		{
			if (editor.shipNameField.Focused || editor.shipDescriptionField.Focused)
				return;

			//ignore hotkeys while settings window is open
			//if (_settingsWindow != null && _settingsWindow.enabled)
			//	return;

			//EditorFacility facility = EditorLogic.fetch.ship.shipFacility;

			//hotkeyed editor functions
			if (enableHotkeys) {

				//check for the configured modifier key
				bool modKeyDown = GameSettings.MODIFIER_KEY.GetKey();
				//check for configured editor fine key
				bool fineKeyDown = GameSettings.Editor_fineTweak.GetKey();

				// P - strut/fuel line alignment
				if (Input.GetKeyDown (cfg.KeyMap.CompoundPartAlign)) {
					Part p = Utility.GetPartUnderCursor ();
					if (p != null && p.GetType () == typeof(CompoundPart)) {
						AlignCompoundPart ((CompoundPart)p);
					}
				}
			
				// V - Vertically align part under cursor with the part it is attached to
				if (Input.GetKeyDown (cfg.KeyMap.VerticalSnap)) {
					try {
						Part sp = Utility.GetPartUnderCursor ();

						if (sp != null && sp.srfAttachNode != null && sp.srfAttachNode.attachedPart != null && !GizmoActive()) {

							List<Part> symParts = sp.symmetryCounterparts;

							Log.Debug ("symmetryCounterparts to move: " + symParts.Count.ToString ());

							//move hovered part
							sp.transform.localPosition = new Vector3 (sp.transform.localPosition.x, 0f, sp.transform.localPosition.z);
							sp.attPos0.y = 0f;

							//move any symmetry siblings/counterparts
							foreach (Part symPart in symParts) {
								symPart.transform.localPosition = new Vector3 (symPart.transform.localPosition.x, 0f, symPart.transform.localPosition.z);
								symPart.attPos0.y = 0f;
							}

							//need to verify this is the right way, it does seem to work
							//Add edit to undo history
							editor.SetBackup ();
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
						Part sp = Utility.GetPartUnderCursor ();

						if (sp != null && sp.srfAttachNode != null && sp.srfAttachNode.attachedPart != null && !GizmoActive()) {

							//Part ap = sp.srfAttachNode.attachedPart;
							List<Part> symParts = sp.symmetryCounterparts;

							Log.Debug ("symmetryCounterparts to move: " + symParts.Count.ToString ());

							//move selected part
							sp.transform.localPosition = new Vector3 (sp.transform.localPosition.x, sp.transform.localPosition.y, 0f);
							sp.attPos0.z = 0f;

							//move any symmetry siblings/counterparts
							foreach (Part symPart in symParts) {
								symPart.transform.localPosition = new Vector3 (symPart.transform.localPosition.x, symPart.transform.localPosition.y, 0f);
								symPart.attPos0.z = 0f;
							}

							//Add edit to undo history
							editor.SetBackup ();
						}
					} catch (Exception ex) {
						Log.Error ("Error trying to Horizontally align: " + ex.Message);
					}
					return;
				}     

				//Space - when no part is selected, reset camera
				if (Input.GetKeyDown (cfg.KeyMap.ResetCamera) && !EditorLogic.SelectedPart) {

					if (!GizmoActive()) {
					VABCamera VABcam = Camera.main.GetComponent<VABCamera> ();
					VABcam.camPitch = 0;
					VABcam.camHdg = 0;
					//VABcam.ResetCamera ();

					SPHCamera SPHcam = Camera.main.GetComponent<SPHCamera> ();
					SPHcam.camPitch = 0;
					SPHcam.camHdg = 0;
					//SPHcam.ResetCamera();
					}
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
				if (modKeyDown && Input.GetKeyDown (cfg.KeyMap.PartClipping)) {
					CheatOptions.AllowPartClipping ^= true;
					Log.Debug ("AllowPartClipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"));
					OSDMessage ("Part clipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"));
					return;
				}

				//using gamesettings keybinding Input.GetKeyDown (cfg.KeyMap.AngleSnap)
				// C, Shift+C : Increment/Decrement Angle snap
				if (GameSettings.Editor_toggleAngleSnap.GetKeyDown()) {
	
					if (!modKeyDown) {
						Log.Debug ("Starting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString ());
	
						int currentAngleIndex = cfg.AngleSnapValues.IndexOf (editor.srfAttachAngleSnap);
	
						Log.Debug ("currentAngleIndex: " + currentAngleIndex.ToString ());
	
						//rotate through the angle snap values
						float newAngle;
						if (fineKeyDown) {
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
	
				//using gamesettings keybinding Input.GetKeyDown (cfg.KeyMap.Symmetry)
				// X, Shift+X : Increment/decrement symmetry mode
				if (GameSettings.Editor_toggleSymMode.GetKeyDown()) {

					//only inc/dec symmetry in radial mode, mirror is just 1&2
					if (editor.symmetryMethod == SymmetryMethod.Radial) {
						if (modKeyDown || (_symmetryMode < 2 && fineKeyDown)) {
							//Alt+X or Symmetry is at 1(index 2) or lower
							_symmetryMode = 0;
						} else if (_symmetryMode > cfg.MaxSymmetry - 2 && !fineKeyDown) {
							//Stop adding at max symmetry
							_symmetryMode = cfg.MaxSymmetry - 1;
						} else {
							//inc/dec symmetry
							_symmetryMode = _symmetryMode + (fineKeyDown ? -1 : 1);
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

		//private Rect _settingsWindowRect;
		GUIStyle osdLabelStyle, symmetryLabelStyle;

		void InitializeGUI ()
		{
			_settingsWindow = this.gameObject.AddComponent<SettingsWindow> ();
			_settingsWindow.WindowDisabled += new SettingsWindow.WindowDisabledEventHandler (SettingsWindowClosed);

			_partInfoWindow = this.gameObject.AddComponent<PartInfoWindow> ();

			_strutWindow = this.gameObject.AddComponent<StrutWindow> ();

			osdLabelStyle = new GUIStyle () {
				stretchWidth = true,
				stretchHeight = true,
				alignment = TextAnchor.MiddleCenter,
				fontSize = 22,
				fontStyle = FontStyle.Bold,
				name = "OSDLabel"
			};
			osdLabelStyle.normal.textColor = Color.yellow;

			symmetryLabelStyle = new GUIStyle () {
				stretchWidth = true,
				stretchHeight = true,
				alignment = TextAnchor.MiddleCenter,
				fontSize = 18,
				fontStyle = FontStyle.Bold,
				name = "SymmetryLabel"
			};
			symmetryLabelStyle.normal.textColor = Color.yellow;

			//skin.customStyles = new GUIStyle[]{ osdLabel, symmetryLabel };
		}

		//show the addon's GUI
		public void Show ()
		{
			this.Visible = true;
			Log.Debug ("Show()");
			//if (!_settingsWindow.enabled) {
			//	_settingsWindow.Show (cfg, _configFilePath, pluginVersion);
			//}
		}

		//hide the addon's GUI
		public void Hide ()
		{
			this.Visible = false;
			Log.Debug ("Hide()");
			//if (_settingsWindow.enabled) {
			//	_settingsWindow.enabled = false;
			//}
		}

		public void SettingsWindowClosed(){
			Log.Debug ("Settings window closed, reloading config");
			cfg = ConfigManager.LoadConfig (_configFilePath);
			Hide ();
		}

		bool _showMenu = false;
		Rect _menuRect = new Rect ();
		float _menuWidth = 100.0f;
		float _menuHeight = 100.0f;

		public void ShowMenu ()
		{
			Vector3 position = Input.mousePosition;
				
			_menuRect = new Rect () {
				xMin = position.x - _menuWidth / 2,
				xMax = position.x + _menuWidth / 2,
				yMin = Screen.height - 37 - _menuHeight,
				yMax = Screen.height - 37
			};
			_showMenu = true;
		}

		public void HideMenu ()
		{
			_showMenu = false;
		}

		//Unity GUI loop
		void OnGUI ()
		{
			//show on-screen messages
			//DisplayOSD ();

			//show and update the angle snap and symmetry mode labels
			ShowSnapLabels ();

			if (Event.current.type == EventType.Layout) {
				if (_showMenu || _menuRect.Contains (Event.current.mousePosition))
					_menuRect = GUILayout.Window (this.GetInstanceID (), _menuRect, MenuContent, "EEX Menu");
				else
					_menuRect = new Rect ();
			}
		}

		void MenuContent (int WindowID)
		{
			GUILayout.BeginVertical ();
			if (GUILayout.Button ("Settings")) {
				_settingsWindow.Show (cfg, _configFilePath, pluginVersion);
				this.Visible = true;
			}


			_strutWindow.enabled = GUILayout.Toggle (_strutWindow.enabled, "Strut Tool", "Button");

			//if (GUILayout.Button ("Strut tool")) {
			//	_strutWindow.Show ();
			//	this.Visible = true;
			//}

			if (cfg.ShowDebugInfo) {
				if (GUILayout.Button ("Position Debug")) {
					_partInfoWindow.Show ();
				}
			}
			GUILayout.EndVertical ();
		}

		void OSDMessage (string message)
		{
			//ScreenMessage msg = new ScreenMessage (message, cfg.OnScreenMessageTime, false, ScreenMessageStyle.LOWER_CENTER);
			ScreenMessages.PostScreenMessage (message, cfg.OnScreenMessageTime, ScreenMessageStyle.LOWER_CENTER);
		}

		#region Snap labels


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
			//editor.symmetryButton.transform.position?

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
				//editor.symmetrySprite.Hide (true);
				//editor.mirrorSprite.Hide (true);
				//KSP code appears to use this method
				editor.symmetrySprite.gameObject.SetActive (false);
				editor.mirrorSprite.gameObject.SetActive (false);

				// Show Symmetry label
				GUI.Label (symmetryLabelRect, symmetryLabelValue, symmetryLabelStyle);

				//if angle snap is on hide stock sprite
				if (GameSettings.VAB_USE_ANGLE_SNAP) {
					//editor.angleSnapSprite.Hide (true);
					editor.angleSnapSprite.gameObject.SetActive (false);
					GUI.Label (angleSnapLabelRect, editor.srfAttachAngleSnap + degreesSymbol, symmetryLabelStyle);

				} else {
					//angle snap is off, show stock sprite
					editor.angleSnapSprite.PlayAnim (0);
					//editor.angleSnapSprite.Hide (false);
					editor.angleSnapSprite.gameObject.SetActive (true);
				}
			}
		}

		#endregion

		#endregion
	}
}
