using System;
using UnityEngine;

namespace EditorExtensions
{
	public static class Log
	{
		const string MessagePrefix = "EditorExtensions: ";

		public static void Debug (string message)
		{
#if DEBUG
			UnityEngine.Debug.Log (MessagePrefix + message);
#endif
		}

		public static void Info(string message)
		{
			UnityEngine.Debug.Log (MessagePrefix + message);
		}

		public static void Error(string message)
		{
			UnityEngine.Debug.LogError (MessagePrefix + message);
		}

		public static void Warn(string message)
		{
			UnityEngine.Debug.LogWarning (MessagePrefix + message);
		}
	}
}

