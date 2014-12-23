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
			MonoBehaviour.print (MessagePrefix + message);
#endif
		}

		public static void Info(string message)
		{
			MonoBehaviour.print (MessagePrefix + message);
		}

		public static void Error(string message)
		{
			MonoBehaviour.print (MessagePrefix + message);
		}
	}
}

