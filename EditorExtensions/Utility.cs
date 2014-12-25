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

			if (ed != null && Physics.Raycast (ray, out hit)) {
				return ed.ship.Parts.Find (p => p.gameObject == hit.transform.gameObject);
			}
			return null;
		}
	}
}

