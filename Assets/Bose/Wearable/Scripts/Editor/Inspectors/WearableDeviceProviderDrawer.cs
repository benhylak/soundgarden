using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomPropertyDrawer(typeof(WearableDeviceProvider))]
	public class WearableDeviceProviderDrawer : PropertyDrawer
	{
		private const string SimulateHardwareDevicesField = "_simulateHardwareDevices";
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);			
			
			EditorGUILayout.PropertyField(property.FindPropertyRelative(SimulateHardwareDevicesField), WearableConstants.EmptyLayoutOptions);

			EditorGUI.EndProperty();
		}
	}
}
