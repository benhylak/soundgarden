using System.Collections.Generic;
using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Main logic for the Basic Demo. Configures the <see cref="RotationMatcher"/> based on UI input, and
	/// fades out the reference glasses based on closeness.
	/// </summary>
	public class BasicDemoController : MonoBehaviour
	{
		private WearableControl _wearableControl;
		private RotationMatcher _matcher;

		private void Awake()
		{
			_matcher = GetComponent<RotationMatcher>();

			// Grab an instance of the WearableControl singleton. This is the primary access point to the wearable SDK.
			_wearableControl = WearableControl.Instance;
		}

		/// <summary>
		/// Sets rotation to relative mode using the current orientation.
		/// </summary>
		public void SetRelativeReference()
		{
			_matcher.SetRelativeReference(_wearableControl.LastSensorFrame.rotation);
		}

		/// <summary>
		/// Sets rotation to absolute mode.
		/// </summary>
		public void SetAbsoluteReference()
		{
			_matcher.SetAbsoluteReference();
		}
	}
}
