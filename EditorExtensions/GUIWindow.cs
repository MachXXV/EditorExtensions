using System;
using UnityEngine;

namespace EditorExtensions
{
	public abstract class GUIWindow : MonoBehaviour
	{
		public static SettingsWindow Instance { get; private set; }
		public bool Visible { get; set; }

		public delegate void WindowDisabledEventHandler();
		public event WindowDisabledEventHandler WindowDisabled;
		protected virtual void OnWindowDisabled() 
		{
			if (WindowDisabled != null)
				WindowDisabled();
		}

		string _windowTitle = string.Empty;

		Rect _windowRect = new Rect () {
			xMin = Screen.width/2 - 100,
			xMax = Screen.width/2 + 100,
			yMin = Screen.height/2 - 50,
			yMax = Screen.height/2 + 50 //0 height, GUILayout resizes it
		};

		//ctor
		public GUIWindow ()
		{
			//start disabled
			this.enabled = false;
		}

		void Awake ()
		{
		}

		void Update ()
		{
		}

		void OnEnable ()
		{
		}

		void CloseWindow(){
			this.enabled = false;
			OnWindowDisabled ();
		}

		void OnDisable(){
		}

		void OnGUI ()
		{
			if (Event.current.type == EventType.Layout) {
				_windowRect.yMax = _windowRect.yMin;
				_windowRect = GUILayout.Window (this.GetInstanceID (), _windowRect, WindowContent, _windowTitle);
			}
		}

		void OnDestroy ()
		{
		}

		/// <summary>
		/// Initializes the window content and enables it
		/// </summary>
		public void Show ()
		{
			this.enabled = true;
		}

		void WindowContent (int windowID)
		{

		}
	}
}

