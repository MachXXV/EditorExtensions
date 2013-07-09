using System;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.EditorAny, false)]
public class EditorExtensions : MonoBehaviour
{
	#region member vars

    bool debug = true;

    const string launchSiteName_LaunchPad = "LaunchPad";
    const string launchSiteName_Runway = "Runway";
	const string degreesSymbol = "\u00B0";
	const string VABGameObjectName = "interior_vehicleassembly";
	const string SPHGameObjectName = "xport_sph3";

	int _symmetryMode = 0;
    int maxSymmetryMode = 50;
    static float[] angle = { 0, 1, 5, 15, 30, 45, 90 };

    bool ignoreHotKeys = false;

    bool altKeyPressed;
    bool shiftKeyPressed;

    bool inVAB;

    EditorLogic editor;

	Rect symLabelRect;
	Rect angleSnapLabelRect;

	#endregion

	#region logging

	void DebugMessage(string message)
	{
		if (debug)
			print ("EditorExtensions: " + message);
	}

	void DebugMessage(string message, bool boolValue)
	{
		if (debug)
			print (String.Format("EditorExtensions: {0} {1}", message, boolValue.ToString()));
	}

	#endregion

	public EditorExtensions()
	{
		DebugMessage("Class constructor");
	}

	//Unity initialization call
    public void Awake()
    {
        //DontDestroyOnLoad(this);
		DebugMessage ("initializing");

		editor = EditorLogic.fetch;

		AlterEditorSpace(editor);

		editor.symmetrySprite.Hide(true);
		editor.mirrorSprite.Hide(true);

		//Rects for symmetry/angle snap labels
		symLabelRect = new Rect (70, Screen.height - 104, 50, 50);
		angleSnapLabelRect = new Rect (137, Screen.height - 104, 50, 50);

		InitStyles ();

		//Disable shortcut keys when ship name textarea has focus
		editor.shipNameField.commitOnLostFocus = true;
		editor.shipNameField.AddCommitDelegate((IKeyFocusable _) => { ignoreHotKeys = false; });
		editor.shipNameField.AddFocusDelegate((UITextField _) => { ignoreHotKeys = true; });
    }

	void AlterEditorSpace(EditorLogic editor)
	{
		editor.maxHeight = 2000;

		// Modify cameras/available interior space
		if (HighLogic.LoadedScene == GameScenes.EDITOR)
		{
			DebugMessage ("Updating VAB dimensions and camera");

			VABCamera VABcam = Camera.mainCamera.GetComponent<VABCamera>();
			VABcam.maxHeight = 2000;
			VABcam.maxDistance = 2000;

			GameObject interior = GameObject.Find(VABGameObjectName);
			interior.transform.localScale = new Vector3(2.2f, 1.8f, 1.8f);
			interior.transform.position = new Vector3(59f, 51.5f, 12);
		}
		else if (HighLogic.LoadedScene == GameScenes.SPH)
		{
			DebugMessage ("Updating SPH dimensions and camera");

			SPHCamera SPHcam = Camera.mainCamera.GetComponent<SPHCamera>();
			SPHcam.maxHeight = 2000;
			SPHcam.maxDistance = 2000;
			SPHcam.maxDisplaceX = 2000;
			SPHcam.maxDisplaceZ = 2000;

			GameObject interior = GameObject.Find(SPHGameObjectName);
			interior.transform.localScale = new Vector3(12, 6, 12);
			interior.transform.position = new Vector3(-24.9f, -0.3f, 22.8f);
		}
	}

