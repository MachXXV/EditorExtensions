using System;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.EditorAny, false)]
public class EditorExtensions : MonoBehaviour
{
    bool debug = true;

    const string launchSiteName_LaunchPad = "LaunchPad";
    const string launchSiteName_Runway = "Runway";
	const string degreesSymbol = "\u00B0";

    //static int[] symmetryModes = { -1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 25, 30 };

	int _symmetryMode = 0;
    int maxSymmetryMode = 50;
    static float[] angle = { 0, 5, 15, 30, 45, 90 };

    bool ignoreHotKeys = false;

    bool altKeyPressed;
    bool shiftKeyPressed;

    bool inVAB;

    EditorLogic editor;

	Rect symLabelRect;
	Rect angleSnapLabelRect;

	void DebugMessage(string message)
	{
		if (debug)
			print ("EditorExtensions: " + message);
	}

    public void Awake()
    {
        //DontDestroyOnLoad(this);
		DebugMessage ("initializing");

		editor = EditorLogic.fetch;

		editor.symmetrySprite.Hide(true);
		editor.mirrorSprite.Hide(true);

		//Rects for symmetry/angle snap labels
		symLabelRect = new Rect (70, Screen.height - 104, 50, 50);
		angleSnapLabelRect = new Rect (137, Screen.height - 104, 50, 50);

		//init styles
		if (labelStyle == null)
			InitLabelStyle ();

		//Disable shortcut keys when ship name textarea has focus
		editor.shipNameField.commitOnLostFocus = true;
		editor.shipNameField.AddCommitDelegate((IKeyFocusable _) => { ignoreHotKeys = false; });
		editor.shipNameField.AddFocusDelegate((UITextField _) => { ignoreHotKeys = true; });
    }

    public void Update()
    {
        //need to verify the EditorLogic state - do we need to fetch it every time?
        editor = EditorLogic.fetch;
        if (editor == null)
            return;

        if(ignoreHotKeys || editor.editorScreen != EditorLogic.EditorScreen.Parts)
            return;

        // V - Vertical alignment toggle
        if (Input.GetKeyDown(KeyCode.V))
        {
			DebugMessage ("Toggling vertical snap");
            GameSettings.VAB_ANGLE_SNAP_INCLUDE_VERTICAL ^= true;
            return;
        }

        altKeyPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);        

        // ALT+R : Toggle radial attachment
        if (altKeyPressed && EditorLogic.SelectedPart && Input.GetKeyDown(KeyCode.R))
        {
			DebugMessage ("Toggling allowSrfAttach for " + EditorLogic.SelectedPart.name);
            EditorLogic.SelectedPart.attachRules.allowSrfAttach ^= true;
            return;
        }

		// ALT+Z : Toggle part clipping (From cheat options)
		if (altKeyPressed && Input.GetKeyDown(KeyCode.Z))
		{
			CheatOptions.AllowPartClipping ^= true;
			DebugMessage("AllowPartClipping " + (CheatOptions.AllowPartClipping ? "enabled" : "disabled"));
			return;
		}

        shiftKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        inVAB = (editor.editorType == EditorLogic.EditorMode.VAB);

        // C, Shift+C : Increment/Decrement Angle snap
		if (Input.GetKeyDown (KeyCode.C)) {

			//GameSettings.VAB_USE_ANGLE_SNAP = false;
			DebugMessage ("Starting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString ());

			int currentAngleIndex = Array.IndexOf(angle, editor.srfAttachAngleSnap);

			DebugMessage ("currentAngleIndex: " + currentAngleIndex.ToString());

			float newAngle;
			if (shiftKeyPressed)
			{
				newAngle = angle[currentAngleIndex == 0 ? angle.Length - 1 : currentAngleIndex - 1];
			}
            else
			{
				DebugMessage ("new AngleIndex: " + (currentAngleIndex == angle.Length - 1 ? 0 : currentAngleIndex + 1).ToString());
				newAngle = angle[currentAngleIndex == angle.Length - 1 ? 0 : currentAngleIndex + 1];
			}

			DebugMessage ("Setting srfAttachAngleSnap to " + newAngle.ToString());
			editor.srfAttachAngleSnap = newAngle;
			DebugMessage ("Current srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString());

			if (editor.srfAttachAngleSnap == 0) {
				GameSettings.VAB_USE_ANGLE_SNAP = false;
				editor.angleSnapSprite.PlayAnim (0);
			}
			else
			{
				GameSettings.VAB_USE_ANGLE_SNAP = true;
			}

			DebugMessage ("Exiting srfAttachAngleSnap = " + editor.srfAttachAngleSnap.ToString());

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


		/*
        // "X", "Shift X" Symmetry level/"Mode"
        if (inVAB && Input.GetKeyDown(KeyCode.X))
        {
			//disable sprite to avoid out of bounds animation calls
			editor.symmetrySprite.enabled = false;

			DebugMessage ("A Symmetry mode starting at " + editor.symmetryMode.ToString());

            if (altKeyPressed || (editor.symmetryMode > maxSymmetryMode && !shiftKeyPressed))
			{
				DebugMessage ("B Symmetry " + editor.symmetryMode.ToString());
                editor.symmetryMode = -1;
				DebugMessage ("C Symmetry " + editor.symmetryMode.ToString());
			}
			else if (editor.symmetryMode == 7 && !shiftKeyPressed)
			{
				DebugMessage ("D Symmetry " + editor.symmetryMode.ToString());
				editor.symmetryMode = 8;
				DebugMessage ("E Symmetry " + editor.symmetryMode.ToString());
			}

			if (editor.symmetryMode > 7)
			{
				DebugMessage ("F Symmetry " + editor.symmetryMode.ToString());
                editor.symmetryMode = editor.symmetryMode + (shiftKeyPressed ? -1 : 1);
				DebugMessage ("G Symmetry  " + editor.symmetryMode.ToString());
			}
        }
        */


        //idea-delete single part from group?
    }

	GUIStyle labelStyle;
	void InitLabelStyle()
	{
		labelStyle = new GUIStyle ("Label");
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontSize = 22;
		//labelStyle.fontStyle = FontStyle.Bold;
		labelStyle.normal.textColor = XKCDColors.DarkYellow;
	}

    public void OnGUI()
    {
		//need to verify the EditorLogic state - do we need to fetch it every time?
		editor = EditorLogic.fetch;
		if (editor == null)
			return;

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
}



