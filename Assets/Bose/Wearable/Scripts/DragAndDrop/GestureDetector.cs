using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Bose.Wearable
{
	/// <summary>
	/// Automatically fires an event if the selected gesture is detected.
	/// </summary>
	[AddComponentMenu("Bose/Wearable/GestureDetector")]
	public class GestureDetector : MonoBehaviour
	{
		/// <summary>
		/// The gesture that will be detected.
		/// </summary>
		public GestureId Gesture
		{
			get { return _gesture; }
			set
			{
				Assert.IsFalse(value == GestureId.None, string.Format(WearableConstants.NoneIsInvalidGesture, GetType()));

				if (_requirement != null && 
				    _gesture != value &&
				    _gesture != GestureId.None)
				{
					_requirement.DisableGesture(_gesture);
				}

				_gesture = value;

				if (_requirement != null)
				{
					_requirement.EnableGesture(_gesture);
				}
			}
		}

		[SerializeField]
		private GestureId _gesture;

		[SerializeField]
		private UnityEvent _onGestureDetected;

		private WearableControl _wearableControl;
		private WearableRequirement _requirement;

		private void Awake()
		{
			_wearableControl = WearableControl.Instance;
			_wearableControl.GestureDetected += GestureDetected;
			
			// Establish a requirement for the referenced gesture.
			_requirement = gameObject.AddComponent<WearableRequirement>();

			if (_gesture != GestureId.None)
			{
				_requirement.EnableGesture(_gesture);
			}
		}

		private void OnDestroy()
		{
			_wearableControl.GestureDetected -= GestureDetected;
		}

		private void GestureDetected(GestureId gesture)
		{
			if (gesture != _gesture)
			{
				return;
			}

			_onGestureDetected.Invoke();
		}
	}
}
