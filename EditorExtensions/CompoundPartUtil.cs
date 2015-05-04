using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EditorExtensions
{
	public static class CompoundPartUtil
	{
		static CompoundPartUtil ()
		{
		}

		//get distance: Vector3.Distance(object1.transform.position, object2.transform.position);

		const string strutPartName = "strutConnector";
		const string fuelLinePartName = "fuelLine";

		public static void AttachStrut (Part startPart, Part destPart)
		{

			// Make a new strut object
			AvailablePart ap = PartLoader.getPartInfoByName (strutPartName);
			UnityEngine.Object obj = UnityEngine.Object.Instantiate (ap.partPrefab);

			CompoundPart strut = (CompoundPart)obj;
			strut.gameObject.SetActive (true);
			strut.gameObject.name = ap.name;
			strut.partInfo = ap;
			//strut.attachMode = AttachModes.SRF_ATTACH;
			//strut.attachMethod = AttachNodeMethod.FIXED_JOINT;
			//strut.attachState = CompoundPart.AttachState.Attached;
			//strut.SetMirror(Vector3.one);
			//strut.maxLength = 10f; //default

			Log.Debug ("Created new strut");

			// set position in space, relative to source tank
			strut.transform.localScale = startPart.transform.localScale;
			strut.transform.parent = startPart.transform; // must be BEFORE localposition!

			Vector3 midway = Vector3.Lerp (startPart.transform.position, destPart.transform.position, 0.5f);

			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();

			Log.Debug ("dist: " + (Vector3.Distance (startPart.transform.position, midway)).ToString ("F2"));
			Log.Debug ("dist: " + (Vector3.Distance (destPart.transform.position, midway)).ToString ("F2"));

			getStartDestPositions (startPart, destPart, midway, out startPosition, out destPosition);

			strut.transform.position = startPosition;
			Log.Debug ("set strut transform position");

			// Aim the fuel node starting position at the destination position so we can calculate the direction later
			strut.transform.up = startPart.transform.up;
			strut.transform.forward = startPart.transform.forward;
			strut.transform.LookAt (destPart.transform);
			strut.transform.Rotate (0, 90, 0);
			Log.Debug ("set strut transform");

			// attach to source part
			AttachNode an = new AttachNode ();
			an.id = "srfAttach";
			an.attachedPart = startPart;
			an.attachMethod = AttachNodeMethod.HINGE_JOINT;
			an.nodeType = AttachNode.NodeType.Surface;
			an.size = 1;  // found from inspecting fuel lines
			an.orientation = new Vector3 (0.125f, 0.0f, 0.0f); // seems to be a magic number
			Log.Debug ("created attachnode");
			strut.srfAttachNode = an;
			Log.Debug ("set strut attachnode");

			// attach to destination part
			strut.target = destPart;
			strut.targetPosition = destPosition;
			strut.targetRotation = destPart.transform.rotation;

			//strut.direction=(strut.transform.position - destPart.transform.position).normalized;
			//strut.direction = strut.transform.localRotation * strut.transform.localPosition;  // only works if destPart is parent
			//strut.direction = (strut.transform.InverseTransformPoint(destPart.transform.position) - strut.transform.localPosition).normalized;  // works but crooked
			//strut.direction = (strut.transform.InverseTransformPoint(destPosition) - strut.transform.localPosition).normalized; // doesn't connect
			//strut.direction = (strut.transform.InverseTransformPoint(destPosition) - strut.transform.localPosition); // doesn't connect
			strut.direction = strut.transform.InverseTransformPoint (destPart.transform.position).normalized;  // correct!
			Log.Debug ("strut direction");

			// add to ship
			startPart.addChild (strut);
			Log.Debug ("added strut as child of start part");

			EditorLogic.fetch.ship.Add (strut);
			Log.Debug ("added strut to ship");

			strut.raycastTarget(strut.transform.InverseTransformPoint (destPart.transform.position).normalized);
			Log.Debug ("strut raycastTarget");

			EditorLogic.fetch.SetBackup ();
		}

		public static void CenterStrut (CompoundPart strut)
		{
			Part startPart = strut.parent;
			Part destPart = strut.target;

			Vector3 midway = Vector3.Lerp (startPart.transform.position, destPart.transform.position, 0.5f);

			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();

			getStartDestPositions (startPart, destPart, midway, out startPosition, out destPosition);

			strut.transform.position = startPosition;

			strut.transform.up = startPart.transform.up;
			strut.transform.forward = startPart.transform.forward;
			strut.transform.LookAt (destPart.transform);
			strut.transform.Rotate (0, 90, 0);

			strut.raycastTarget(strut.transform.InverseTransformPoint (destPart.transform.position).normalized);
			EditorLogic.fetch.SetBackup ();
		}

		/// <summary>
		/// Align compount part, leave in starting position but center and level to target
		/// </summary>
		/// <param name="part">Part</param>
		public static void AlignCompoundPart (CompoundPart part)
		{
			Part startPart = part.parent;
			Part destPart = part.target;

			Vector3 midway = Vector3.Lerp (startPart.transform.position, destPart.transform.position, 0.5f);

			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();

			//lengthwise position on parent (extents, +/- from center)
			float localHeight = part.transform.localPosition.y;
			float parentHeight = part.parent.GetPartRendererBound ().extents.y;

			//offset strut when attaching to top/bottom of parent
			const float strutOffset = 0.1f;
			//threshold for small parent parts
			const float parentSizeCutoff = 0.5f;

			float heightPct = localHeight / parentHeight;
			Log.Debug ("heightPct: " + heightPct.ToString ());

			Log.Debug ("Original localHeight: " + localHeight.ToString ());
			if (parentHeight < parentSizeCutoff) {
				//for small parts, just center on them
				localHeight = 0f;
			} else if (Math.Abs (localHeight) < parentHeight / 2) {
				//middle 50% of parent, snap to center
				localHeight = 0f;
			} else {
				if(localHeight < 0)
					localHeight = -(parentHeight - strutOffset);
				if (localHeight > 0)
					localHeight = parentHeight - strutOffset;
			}

			//position vertically
			part.transform.localPosition = new Vector3 (part.transform.localPosition.x, localHeight, part.transform.localPosition.z);

			Log.Debug ("new localHeight: " + localHeight.ToString ());

			getStartDestPositions (startPart, destPart, midway, out startPosition, out destPosition);

			Vector3 orgPosition = part.transform.position;
			part.transform.position = new Vector3(startPosition.x, orgPosition.y, startPosition.z);

			part.transform.up = startPart.transform.up;
			part.transform.forward = startPart.transform.forward;
			part.transform.LookAt (destPart.transform); //this rotates the strut base towards the target, need to keep it flush with the parent
			part.transform.Rotate (0, 90, 0);

			Vector3 dirToTarget = part.transform.InverseTransformPoint (destPart.transform.position).normalized;
			//level veritically
			dirToTarget.y = 0f;

			part.raycastTarget(dirToTarget);
			EditorLogic.fetch.SetBackup ();
		}

		private static Boolean isFLpathObstructed (Part startPart, Part destPart, Vector3 midway)
		{
			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();
			getStartDestPositions (startPart, destPart, midway, out startPosition, out destPosition);

			EditorLogic editor = EditorLogic.fetch;
			ShipConstruct ship = editor.ship;
			List<Part> parts = ship.parts;

			//ASPConsoleStuff .printVector3 ("testing midway", midway);
			Vector3 collisionpoint = new Vector3 ();
			foreach (Part p in parts) {
				if ((p == startPart) || (p == destPart)) {
					continue;
				}
				if (fireRayAt (p, startPosition, destPosition, out collisionpoint)) {
					//ASPConsoleStuff .printPart ("**** fuel line is obstructed at " + collisionpoint.ToString ("F2") + " by", p);
					return true;
				}
			}

			return false;
		}

		private static void getStartDestPositions (Part startPart, Part destPart, Vector3 midway, out Vector3 startPosition, out Vector3 destPosition)
		{
			fireRayAt (startPart, midway, startPart.transform.position, out startPosition);
			fireRayAt (destPart, midway, destPart.transform.position, out destPosition);
		}

		private static bool fireRayAt (Part p, Vector3 origin, Vector3 dest, out Vector3 collisionpoint)
		{
			Ray r = new Ray ();
			r.origin = origin;
			r.direction = vector3direction (origin, dest);
			float distance = Vector3.Distance (origin, dest);

			RaycastHit hit = new RaycastHit ();
			if (p.collider.Raycast (r, out hit, distance)) {
				collisionpoint = hit.point;
				return true;
			} else {
				collisionpoint = origin;
				//ASPConsoleStuff . printPart ("!! ray failed aiming at", p);
				return false;
			}
		}

		private static Vector3 vector3direction (Vector3 from, Vector3 to)
		{
			// I can never remember which way the math goes
			return (to - from).normalized;
		}
	}
}