    public void Update()
    {
		//Idea: change root part. 

        //need to verify the EditorLogic state - do we need to fetch it every time?
        editor = EditorLogic.fetch;
        if (editor == null)
            return;

        if(ignoreHotKeys || editor.editorScreen != EditorLogic.EditorScreen.Parts)
            return;

		//may need to go away from this and do explicit editor.editorType calls 
		inVAB = (editor.editorType == EditorLogic.EditorMode.VAB);

		//check for the various alt/mod etc keypresses
		altKeyPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
		//check for shift key
		shiftKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

		//Space - when no part is selected, reset camera
		if (Input.GetKeyDown(KeyCode.Space) && !editor.PartSelected)
		{
			if (HighLogic.LoadedScene == GameScenes.EDITOR)
			{
				VABCamera VABcam = Camera.mainCamera.GetComponent<VABCamera>();
				VABcam.camPitch = 0;
				VABcam.camHdg = 0;
				//VABcam.ResetCamera();
			}
			else if (HighLogic.LoadedScene == GameScenes.SPH)
			{
				SPHCamera SPHcam = Camera.mainCamera.GetComponent<SPHCamera>();
				SPHcam.camPitch = 0;
				SPHcam.camHdg = 0;
				//SPHcam.ResetCamera();
			}
		}

		// Alt+M - Toggle VAB/SPH editor mode (while staying in the same hangar)
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (editor.editorType == EditorLogic.EditorMode.SPH){
				editor.editorType = EditorLogic.EditorMode.VAB;
				editor.launchSiteName = launchSiteName_LaunchPad;
				OSDMessage ("VAB/Launchpad Mode");
			}
			else{
				editor.editorType = EditorLogic.EditorMode.SPH;
				editor.launchSiteName = launchSiteName_Runway;
				editor.symmetryMode = 1;
				OSDMessage ("SPH/Runway Mode");
			}
			return;
		}

        // V - Vertical alignment toggle
        if (Input.GetKeyDown(KeyCode.V))
        {
			//DebugMessage ("Toggling vertical snap");
            GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ^= true;

			//if normal radial angle snap is currently off, vertical snap will have no effect unless it is re-enabled
			//automatically set aangle snap to minimum - some people thought vert snap was broken in this situation, the game doesn't appear to allow it
			if (GameSettings.VAB_USE_ANGLE_SNAP == false && GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL == true)
			{
				DebugMessage ("Enabling angle snap to allow vertical snap to work");
				//angle snap needs be > 0, otherwise log is spammed with DivideByZero errors
				if(editor.srfAttachAngleSnap == 0)
					editor.srfAttachAngleSnap = 1;
				GameSettings.VAB_USE_ANGLE_SNAP = true;
			}
			OSDMessage ("Vertical snap " + (GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ? "enabled" : "disabled"));
			DebugMessage ("Vertical snap " + (GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ? "enabled" : "disabled"));
            return;
        }

		// ALT+R : Toggle radial/surface attachment globally
		// When part is selected, also toggles surface attachment for that part, even if that part's cfg has it disabled.
		if (altKeyPressed && Input.GetKeyDown(KeyCode.R))
		{
			if (EditorLogic.SelectedPart)
			{
				//Toggle surface attachment for selected part, set global surface attachment toggle to match
				EditorLogic.SelectedPart.attachRules.srfAttach ^= true;
				editor.allowSrfAttachment = EditorLogic.SelectedPart.attachRules.srfAttach;
				DebugMessage ("Toggling srfAttach for " + EditorLogic.SelectedPart.name);
				OSDMessage(String.Format("Surface attachment {0}, and for {1}"
				                         ,EditorLogic.SelectedPart.attachRules.srfAttach ? "enabled" : "disabled"
				                         ,EditorLogic.SelectedPart.name
				                         ));
			}
			else
			{
				//just toggle global surface attachment, parts whose config do not allow it are unaffected
				editor.allowSrfAttachment ^= true;
				OSDMessage("Allow surface attachment is globally " + (editor.allowSrfAttachment ? "enabled" : "disabled"));
				DebugMessage("Allow surface attachment is globally " + (editor.allowSrfAttachment ? "enabled" : "disabled"));
			}
		}

