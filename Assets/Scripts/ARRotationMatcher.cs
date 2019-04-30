using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// Corrects the headphone rotation RELATIVE to the Camera parent (so it's looking straight ahead with camera)
	///
	/// This is necessary because in AR, the world 0 rotation is entirely arbitrary.
	/// </summary>
	[AddComponentMenu("Bose/Wearable/RotationMatcher")]
	public class ARRotationMatcher : RotationMatcher
	{
	    public Camera _mainCamera;

	    
		// ReSharper disable once RedundantOverriddenMember
		protected override void Awake()
		{
			base.Awake();
			
			_mode = RotationReference.Relative;			
		}
		
		// ReSharper disable once RedundantOverriddenMember
		protected override void Update()
		{
			base.Update();
		}

		/// <summary>
		/// Set the reference to the device's current orientation.
		/// </summary>
		public override void SetRelativeReference()
		{
			ReferenceMode = RotationReference.Relative;

			if(_wearableControl != null)
			{	
				//not a ton of math. the reference to apply the rotation on top of is the main camera's rotation, minus
				//what the sensor is saying rn.
				
				_inverseReference = _mainCamera.transform.rotation * 
				                    Quaternion.Inverse(_wearableControl.LastSensorFrame.rotation);
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
