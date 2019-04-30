using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomPropertyDrawer(typeof(WearableDeviceConfig))]
	public sealed class WearableDeviceConfigDrawer : PropertyDrawer
	{
		private const string SensorUpdateIntervalPropertyName = "updateInterval";
		private const string RotationSensorSourcePropertyName = "rotationSource";
		private const string AccelerometerConfigPropertyName = "accelerometer";
		private const string GyroscopeConfigPropertyName = "gyroscope";
		private const string RotationConfigPropertyName = "rotation";
		private const string DoubleTapPropertyName = "doubleTap";
		private const string HeadNodPropertyName = "headNod";
		private const string HeadShakePropertyName = "headShake";

		private const string EnabledPropertName = "isEnabled";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var titleRect = new Rect(
				position.x,
				position.y,
				position.width,
				WearableConstants.SingleLineHeight);
			GUI.Label(titleRect, "Sensors", EditorStyles.boldLabel);

			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var sensorUpdateIntervalRect = new Rect(
				position.x,
				titleRect.y + titleRect.height,
				position.width,
				WearableConstants.SingleLineHeight);

			EditorGUI.BeginDisabledGroup(HasAnySensorsEnabled(property));
			var sensorUpdateProp = property.FindPropertyRelative(SensorUpdateIntervalPropertyName);
			EditorGUI.PropertyField(sensorUpdateIntervalRect, sensorUpdateProp);
			EditorGUI.EndDisabledGroup();

			var rotSensorSourceRect = new Rect(
				position.x,
				sensorUpdateIntervalRect.y + sensorUpdateIntervalRect.height,
				position.width,
				WearableConstants.SingleLineHeight);

			var rotationModeProp = property.FindPropertyRelative(RotationSensorSourcePropertyName);
			EditorGUI.PropertyField(rotSensorSourceRect, rotationModeProp);

			var accelProp = property.FindPropertyRelative(AccelerometerConfigPropertyName);
			var accelRect = new Rect(
				position.x,
				rotSensorSourceRect.y + WearableConstants.SingleLineHeight,
				position.width,
				EditorGUI.GetPropertyHeight(accelProp));
			EditorGUI.PropertyField(accelRect, accelProp);

			var gyroProp = property.FindPropertyRelative(GyroscopeConfigPropertyName);
			var gyroRect = new Rect(
				position.x,
				accelRect.y + accelRect.height,
				position.width,
				EditorGUI.GetPropertyHeight(gyroProp));
			EditorGUI.PropertyField(gyroRect, gyroProp);

			var rotProp = property.FindPropertyRelative(RotationConfigPropertyName);
			var rotRect = new Rect(
				position.x,
				gyroRect.y + gyroRect.height,
				position.width,
				EditorGUI.GetPropertyHeight(rotProp));
			EditorGUI.PropertyField(rotRect, rotProp);

			var gesturesLabelRect = new Rect(
				position.x,
				rotRect.y + rotRect.height,
				position.width,
				WearableConstants.SingleLineHeight);
			EditorGUI.LabelField(gesturesLabelRect, "Gestures", EditorStyles.boldLabel);

			var doubleTapProp = property.FindPropertyRelative(DoubleTapPropertyName);
			var doubleTapRect = new Rect(
				position.x,
				rotRect.y + rotRect.height + WearableConstants.SingleLineHeight,
				position.width,
				EditorGUI.GetPropertyHeight(doubleTapProp));
			EditorGUI.PropertyField(doubleTapRect, doubleTapProp);

			var headNodProp = property.FindPropertyRelative(HeadNodPropertyName);
			var headNodRect = new Rect(
				position.x,
				doubleTapRect.y + doubleTapRect.height,
				position.width,
				EditorGUI.GetPropertyHeight(headNodProp));
			EditorGUI.PropertyField(headNodRect, headNodProp);

			var headShakeProp = property.FindPropertyRelative(HeadShakePropertyName);
			var headShakeRect = new Rect(
				position.x,
				headNodRect.y + headNodRect.height,
				position.width,
				EditorGUI.GetPropertyHeight(headShakeProp));
			EditorGUI.PropertyField(headShakeRect, headShakeProp);

			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var newProps = new[]
			{
				property.FindPropertyRelative(SensorUpdateIntervalPropertyName),
				property.FindPropertyRelative(RotationSensorSourcePropertyName),
				property.FindPropertyRelative(AccelerometerConfigPropertyName),
				property.FindPropertyRelative(GyroscopeConfigPropertyName),
				property.FindPropertyRelative(RotationConfigPropertyName),
				property.FindPropertyRelative(DoubleTapPropertyName),
				property.FindPropertyRelative(HeadNodPropertyName),
				property.FindPropertyRelative(HeadShakePropertyName)
			};

			var height = WearableConstants.SingleLineHeight * 2;
			for (var i = 0; i < newProps.Length; i++)
			{
				height += EditorGUI.GetPropertyHeight(newProps[i]);
			}

			return height;
		}

		/// <summary>
		/// Returns true if any sensors are enabled.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		private static bool HasAnySensorsEnabled(SerializedProperty property)
		{
			var newProps = new[]
			{
				property.FindPropertyRelative(AccelerometerConfigPropertyName),
				property.FindPropertyRelative(GyroscopeConfigPropertyName),
				property.FindPropertyRelative(RotationConfigPropertyName)
			};

			var numberOfSensorsActive = 0;
			for (var i = 0; i < newProps.Length; i++)
			{
				if (!newProps[i].FindPropertyRelative(EnabledPropertName).boolValue)
				{
					continue;
				}

				numberOfSensorsActive++;
			}

			return numberOfSensorsActive == 0;
		}
	}
}
