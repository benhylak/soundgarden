using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomEditor(typeof(WearableRequirement))]
	public sealed class WearableRequirementInspector : UnityEditor.Editor
	{
		private const string DeviceConfigPropertyName = "_wearableDeviceConfig";

		public override void OnInspectorGUI()
		{
			GUI.changed = false;
			var property = serializedObject.FindProperty(DeviceConfigPropertyName);
			EditorGUILayout.PropertyField(property, WearableConstants.EmptyLayoutOptions);

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();

				if (Application.isPlaying)
				{
					var requirement = (WearableRequirement)target;
					requirement.SetDirty();
				}
			}
		}
	}
}
