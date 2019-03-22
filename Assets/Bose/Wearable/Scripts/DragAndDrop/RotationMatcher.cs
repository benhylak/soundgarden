using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// Automatically rotates a GameObject to match the orientation of the Prototype Glasses.
	/// Provides both "absolute" and "relative" rotation modes.
	/// The <see cref="RotationSensor"/> must be started for the component to have any effect.
	/// </summary>
	[AddComponentMenu("Bose/Wearable/RotationMatcher")]
	public class RotationMatcher : MonoBehaviour
	{
		public enum RotationReference
		{
			/// <summary>
			/// In absolute mode, pointing the Prototype Glasses north will orient the object's forward vector in
			/// the +Z direction.
			/// </summary>
			Absolute,

			/// <summary>
			/// In relative mode, the object is rotated with regards to a fixed reference orientation. Pointing in the
			/// direction of the reference orientation will orient the object's forward vector in the +Z direction.
			/// </summary>
			Relative
		}

		/// <summary>
		/// The reference mode to use when rotating the object. See <see cref="RotationReference"/> for descriptions
		/// of each rotation mode.
		/// </summary>
		public RotationReference ReferenceMode
		{
			get { return _mode; }
			private set { _mode = value; }
		}

		/// <summary>
		/// Get or set the reference rotation. Returns <code>Quaternion.identity</code> if in "absolute" mode,
		/// and the previously-set reference if in "relative" mode. Setting a reference rotation will automatically
		/// switch to "relative" mode.
		/// </summary>
		public Quaternion ReferenceRotation
		{
			get
			{
				if (_mode == RotationReference.Absolute)
				{
					return Quaternion.identity;
				}
				else
				{
					return Quaternion.Inverse(_inverseReference);
				}
			}
		}

		private WearableControl _wearableControl;
		private RotationReference _mode;
		private Quaternion _inverseReference;

		private void Awake()
		{
			// Begin in absolute mode and cache the wearable controller.
			_wearableControl = WearableControl.Instance;
			_mode = RotationReference.Absolute;
			_inverseReference = Quaternion.identity;

			// Establish a requirement for the rotation sensor
			WearableRequirement requirement = GetComponent<WearableRequirement>();
			if (requirement == null)
			{
				requirement = gameObject.AddComponent<WearableRequirement>();
			}
			requirement.EnableSensor(SensorId.Rotation);
			requirement.SetSensorUpdateInterval(SensorUpdateInterval.EightyMs);
		}

		private void Update()
		{
			if (_wearableControl.ConnectedDevice == null)
			{
				return;
			}

			// Get a frame of sensor data. Since no integration is being performed, we can safely ignore all
			// intermediate frames and just grab the most recent.
			SensorFrame frame = _wearableControl.LastSensorFrame;

			if (_mode == RotationReference.Absolute)
			{
				// In absolute mode, match the rotation exactly.
				transform.rotation = frame.rotation;
			}
			else if (_mode == RotationReference.Relative)
			{
				// In relative mode, left-apply the inverse of the reference rotation to compute the relative change
				transform.rotation = _inverseReference * frame.rotation;
			}
		}

		/// <summary>
		/// Set rotation to always use the rotation from the latest <see cref="SensorFrame"/> when matching the
		/// rotation.
		/// </summary>
		public void SetAbsoluteReference()
		{
			ReferenceMode = RotationReference.Absolute;
		}

		/// <summary>
		/// Set the reference to the device's current orientation.
		/// </summary>
		public void SetRelativeReference()
		{
			ReferenceMode = RotationReference.Relative;

			if (_wearableControl.ConnectedDevice != null)
			{
				_inverseReference = Quaternion.Inverse(_wearableControl.LastSensorFrame.rotation);
			}
		}

		/// <summary>
		/// Set the <see cref="Quaternion"/> <paramref name="rotation"/> as a reference when matching the rotation.
		/// </summary>
		/// <param name="rotation"></param>
		public void SetRelativeReference(Quaternion rotation)
		{
			ReferenceMode = RotationReference.Relative;
			_inverseReference = Quaternion.Inverse(rotation);
		}
	}
}
