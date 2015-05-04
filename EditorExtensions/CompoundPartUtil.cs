using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EditorExtensions
{
	public static class CompoundPartUtil
	{
		const string VectorFormat = "F3";

		static CompoundPartUtil()
		{
		}

		//get distance: Vector3.Distance(object1.transform.position, object2.transform.position);
		//Physics.Raycast(this.transform.position, this.transform.TransformDirection(dir), out this.\u0001\u0002, this.maxLength, EditorLogic.LayerMask)

		const string strutPartName = "strutConnector";
		const string fuelLinePartName = "fuelLine";
		const float CompoundPartMaxLength = 10f;

		[Obsolete]
		public static void AttachStrut (Part startPart, Part destPart)
		{

			// Make a new strut object
			AvailablePart ap = PartLoader.getPartInfoByName(strutPartName);
			UnityEngine.Object obj = UnityEngine.Object.Instantiate(ap.partPrefab);

			CompoundPart strut = (CompoundPart)obj;
			strut.gameObject.SetActive(true);
			strut.gameObject.name = ap.name;
			strut.partInfo = ap;
			//strut.attachMode = AttachModes.SRF_ATTACH;
			//strut.attachMethod = AttachNodeMethod.FIXED_JOINT;
			//strut.attachState = CompoundPart.AttachState.Attached;
			//strut.SetMirror(Vector3.one);
			//strut.maxLength = CompoundPartMaxLength; //default

			Log.Debug("Created new strut");

			// set position in space, relative to source tank
			strut.transform.localScale = startPart.transform.localScale;
			strut.transform.parent = startPart.transform; // must be BEFORE localposition!

			Vector3 midway = Vector3.Lerp(startPart.transform.position, destPart.transform.position, 0.5f);

			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();

			Log.Debug("dist: " + (Vector3.Distance(startPart.transform.position, midway)).ToString("F2"));
			Log.Debug("dist: " + (Vector3.Distance(destPart.transform.position, midway)).ToString("F2"));


			getStartDestPositions(startPart, destPart, midway, out startPosition, out destPosition);

			strut.transform.position = startPosition;
			Log.Debug("set strut transform position");

			// Aim the fuel node starting position at the destination position so we can calculate the direction later
			strut.transform.up = startPart.transform.up;
			strut.transform.forward = startPart.transform.forward;
			strut.transform.LookAt(destPart.transform);
			strut.transform.Rotate(0, 90, 0);
			Log.Debug("set strut transform");

			// attach to source part
			AttachNode an = new AttachNode ();
			an.id = "srfAttach";
			an.attachedPart = startPart;
			an.attachMethod = AttachNodeMethod.HINGE_JOINT;
			an.nodeType = AttachNode.NodeType.Surface;
			an.size = 1;  // found from inspecting fuel lines
			an.orientation = new Vector3 (0.125f, 0.0f, 0.0f); // seems to be a magic number
			Log.Debug("created attachnode");
			strut.srfAttachNode = an;
			Log.Debug("set strut attachnode");

			// attach to destination part
			strut.target = destPart;
			strut.targetPosition = destPosition;
			strut.targetRotation = destPart.transform.rotation;

			//strut.direction=(strut.transform.position - destPart.transform.position).normalized;
			//strut.direction = strut.transform.localRotation * strut.transform.localPosition;  // only works if destPart is parent
			//strut.direction = (strut.transform.InverseTransformPoint(destPart.transform.position) - strut.transform.localPosition).normalized;  // works but crooked
			//strut.direction = (strut.transform.InverseTransformPoint(destPosition) - strut.transform.localPosition).normalized; // doesn't connect
			//strut.direction = (strut.transform.InverseTransformPoint(destPosition) - strut.transform.localPosition); // doesn't connect
			strut.direction = strut.transform.InverseTransformPoint(destPart.transform.position).normalized;  // correct!
			Log.Debug("strut direction");

			// add to ship
			startPart.addChild(strut);
			Log.Debug("added strut as child of start part");

			EditorLogic.fetch.ship.Add(strut);
			Log.Debug("added strut to ship");

			strut.raycastTarget(strut.transform.InverseTransformPoint(destPart.transform.position).normalized);
			Log.Debug("strut raycastTarget");

			EditorLogic.fetch.SetBackup();
		}

		public static void CenterStrut (CompoundPart strut)
		{
			Part startPart = strut.parent;
			Part destPart = strut.target;

			Vector3 midway = Vector3.Lerp(startPart.transform.position, destPart.transform.position, 0.5f);

			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();

			GetEndpoints(startPart, destPart, midway, out startPosition, out destPosition);

			strut.transform.position = startPosition;

			strut.transform.up = startPart.transform.up;
			strut.transform.forward = startPart.transform.forward;
			strut.transform.LookAt(destPart.transform);
			strut.transform.Rotate(0, 90, 0);

			strut.raycastTarget(strut.transform.InverseTransformPoint(destPart.transform.position).normalized);
			EditorLogic.fetch.SetBackup();
		}

		/// <summary>
		/// Align compount part, leave in starting position but center and level to target
		/// </summary>
		/// <param name="part">Part</param>
		public static void AlignCompoundPart (CompoundPart part)
		{
			if (part.parent == null || part.target == null) {
				Log.Debug("Part is not fully connected");
				return;
			}

			Part startPart = part.parent;
			Part targetPart = part.target;

			float parentLocalHeight = GetCompoundPartPositionHeight(part, part.parent);
			//float targetLocalHeight = GetCompoundPartPositionHeight(part, part.target);

			//position vertically on parent
			//part.transform.localPosition = new Vector3 (part.transform.localPosition.x, parentLocalHeight, part.transform.localPosition.z);

			//Vector3 startPartPosition = startPart.transform.position;
			//translate start position along major axis
			//startPartPosition = startPart.transform.Translate(0, 0, parentLocalHeight, startPart.transform);

			//new target position
			Vector3 targetPartPosition = targetPart.transform.position;
			//add target local height to dest position along up axis
			//maybe try up and down to find shortest?

			Vector3 midway = Vector3.Lerp(part.transform.position, targetPartPosition, 0.5f);

			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();
			//center to center
			GetEndpoints(startPart, targetPart, midway, out startPosition, out destPosition);

			//Vector3 orgPosition = part.transform.position;
			//part.transform.position = new Vector3 (startPosition.x, orgPosition.y, startPosition.z);

			Vector3 originPos = new Vector3 (0, parentLocalHeight, 0);
			Log.Debug("Local origin position: " + originPos.ToString(VectorFormat));

			GetCollisionPointOnAxis(startPart, originPos,
				startPart.transform.InverseTransformPoint(targetPart.transform.position),
				out startPosition);
			Log.Debug("Collided local start position: " + startPosition.ToString(VectorFormat));

			startPosition = startPart.transform.TransformPoint(startPosition);

			Log.Debug("Collided global start position: " + startPosition.ToString(VectorFormat));
			
			part.transform.position = startPosition;
			part.transform.up = startPart.transform.up;
			part.transform.forward = startPart.transform.forward;
			part.transform.LookAt(targetPart.transform); //this rotates the strut base towards the target, need to keep it flush with the parent
			part.transform.Rotate(0, 90, 0);

			//Vector3 dirToTarget = part.transform.InverseTransformPoint(targetPart.transform.position).normalized;
			Vector3 dirToTarget = part.transform.InverseTransformPoint(destPosition).normalized;
			//level vertically
			dirToTarget.y = 0f;

			part.raycastTarget(dirToTarget);
			EditorLogic.fetch.SetBackup();
		}

		static float GetCompoundPartPositionHeight (CompoundPart part, Part target)
		{
			Log.Debug(string.Format("Getting new position height for {0} on {1}", part.name, target.name));

			//lengthwise position on parent (extents, +/- from center)
			float localHeight = part.transform.localPosition.y;
			float parentHeight = part.parent.GetPartRendererBound().extents.y;

			//offset strut when attaching to top/bottom of parent
			const float strutOffset = 0.1f;
			//threshold for small parent parts
			const float parentSizeCutoff = 0.5f;

			float heightPct = localHeight / parentHeight;
			Log.Debug("Attachment height%: " + (heightPct * 100f).ToString("F0"));

			Log.Debug("Original localHeight: " + localHeight.ToString());

			//+1 up, -1 down
			float upOrDown = 1f;
			if (localHeight < 0f)
				upOrDown = -1f;

			if (parentHeight < parentSizeCutoff) {
				//for small parts, just center on them
				Log.Debug("Parent is small, defaulting to center");
				localHeight = 0f;
			} else if (Math.Abs(localHeight) < parentHeight * 0.1f) {
				//middle 20% of parent, snap to center (10% of extent(
				Log.Debug("Centering on parent");
				localHeight = 0f;
			} else if (Math.Abs(localHeight) < parentHeight * 0.6f) {
				//top/bottom quarter (60% of extent)
				Log.Debug("Centering quarter on parent");
				localHeight = parentHeight / 2 * upOrDown;
			} else {
				//top/bottom edge
				Log.Debug("Aligning to edge of parent");
				localHeight = (parentHeight - strutOffset) * upOrDown;
			}
			Log.Debug("new localHeight: " + localHeight.ToString());
			return localHeight;
		}

		private static Boolean isPathObstructed (Part startPart, Part destPart, Vector3 midway)
		{
			Vector3 startPosition = new Vector3 ();
			Vector3 destPosition = new Vector3 ();
			GetEndpoints(startPart, destPart, midway, out startPosition, out destPosition);

			List<Part> parts = EditorLogic.fetch.ship.parts;

			Vector3 collisionpoint = new Vector3 ();
			foreach (Part p in parts) {
				if (p == startPart || p == destPart) {
					continue;
				} else if (GetPartCollisionPoint(p, startPosition, destPosition, out collisionpoint)) {
					Log.Debug("Path obstructed by part: " + p.name);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the endpoints on two part's colliders given a midway point between the parts
		/// </summary>
		private static bool GetEndpoints (Part startPart, Part destPart, Vector3 midway, out Vector3 startPosition, out Vector3 destPosition)
		{
			if (
				GetPartCollisionPoint(startPart, midway, startPart.transform.position, out startPosition)
				&
				GetPartCollisionPoint(destPart, midway, destPart.transform.position, out destPosition))
				return true;
			else
				return false;
		}



		/// <summary>
		/// Gets the collision point towards destination, perpendicular to part's axis (first to edge of collider, then to target: \_ )
		/// </summary>
		private static bool GetCollisionPointOnAxis (Part part, Vector3 origin, Vector3 target, out Vector3 collisionPoint)
		{
			Vector3 targetDirection = (target - origin).normalized;

			targetDirection = new Vector3 (targetDirection.x, 0f, targetDirection.z);

			return GetPartCollisionPoint(part, origin, targetDirection, out collisionPoint);
		}

		/// <summary>
		/// Gets the collision point on the part's collider, from origin inside part.
		/// </summary>
		private static bool GetPartCollisionPoint (Part part, Vector3 origin, Vector3 target, out Vector3 collisionPoint)
		{
			Vector3 targetDirection = (target - origin).normalized;
			return GetPartCollisionPoint(part, origin, targetDirection, out collisionPoint);
		}

		/// <summary>
		/// get collision point starting from inside the part's collider
		/// </summary>
		private static bool GetPartCollisionPointByDirection (Part part, Vector3 origin, Vector3 targetDirection, out Vector3 collisionPoint)
		{
			//using this method becuase Physics.RayCast ignores hits originating from inside the collider
			Ray r = new Ray (origin, targetDirection);
			RaycastHit hit = new RaycastHit ();
			if (part.collider.Raycast(r, out hit, CompoundPartMaxLength)) {
				collisionPoint = hit.point;
				return true;
			} else {
				collisionPoint = origin;
				return false;
			}
		}

		[Obsolete]
		private static void getStartDestPositions (Part startPart, Part destPart, Vector3 midway, out Vector3 startPosition, out Vector3 destPosition)
		{
			fireRayAt(startPart, midway, startPart.transform.position, out startPosition);
			fireRayAt(destPart, midway, destPart.transform.position, out destPosition);
		}

		[Obsolete]
		private static bool fireRayAt (Part p, Vector3 origin, Vector3 dest, out Vector3 collisionpoint)
		{
			Ray r = new Ray ();
			r.origin = origin;
			r.direction = vector3direction(origin, dest);
			float distance = Vector3.Distance(origin, dest);

			RaycastHit hit = new RaycastHit ();
			if (p.collider.Raycast(r, out hit, distance)) {
				collisionpoint = hit.point;
				return true;
			} else {
				collisionpoint = origin;
				return false;
			}
		}

		[Obsolete]
		private static Vector3 vector3direction (Vector3 from, Vector3 to)
		{
			// I can never remember which way the math goes
			return (to - from).normalized;
		}
	}
}