		/****consolidating this to just Alt+R to simplify things
		// ALT+R : Toggle radial attachment globally
		if (altKeyPressed && !shiftKeyPressed && Input.GetKeyDown(KeyCode.R))
		{
			editor.allowSrfAttachment ^= true;
			OSDMessage("Allow surface attachment is globally " + (editor.allowSrfAttachment ? "enabled" : "disabled"));
			DebugMessage("Allow surface attachment is globally " + (editor.allowSrfAttachment ? "enabled" : "disabled"));

			DebugMessage (
				String.Format("after toggle = Part: {0} allowSrfAttach: {1} srfAttach: {2} allowSrfAttachment: {3} allowNodeAttachment: {4}"
			              , EditorLogic.SelectedPart.name
			              ,EditorLogic.SelectedPart.attachRules.allowSrfAttach.ToString()
			              ,EditorLogic.SelectedPart.attachRules.srfAttach.ToString()
			              ,editor.allowSrfAttachment.ToString()
			              ,editor.allowNodeAttachment.ToString()
			              ));

			return;
		}


		// Shift+ALT+R : Toggle radial attachment for selected part, also allows radial attachment for parts that do not usually allow it.
        if (altKeyPressed && shiftKeyPressed && EditorLogic.SelectedPart && Input.GetKeyDown(KeyCode.R))
        {
			//some excessive loggeration to figure out the surface attachment stuff
			DebugMessage (
				String.Format("before toggle = Part: {0} allowSrfAttach: {1} srfAttach: {2} allowSrfAttachment: {3} allowNodeAttachment: {4}"
			              ,EditorLogic.SelectedPart.name
			              ,EditorLogic.SelectedPart.attachRules.allowSrfAttach.ToString()
			              ,EditorLogic.SelectedPart.attachRules.srfAttach.ToString()
			              ,editor.allowSrfAttachment.ToString()
			              ,editor.allowNodeAttachment.ToString()
				));

			DebugMessage ("Toggling srfAttach for " + EditorLogic.SelectedPart.name);
			//EditorLogic.SelectedPart.attachRules.allowSrfAttach no longer seems to have an effect
            //EditorLogic.SelectedPart.attachRules.allowSrfAttach ^= true;
			EditorLogic.SelectedPart.attachRules.srfAttach ^= true;

			OSDMessage(String.Format("Surface attachment {0} for {1}"
			                         ,EditorLogic.SelectedPart.attachRules.srfAttach ? "enabled" : "disabled"
			                         ,EditorLogic.SelectedPart.name
			                         ));

			//set the global radial attachment setting to the same
			//otherwise this can get confusing when the two settings don't match, since the global will override the part-specific setting, 
			//which can make this toggle appear to be nto working correctly
			editor.allowSrfAttachment = EditorLogic.SelectedPart.attachRules.srfAttach;


				DebugMessage (
				String.Format("after toggle = Part: {0} allowSrfAttach: {1} srfAttach: {2} allowSrfAttachment: {3} allowNodeAttachment: {4}"
			              , EditorLogic.SelectedPart.name
			              ,EditorLogic.SelectedPart.attachRules.allowSrfAttach.ToString()
			              ,EditorLogic.SelectedPart.attachRules.srfAttach.ToString()
			              ,editor.allowSrfAttachment.ToString()
			              ,editor.allowNodeAttachment.ToString()
			              ));

            return;
        }
        */

