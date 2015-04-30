using System;
using UnityEngine;

namespace EditorExtensions
{
	#if DEBUG
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class Debug_AutoLoadQuicksaveOnStartup: UnityEngine.MonoBehaviour
	{
		public static bool first = true;
		public void Start()
		{
			if (first)
			{
				first = false;
				HighLogic.SaveFolder = "default";
				var game = GamePersistence.LoadGame("quicksave", HighLogic.SaveFolder, true, false);
				if (game != null && game.flightState != null && game.compatible)
				{
					HighLogic.LoadScene(GameScenes.EDITOR);
				}
			}
		}
	}
	#endif
}

