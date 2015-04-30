using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EditorExtensions
{
	public static class AutoStrut
	{
		static AutoStrut ()
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
			CompoundPart f = (CompoundPart)obj;
			f.gameObject.SetActive (true);
			f.gameObject.name = strutPartName;
			f.partInfo = ap;
			f.attachMode = AttachModes.SRF_ATTACH;

			// set position in space, relative to source tank
			f.transform.localScale = startPart.transform.localScale;
			f.transform.parent = startPart.transform; // must be BEFORE localposition!

			Vector3 midway = Vector3.Lerp (startPart.transform.position, destPart.transform.position, 0.5f);

			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();

			Log.Debug ("dist: " + (Vector3.Distance (startPart.transform.position, midway)).ToString ("F2"));
			Log.Debug ("dist: " + (Vector3.Distance (destPart.transform.position, midway)).ToString ("F2"));

			float adjustmentincrement = 0.5f; // how much to move the midpoint
			float adjustment = 0.0f;
			bool flcollide = isFLpathObstructed (startPart, destPart, midway);
			while ((flcollide) && (adjustment < 3)) {
				Vector3 newmidway = new Vector3 (midway.x, midway.y, midway.z);
				adjustment = adjustment + adjustmentincrement;
				adjustmentincrement = adjustmentincrement * 2f;

				foreach (float yinc in new float[] {0f, adjustmentincrement, -adjustmentincrement}) {
					newmidway.y = midway.y + yinc;
					foreach (float xinc in new float[] {0f, adjustmentincrement, -adjustmentincrement}) {
						newmidway.x = midway.x + xinc;
						foreach (float zinc in new float[] {0f, adjustmentincrement, -adjustmentincrement}) {
							newmidway.z = midway.z + zinc;
							flcollide = isFLpathObstructed (startPart, destPart, newmidway);
							if (!flcollide) {
								midway = newmidway;
								break;
							}
						}
						if (!flcollide) {
							midway = newmidway;
							break;
						}
					}
					if (!flcollide) {
						midway = newmidway;
						break;
					}
				}
			}
			//ASPConsoleStuff.printVector3 ("New midway is", midway);
			getStartDestPositions (startPart, destPart, midway, out startPosition, out destPosition);

			f.transform.position = startPosition;

			// Aim the fuel node starting position at the destination position so we can calculate the direction later
			f.transform.up = startPart.transform.up;
			f.transform.forward = startPart.transform.forward;
			f.transform.LookAt (destPart.transform);
			f.transform.Rotate (0, 90, 0);

			// attach to source part
			AttachNode an = new AttachNode ();
			an.id = "srfAttach";
			an.attachedPart = startPart;
			an.attachMethod = AttachNodeMethod.HINGE_JOINT;
			an.nodeType = AttachNode.NodeType.Surface;
			an.size = 1;  // found from inspecting fuel lines
			an.orientation = new Vector3 (0.12500000f, 0.0f, 0.0f); // seems to be a magic number
			f.srfAttachNode = an;

			// attach to destination part
			f.target = destPart;

			f.targetPosition = destPosition;

			//f.direction=(f.transform.position - destPart.transform.position).normalized;
			//f.direction = f.transform.localRotation * f.transform.localPosition;  // only works if destPart is parent
			//f.direction = (f.transform.InverseTransformPoint(destPart.transform.position) - f.transform.localPosition).normalized;  // works but crooked
			//f.direction = (f.transform.InverseTransformPoint(destPosition) - f.transform.localPosition).normalized; // doesn't connect
			//f.direction = (f.transform.InverseTransformPoint(destPosition) - f.transform.localPosition); // doesn't connect
			f.direction = f.transform.InverseTransformPoint (destPart.transform.position).normalized;  // correct!

			// add to ship
			startPart.addChild (f);

			EditorLogic.fetch.ship.Add (f);

		}

		private static Boolean isFLpathObstructed (Part startPart, Part destPart, Vector3 midway)
		{
			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();
			getStartDestPositions (startPart, destPart, midway, out startPosition, out destPosition);

			EditorLogic editor = EditorLogic.fetch;
			ShipConstruct ship = editor.ship;
			List<Part> parts = ship.parts;

			//ASPConsoleStuff.printVector3 ("testing midway", midway);
			Vector3 collisionpoint = new Vector3 ();
			foreach (Part p in parts) {
				if ((p == startPart) || (p == destPart)) {
					continue;
				}
				if (fireRayAt (p, startPosition, destPosition, out collisionpoint)) {
					//ASPConsoleStuff.printPart ("**** fuel line is obstructed at " + collisionpoint.ToString ("F2") + " by", p);
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
				//ASPConsoleStuff.printPart ("!! ray failed aiming at", p);
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

