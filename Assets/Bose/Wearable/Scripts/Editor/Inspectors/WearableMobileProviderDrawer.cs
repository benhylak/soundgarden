using System;
using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomPropertyDrawer(typeof(WearableMobileProvider))]
	public class WearableMobileProviderDrawer : PropertyDrawer
	{
		private const string DescriptionBox =
			"A provider that simulates a Wearable Device using the IMU built into a mobile device. In addition " +
			"to on-device builds, this also works in the editor via the Unity Remote app for easier prototyping. " +
			"All coördinates are relative to the same frame and have the same units as the Wearable Device.";
		private const string DisconnectLabel = "Simulate Device Disconnect";
		private const string GesturesLabel = "Simulate Gesture";
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			WearableMobileProvider provider = (WearableMobileProvider)fieldInfo.GetValue(property.serializedObject.targetObject);
			
			EditorGUI.BeginProperty(position, label, property);
				
			EditorGUILayout.HelpBox(DescriptionBox, MessageType.None);
			EditorGUILayout.Space();
		
			// Gesture triggers
			GUILayout.Label(GesturesLabel, WearableConstants.EmptyLayoutOptions);
			for (int i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				GestureId gesture = WearableConstants.GestureIds[i];
				
				if (gesture == GestureId.None)
				{
					continue;
				}

				using (new EditorGUI.DisabledScope(!(provider.GetGestureEnabled(gesture) && EditorApplication.isPlaying)))
				{
					bool shouldTrigger = GUILayout.Button(Enum.GetName(typeof(GestureId), gesture), WearableConstants.EmptyLayoutOptions);
					if (shouldTrigger)
					{
						provider.SimulateGesture(gesture);
					}
				}
			}
			
			// Disconnect button
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
			{
				bool shouldDisconnect = GUILayout.Button(DisconnectLabel, WearableConstants.EmptyLayoutOptions);
				if (shouldDisconnect)
				{
					provider.SimulateDisconnect();
				}
			}
			
			EditorGUI.EndProperty();
			
			
		}
	}
}
