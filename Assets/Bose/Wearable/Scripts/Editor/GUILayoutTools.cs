using System;
using UnityEngine;

namespace Bose.Wearable.Editor
{
	/// <summary>
	/// Additional GUILayout methods for use in Inspectors, Drawers, etc....
	/// </summary>
	internal static class GUILayoutTools
	{
		/// <summary>
		/// Draws a single pixel line separator
		/// </summary>
		public static void LineSeparator()
		{
			GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));
		}
	}
}
