using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomPropertyDrawer(typeof(WearableDeviceConfig.WearableGestureConfig))]
	public class WearableGestureConfigDrawer : PropertyDrawer
	{
		private const string EnabledPropertyName = "isEnabled";
		private const float BottomPadding = 5f;
		private const float LabelWidth = 100f;
		private const float LabelPadding = 15f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			EditorGUI.BeginProperty(position, label, property);
			GUI.Box(new Rect(
					position.x,
					position.y,
					position.width,
					GetPropertyHeight(property, label) - BottomPadding),
				GUIContent.none);

			GUI.Label(new Rect(
					position.x,
					position.y,
					LabelWidth,
					WearableConstants.SingleLineHeight),
				label,
				EditorStyles.boldLabel);

			var onEnableProp = property.FindPropertyRelative(EnabledPropertyName);
			var onEnableRect = new Rect(
				position.x + LabelWidth + LabelPadding,
				position.y,
				position.width,
				WearableConstants.SingleLineHeight);

			EditorGUI.indentLevel = indent;
			EditorGUI.PropertyField(onEnableRect, onEnableProp, GUIContent.none);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return WearableConstants.SingleLineHeight + BottomPadding;
		}
	}
}
