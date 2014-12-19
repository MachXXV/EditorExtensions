using System;
using UnityEngine;

namespace EditorExtensions
{
	public static class Log
	{
		//set debug flag to toggle debugging messages
		#if DEBUG
		const bool debug = true;
		#else
		const bool debug = false;
		#endif

		public static void Debug (string message)
		{
			if (debug)
				MonoBehaviour.print ("EditorExtensions: " + message);
		}

		public static void Error(string message)
		{
			MonoBehaviour.print ("EditorExtensions: " + message);
		}
	}
}

