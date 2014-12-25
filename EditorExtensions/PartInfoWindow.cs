using System;
using UnityEngine;

namespace EditorExtensions
{
	public class PartInfoWindow : MonoBehaviour
	{
		//public static SettingsWindow Instance { get; private set; }
		//public bool Visible { get; set; }

		public delegate void WindowDisabledEventHandler();
		public event WindowDisabledEventHandler WindowDisabled;
		protected virtual void OnWindowDisabled() 
		{
			if (WindowDisabled != null)
				WindowDisabled();
		}

		string _windowTitle = "Part Position Info";

		Rect _windowRect = new Rect () {
			xMin = Screen.width - 350,
			xMax = Screen.width - 50,
			yMin = 100,
			yMax = 100 //0 height, GUILayout resizes it
		};

		//ctor
		public PartInfoWindow ()
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

		const string vectorFormat = "F3";
		GUILayoutOption[] settingsLabelLayout = new GUILayoutOption[] { GUILayout.MinWidth (150) };
		void WindowContent (int windowID)
		{
			GUILayout.BeginVertical ("box");

			//string prefabName = getcom(365447);
			//GameObject prefab = AssetBase.GetPrefab(prefabName);
			//UnityEngine.Object obj4 = Object.Instantiate(prefab);
			//GizmoOffset offset = ((GameObject) obj4).GetComponent<GizmoOffset>();

			int activeGizmos = -1;
			try{
				activeGizmos = HighLogic.FindObjectsOfType<EditorGizmos.GizmoOffset>().Length + HighLogic.FindObjectsOfType<EditorGizmos.GizmoRotate>().Length;
			} catch(Exception){
			}
			GUILayout.Label ("Gizmos: " + activeGizmos.ToString());

			//Get selected part, mouseover part if none is active
			Part sp = EditorLogic.SelectedPart;
			if (sp == null)
				sp = Utility.GetPartUnderCursor ();

			if (sp != null) {

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Current Part:");
				GUILayout.Label (sp ? sp.name : "none");
				GUILayout.EndHorizontal ();

				//GUILayout.Label ("allowSrfAttach " + (EditorLogic.SelectedPart.attachRules.allowSrfAttach ? "enabled" : "disabled"));
				GUILayout.Label ("srfAttach: " + (sp.attachRules.srfAttach ? "enabled" : "disabled"));
				//GUILayout.Label ("allowCollision " + (EditorLogic.SelectedPart.attachRules.allowCollision ? "enabled" : "disabled"));
				//GUILayout.Label ("allowStack " + (EditorLogic.SelectedPart.attachRules.allowStack ? "enabled" : "disabled"));
				//GUILayout.Label ("allowDock " + (EditorLogic.SelectedPart.attachRules.allowDock ? "enabled" : "disabled"));
				//GUILayout.Label ("allowRotate " + (EditorLogic.SelectedPart.attachRules.allowRotate ? "enabled" : "disabled"));
				//GUILayout.Label ("stack " + (EditorLogic.SelectedPart.attachRules.stack ? "enabled" : "disabled"));

				//foreach (var child in sp.children) {
				//	GUILayout.Label ("child: " + child.name);
				//}

				GUILayout.Label ("localPosition " + sp.transform.localPosition.ToString (vectorFormat));
				GUILayout.Label ("position " + sp.transform.position.ToString (vectorFormat));
				GUILayout.Label ("rotation " + sp.transform.rotation.ToString (vectorFormat));
				GUILayout.Label ("attRotation: " + sp.attRotation.ToString (vectorFormat));
				GUILayout.Label ("attRotation0: " + sp.attRotation0.ToString (vectorFormat));
				//attPos doesnt seem to be used
				GUILayout.Label ("attPos: " + sp.attPos.ToString (vectorFormat));
				GUILayout.Label ("attPos0: " + sp.attPos0.ToString (vectorFormat));
				GUILayout.Label ("isAttached " + sp.isAttached.ToString ());

				GUILayout.Label ("orgPos: " + sp.orgPos.ToString (vectorFormat));

				if (sp.srfAttachNode != null) {
					GUILayout.Label ("srfAttachNode.position: " + sp.srfAttachNode.position.ToString (vectorFormat));

					GUILayout.BeginVertical ("box");
					GUILayout.Label ("Attached part:");
					if (sp.srfAttachNode.attachedPart != null) {
						GUILayout.Label ("attPos0: " + sp.srfAttachNode.attachedPart.attPos0.ToString (vectorFormat));
						GUILayout.Label ("localPosition " + sp.srfAttachNode.attachedPart.transform.localPosition.ToString (vectorFormat));
						GUILayout.Label ("position " + sp.srfAttachNode.attachedPart.transform.position.ToString (vectorFormat));
						GUILayout.Label ("rotation " + sp.srfAttachNode.attachedPart.transform.rotation.ToString (vectorFormat));
						GUILayout.Label ("up " + sp.srfAttachNode.attachedPart.transform.up.ToString (vectorFormat));

						AttachNode an = sp.srfAttachNode.attachedPart.attachNodes [0];
						GUILayout.Label ("attachNode " + an.position.ToString (vectorFormat));
						//sp.attPos0.y = sp.srfAttachNode.attachedPart.attPos0.y;
					}

					GUILayout.EndVertical ();
				}
			}			

			GUILayout.EndVertical ();//end main content

			if (GUILayout.Button ("Close")) {
				this.enabled = false;
			}

			GUI.DragWindow ();
		}

	}
}

