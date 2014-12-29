﻿using System;
using UnityEngine;

namespace EditorExtensions
{
	public class StrutWindow : GUIWindow
	{
		internal override void Awake ()
		{
			base.Awake ();
			_windowTitle = "Strutomatic 9000";
		}

		bool _toggle = false;
		internal override void WindowContent (int windowID)
		{

			_toggle = GUILayout.Toggle (_toggle, _toggle ? "On" : "Off", "Button");


			if (GUILayout.Button ("Close")) {
				CloseWindow ();
			}
			GUI.DragWindow ();
		}
	}
}

