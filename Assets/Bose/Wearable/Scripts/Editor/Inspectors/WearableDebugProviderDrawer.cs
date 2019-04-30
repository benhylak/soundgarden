using System;
using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{

	[CustomPropertyDrawer(typeof(WearableDebugProvider))]
	public class WearableDebugProviderDrawer : PropertyDrawer
	{
		private const string DeviceNameField = "_name";
		private const string FirmwareVersionField = "_firmwareVersion";
		private const string RSSIField = "_rssi";
		private const string UIDField = "_uid";
		private const string ProductIdField = "_productId";
		private const string VariantIdField = "_variantId";
		private const string VerboseField = "_verbose";
		private const string SimulateEnabledField = "_simulateMovement";
		private const string RotationTypeField = "_rotationType";
		private const string EulerSpinRateField = "_eulerSpinRate";
		private const string AxisAngleSpinRateField = "_axisAngleSpinRate";

		private const string RotationTypeEuler = "Euler";
		private const string RotationTypeAxisAngle = "AxisAngle";

		private const string DescriptionBox =
			"Provides a minimal data provider that allows connection to a virtual device, and " +
			"logs messages when provider methods are called. If Simulate Movement is enabled, data " +
			"will be generated for all enabled sensors.";
		private const string GesturesLabel = "Simulate Gesture";
		private const string DisconnectLabel = "Simulate Device Disconnect";
		private const string EulerRateBox =
			"Simulates device rotation by changing each Euler angle (pitch, yaw, roll) at a fixed rate in degrees per second.";
		private const string AxisAngleBox =
			"Simulates rotation around a fixed world-space axis at a specified rate in degrees per second.";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			WearableDebugProvider provider = (WearableDebugProvider)fieldInfo.GetValue(property.serializedObject.targetObject);

			EditorGUI.BeginProperty(position, label, property);

			EditorGUILayout.HelpBox(DescriptionBox, MessageType.None);
			EditorGUILayout.Space();

			// Virtual device config
			EditorGUILayout.PropertyField(property.FindPropertyRelative(DeviceNameField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(FirmwareVersionField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(RSSIField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(UIDField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(ProductIdField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(VariantIdField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(VerboseField), WearableConstants.EmptyLayoutOptions);

			// Movement simulation
			SerializedProperty simulateMovementProperty = property.FindPropertyRelative(SimulateEnabledField);
			EditorGUILayout.PropertyField(simulateMovementProperty, WearableConstants.EmptyLayoutOptions);
			if (simulateMovementProperty.boolValue)
			{
				SerializedProperty rotationTypeProperty = property.FindPropertyRelative(RotationTypeField);
				EditorGUILayout.PropertyField(rotationTypeProperty, WearableConstants.EmptyLayoutOptions);

				string rotationType = rotationTypeProperty.enumNames[rotationTypeProperty.enumValueIndex];
				if (rotationType == RotationTypeEuler)
				{
					EditorGUILayout.HelpBox(EulerRateBox, MessageType.None);
					EditorGUILayout.PropertyField(property.FindPropertyRelative(EulerSpinRateField), WearableConstants.EmptyLayoutOptions);
				}
				else if (rotationType == RotationTypeAxisAngle)
				{
					EditorGUILayout.HelpBox(AxisAngleBox, MessageType.None);
					SerializedProperty axisAngleProperty = property.FindPropertyRelative(AxisAngleSpinRateField);
					Vector4 previousValue = axisAngleProperty.vector4Value;
					Vector4 newValue = EditorGUILayout.Vector3Field("Axis", new Vector3(previousValue.x, previousValue.y, previousValue.z), WearableConstants.EmptyLayoutOptions);
					if (newValue.sqrMagnitude < float.Epsilon)
					{
						newValue.x = 1.0f;
					}
					newValue.w = EditorGUILayout.FloatField("Rate", previousValue.w, WearableConstants.EmptyLayoutOptions);
					axisAngleProperty.vector4Value = newValue;
				}
			}

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
