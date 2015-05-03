using System;
using UnityEngine;
using System.Collections.Generic;

namespace EditorExtensions
{
	public class PartInfoWindow : MonoBehaviour
	{
		//public static SettingsWindow Instance { get; private set; }
		//public bool Visible { get; set; }

		public delegate void WindowDisabledEventHandler ();

		public event WindowDisabledEventHandler WindowDisabled;

		protected virtual void OnWindowDisabled ()
		{
			if (WindowDisabled != null)
				WindowDisabled ();
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

		void CloseWindow ()
		{
			this.enabled = false;
			OnWindowDisabled ();
		}

		void OnDisable ()
		{
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
		//GUILayoutOption[] settingsLabelLayout = new GUILayoutOption[] { GUILayout.MinWidth (150) };
		private int _toolbarInt = 0;
		private string[] _toolbarStrings = { "Part", "strut", "srfAttach", "attachNodes" };

		void WindowContent (int windowID)
		{
			_toolbarInt = GUILayout.Toolbar (_toolbarInt, _toolbarStrings);

			GUILayout.BeginVertical ("box");

//			int activeGizmos = -1;
//			try{
			//bad cpu impact
//				activeGizmos = HighLogic.FindObjectsOfType<EditorGizmos.GizmoOffset>().Length + HighLogic.FindObjectsOfType<EditorGizmos.GizmoRotate>().Length;
//			} catch(Exception){
//			}
//			GUILayout.Label ("Gizmos: " + activeGizmos.ToString());

			//Get selected part, mouseover part if none is active
			Part sp = EditorLogic.SelectedPart;
			if (sp == null)
				sp = Utility.GetPartUnderCursor ();

			if (sp != null) {

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Current Part:");
				GUILayout.Label (sp ? sp.name : "none");
				GUILayout.EndHorizontal ();

				GUILayout.Label ("Type: " + sp.GetType ().ToString ());

				if (_toolbarInt == 0) {

					PartInfoLabels (sp);

					if (sp.srfAttachNode != null) {
						GUILayout.Label ("srfAttachNode.position: " + sp.srfAttachNode.position.ToString (vectorFormat));
						GUILayout.BeginVertical ("box");
						GUILayout.Label ("Attached part:");
						if (sp.srfAttachNode.attachedPart != null) {
							PartInfoLabels (sp.srfAttachNode.attachedPart);
						}
						GUILayout.EndVertical ();
					}
				}

				if (_toolbarInt == 1) {
					if (sp.GetType () == typeof(CompoundPart)) {
						CompoundPartInfo ((CompoundPart)sp);
					}
				}

				if (_toolbarInt == 2) {
					SrfAttaches (sp);
				}

				if (_toolbarInt == 3) {
					AttachNodeLabels (sp);
				}


			} else {
				GUILayout.Label ("No part selected");
			}

			GUILayout.EndVertical ();//end main content

			if (GUILayout.Button ("Close")) {
				this.enabled = false;
			}

			GUI.DragWindow ();
		}

		void SrfAttaches (Part part)
		{
			List<Part> children = part.children;

			if (children == null) {
				GUILayout.Label ("children null");
				return;
			}

			for (int i = 0; i < children.Count; i++) {
				GUILayout.Label ("child srfAttachNode #" + i.ToString ());

				GUILayout.Label ("position " + children [i].srfAttachNode.position.ToString (vectorFormat));

				if (children [i].srfAttachNode.attachedPart != null)
					GUILayout.Label ("attached part " + children [i].srfAttachNode.attachedPart.name);

				GUILayout.Label ("offset " + children [i].srfAttachNode.offset.ToString (vectorFormat));
				GUILayout.Label ("orientation " + children [i].srfAttachNode.orientation.ToString (vectorFormat));
				GUILayout.Label ("nodeType " + children [i].srfAttachNode.nodeType.ToString ());
				if (children [i].srfAttachNode.nodeTransform != null) {
					GUILayout.Label ("nodeTransform.position " + children [i].srfAttachNode.nodeTransform.position.ToString (vectorFormat));
					GUILayout.Label ("nodeTransform.up " + children [i].srfAttachNode.nodeTransform.up.ToString (vectorFormat));
				} else {
					GUILayout.Label ("nodeTransform null");
				}
			}
		}

		void AttachNodeLabels (Part part)
		{
			List<AttachNode> nodes = part.attachNodes;

			if (nodes == null) {
				GUILayout.Label ("nodes null");
				return;
			}

			for (int i = 0; i < nodes.Count; i++) {
				GUILayout.Label ("Attach Node #" + i.ToString ());

				GUILayout.Label ("position " + nodes [i].position.ToString (vectorFormat));

				if (nodes [i].attachedPart != null)
					GUILayout.Label ("attached part " + nodes [i].attachedPart.name);

				GUILayout.Label ("offset " + nodes [i].offset.ToString (vectorFormat));

				GUILayout.Label ("orientation " + nodes [i].orientation.ToString (vectorFormat));
			}
		}

		void PartInfoLabels (Part part)
		{

			//GUILayout.Label ("allowSrfAttach " + (EditorLogic.SelectedPart.attachRules.allowSrfAttach ? "enabled" : "disabled"));
			GUILayout.Label ("srfAttach: " + (part.attachRules.srfAttach ? "enabled" : "disabled"));
			//GUILayout.Label ("allowCollision " + (EditorLogic.SelectedPart.attachRules.allowCollision ? "enabled" : "disabled"));
			//GUILayout.Label ("allowStack " + (EditorLogic.SelectedPart.attachRules.allowStack ? "enabled" : "disabled"));
			//GUILayout.Label ("allowDock " + (EditorLogic.SelectedPart.attachRules.allowDock ? "enabled" : "disabled"));
			//GUILayout.Label ("allowRotate " + (EditorLogic.SelectedPart.attachRules.allowRotate ? "enabled" : "disabled"));
			//GUILayout.Label ("stack " + (EditorLogic.SelectedPart.attachRules.stack ? "enabled" : "disabled"));

			//foreach (var child in part.children) {
			//	GUILayout.Label ("child: " + child.name);
			//}

			if (part.collider != null) {
				GUILayout.Label ("part.collider not null");
			}

			if (part.gameObject != null && part.gameObject.collider != null) {
				GUILayout.Label ("part.gameObject.collider not null");
			}

			GUILayout.Label ("isAttached " + part.isAttached.ToString ());
			GUILayout.Label ("attRotation: " + part.attRotation.ToString (vectorFormat));
			GUILayout.Label ("attRotation0: " + part.attRotation0.ToString (vectorFormat));
			GUILayout.Label ("attPos: " + part.attPos.ToString (vectorFormat));
			GUILayout.Label ("attPos0: " + part.attPos0.ToString (vectorFormat));

			GUILayout.Label ("localPosition " + part.transform.localPosition.ToString (vectorFormat));
			GUILayout.Label ("position " + part.transform.position.ToString (vectorFormat));
			GUILayout.Label ("localRotation " + part.transform.localRotation.ToString (vectorFormat));
			GUILayout.Label ("rotation " + part.transform.rotation.ToString (vectorFormat));


			GUILayout.Label ("localScale " + part.transform.localScale.ToString (vectorFormat));
			GUILayout.Label ("lossyScale " + part.transform.lossyScale.ToString (vectorFormat));
			GUILayout.Label ("right " + part.transform.right.ToString (vectorFormat));
			GUILayout.Label ("up " + part.transform.up.ToString (vectorFormat));

			GUILayout.Label ("extents " + part.GetPartRendererBound ().extents.ToString (vectorFormat));
			GUILayout.Label ("size " + part.GetPartRendererBound ().size.ToString (vectorFormat));

			try {				
				//GUILayout.Label ("GetPartRendererBound() extents " + part.GetPartRendererBound().extents.ToString(vectorFormat));
			} catch (Exception) {
				//GUILayout.Label ("bounds.extents error");
			}

			GUILayout.Label ("orgPos: " + part.orgPos.ToString (vectorFormat));
		}

		void CompoundPartInfo (CompoundPart part)
		{
			GUILayout.Label ("name: " + part.name);
			GUILayout.Label ("direction: " + part.direction.ToString (vectorFormat));

			GUILayout.Label ("attachState: " + part.attachState.ToString ());
			if (part.target != null) {
				GUILayout.Label ("target: " + part.target.name);
				GUILayout.Label ("targetPosition: " + part.targetPosition.ToString (vectorFormat));
				GUILayout.Label ("targetRotation: " + part.targetRotation.ToString (vectorFormat));

			}
			//GUILayout.Label ("xxx: " + part.direction.ToString (vectorFormat));
		}

	}
}

