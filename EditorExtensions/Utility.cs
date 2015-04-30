using System;
using UnityEngine;

namespace EditorExtensions
{
	public static class Utility
	{
		public static Part GetPartUnderCursor ()
		{
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			EditorLogic ed = EditorLogic.fetch;

			//Physics.Raycast(this.transform.position, this.transform.TransformDirection(dir), out this.\u0001\u0002, this.maxLength, EditorLogic.LayerMask)

			if (ed != null && Physics.Raycast (ray, out hit)) {
				return ed.ship.Parts.Find (p => p.gameObject == hit.transform.gameObject);
			}
			return null;
		}
	}
}

