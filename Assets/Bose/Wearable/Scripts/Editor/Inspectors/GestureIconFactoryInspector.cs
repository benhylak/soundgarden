using Bose.Wearable.Examples;
using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomEditor(typeof(GestureIconFactory))]
	public sealed class GestureIconFactoryInspector : UnityEditor.Editor
	{
		private const string ListPropertyName = "_gestureToIcons";
		private const string GestureIdPropertyName = "gestureId";
		private const string GestureIconPropertyName = "gestureSprite";

		public override void OnInspectorGUI()
		{
			var listProp = serializedObject.FindProperty(ListPropertyName);
			var originalIndentLevel = EditorGUI.indentLevel;

			GUI.changed = false;
			EditorGUI.indentLevel = 0;
			for (var i = 0; i < listProp.arraySize; i++)
			{
				var entryProp = listProp.GetArrayElementAtIndex(i);
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.PropertyField(entryProp.FindPropertyRelative(GestureIdPropertyName));
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.PropertyField(entryProp.FindPropertyRelative(GestureIconPropertyName));
				GUILayoutTools.LineSeparator();
			}
			EditorGUI.indentLevel = originalIndentLevel;

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
