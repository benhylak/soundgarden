using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Scales the advanced demo's uncertainty cone to match the measurement uncertainty returned by the rotation sensor.
	/// Assumes that the attached mesh subtends an angle of 15º at unity scale.
	/// </summary>
	public class ScaleUncertaintyCone : MonoBehaviour
	{
		/// <summary>
		/// The minimum angle to show. Prevents the cone from being too thin to see.
		/// </summary>
		[SerializeField]
		protected float _minAngle;

		/// <summary>
		/// The maximum angle to show. Prevents the cone from clipping through the sphere.
		/// </summary>
		[SerializeField]
		protected float _maxAngle;

		private WearableControl _wearableControl;

		private void Awake()
		{
			_wearableControl = WearableControl.Instance;
		}

		private void Update()
		{
			if (_wearableControl.ConnectedDevice == null)
			{
				return;
			}

			// Since we are not integrating values, it's fine to just take the most recent frame.
			SensorFrame frame = _wearableControl.LastSensorFrame;

			// Clamp the measurement uncertainty to the desired range.
			float angle = Mathf.Clamp(frame.rotation.measurementUncertainty, _minAngle, _maxAngle);

			// The unscaled geometry of the cone subtends an angle of 15 degrees; find a new scale that makes the
			// cone subtend the correct number of degrees without clipping through the sphere.
			float xyScale = Mathf.Tan(angle * Mathf.Deg2Rad) / Mathf.Tan(15.0f * Mathf.Deg2Rad);
			float zScale = Mathf.Cos(angle * Mathf.Deg2Rad);
			transform.localScale = new Vector3(xyScale, xyScale, zScale);

		}
	}
}