		// ALT+Z : Toggle part clipping (From cheat options)
		if(altKeyPressed && Input.GetKeyDown(KeyCode.Z))
		{
			CheatOptions.AllowPartClipping ^= true;
			DebugMessage("AllowPartClipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"));
			OSDMessage ("Part clipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"));
			return;
		}

        // C, Shift+C : Increment/Decrement Angle snap
		if (Input.GetKeyDown (KeyCode.C)) {

			if (!altKeyPressed)
			{
				//GameSettings.VAB_USE_ANGLE_SNAP = false;
				DebugMessage ("Starting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString ());

				int currentAngleIndex = Array.IndexOf (angle, editor.srfAttachAngleSnap);

				DebugMessage ("currentAngleIndex: " + currentAngleIndex.ToString ());

				float newAngle;
				if (shiftKeyPressed) {
					newAngle = angle [currentAngleIndex == 0 ? angle.Length - 1 : currentAngleIndex - 1];
				} else {
					DebugMessage ("new AngleIndex: " + (currentAngleIndex == angle.Length - 1 ? 0 : currentAngleIndex + 1).ToString ());
					newAngle = angle [currentAngleIndex == angle.Length - 1 ? 0 : currentAngleIndex + 1];
				}

				DebugMessage ("Setting srfAttachAngleSnap to " + newAngle.ToString ());
				editor.srfAttachAngleSnap = newAngle;
			}
			else
			{
				DebugMessage ("Resetting srfAttachAngleSnap to 0");
				editor.srfAttachAngleSnap = 0;
			}

			if (editor.srfAttachAngleSnap == 0)
			{
				GameSettings.VAB_USE_ANGLE_SNAP = false;
				//Vertical snap doesn't work when angle snap is disabled.
				//Resetting it here so that the toggle logic for vert snap maintains state
				GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL = false;
				editor.angleSnapSprite.PlayAnim (0);
			}
			else
			{
				GameSettings.VAB_USE_ANGLE_SNAP = true;
			}

			DebugMessage ("Exiting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString());
			return;

        }

		// X, Shift+X : Increment/decrement symmetry mode
		if (inVAB && Input.GetKeyDown(KeyCode.X))
		{
			if (altKeyPressed
			    || (_symmetryMode > maxSymmetryMode - 2 && !shiftKeyPressed)
			    || (_symmetryMode < 2 && shiftKeyPressed))
			{
				_symmetryMode = 0;
			}
			else
			{
				_symmetryMode = _symmetryMode + (shiftKeyPressed ? -1 : 1);
			}

			DebugMessage ("Setting symmetry to " + _symmetryMode.ToString());
			editor.symmetryMode = _symmetryMode;
		}
    }

	#region GUI

	GUIStyle windowStyle, osdLabelStyle, labelStyle;
	void InitStyles()
	{
		//windowStyle = new GUIStyle(HighLogic.Skin.Window);

		windowStyle = new GUIStyle();
		windowStyle.fixedWidth = 250f;		

		osdLabelStyle = new GUIStyle();
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
	void OSDMessage(string message, float delay = 1)
	{
		messageCutoff = Time.time + delay;
		messageText = message;
		DebugMessage (String.Format ("messageCutoff = {0}, messageText = {1}", messageCutoff.ToString (), messageText));
	}

	void DisplayOSD()
	{
		//DebugMessage ("DisplayOSD()");
		if(Time.time < messageCutoff)
		{
			//DebugMessage("Displaying OSD");
			GUILayout.BeginArea(new Rect (0, (Screen.height/4), Screen.width, 200), osdLabelStyle);
			//GUI.Label(new Rect(200,200,50,50), messageText);
			GUILayout.Label(messageText, osdLabelStyle);
			GUILayout.EndArea();			
		}
		//DebugMessage("end displaying OSD");
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

    public void OnGUI()
    {
		//need to verify the EditorLogic state - do we need to fetch it every time?
		editor = EditorLogic.fetch;
		if (editor == null)
			return;

		DisplayOSD();

        // Show Symmetry level
        string sym = (editor.symmetryMode + 1) + "x";
        if (editor.editorType == EditorLogic.EditorMode.SPH)
            sym = (editor.symmetryMode == 0) ? "M" : "MM";

        GUI.Label(symLabelRect, sym, labelStyle);

        // Show angle snap amount
        editor.angleSnapSprite.Hide(GameSettings.VAB_USE_ANGLE_SNAP);
		editor.symmetrySprite.Hide(true);

		//disable sprite to avoid out of bounds animation calls
		//editor.symmetrySprite.enabled = false;

        if (GameSettings.VAB_USE_ANGLE_SNAP)
        {
			GUI.Label(angleSnapLabelRect, editor.srfAttachAngleSnap + degreesSymbol, labelStyle);
        }
    }

	#endregion
}
