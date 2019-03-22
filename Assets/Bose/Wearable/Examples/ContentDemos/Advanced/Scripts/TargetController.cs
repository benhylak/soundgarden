using System;
using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Controls a target in the advanced demo. When the Prototype Glasses' orientation is pointing toward the target,
	/// a timer is started; the target is considered "collected" when the timer reaches a set value.
	/// </summary>
	public class TargetController : MonoBehaviour
	{
		/// <summary>
		/// Invoked when the target is collected.
		/// </summary>
		public event Action<TargetController> Collected;

		/// <summary>
		/// The reference orientation to use while processing rotations. See <see cref="RotationMatcher"/>
		/// for more explanation.
		/// </summary>
		public Quaternion ReferenceRotation
		{
			get { return Quaternion.Inverse(_inverseReference); }
			set { _inverseReference = Quaternion.Inverse(value); }
		}

		/// <summary>
		/// A margin in degrees to add to the estimated accuracy.
		/// </summary>
		[SerializeField]
		private float _targetMargin;

		/// <summary>
		/// The maximum distance from the target in degrees that is considered facing the target.
		/// </summary>
		[SerializeField]
		private float _maxTargetWidth;

		/// <summary>
		/// The fill rate in units per second, added to the charge when the target is selected.
		/// A target will be collected in 1/_chargeFillRate seconds.
		/// </summary>
		[SerializeField]
		private float _chargeFillRate;

		/// <summary>
		/// The empty rate in units per second, subtracted from the charge when the target is not selected.
		/// </summary>
		[SerializeField]
		private float _chargeEmptyRate;

		/// <summary>
		/// The animation parameter to set while the trigger is charging.
		/// </summary>
		[SerializeField]
		private string _animationIsChargingParameter;

		/// <summary>
		/// The animation parameter to set when the trigger is fully charged.
		/// </summary>
		[SerializeField]
		private string _animationIsFullyChargedParameter;

		private WearableControl _wearableControl;
		private TargetSFX _sfx;
		private Animator _animator;

		private float _charge;
		private Quaternion _inverseReference;
		private bool _collected;
		private bool _targetLocked;

		private void Awake()
		{
			_wearableControl = WearableControl.Instance;
			_sfx = GetComponent<TargetSFX>();
			_animator = GetComponent<Animator>();
		}

		private void Start()
		{
			_animator.SetBool(_animationIsChargingParameter, false);
			_animator.SetBool(_animationIsFullyChargedParameter, false);
			_collected = false;
			_targetLocked = false;
			
			_sfx.FadeInAudio();
			_sfx.PlaySpawnSting();
		}

		private void Update()
		{
			if (_collected)
			{
				return;
			}

			if (_wearableControl.ConnectedDevice == null)
			{
				return;
			}

			// Get the latest rotation from the device
			SensorFrame frame = _wearableControl.LastSensorFrame;

			// Apply a reference rotation then calculate the relative "forward" vector of the device.
			Vector3 forward = (_inverseReference * frame.rotation) * Vector3.forward;

			// Calculate the direction from the parent to the target
			Vector3 targetDir = transform.localPosition.normalized;

			// Scale the similarity between these two vectors to the range [0, 1] and use this to control the layered audio.
			float closeness = 0.5f + 0.5f * Vector3.Dot(forward, targetDir);
			_sfx.Closeness = closeness;

			if (Vector3.Angle(forward, targetDir) < Mathf.Min(frame.rotation.measurementUncertainty + _targetMargin, _maxTargetWidth))
			{
				// If the glasses are pointing within a margin of the target, fill the charge.
				_charge += _chargeFillRate * Time.deltaTime;

				if (!_targetLocked)
				{
					_sfx.PlayLockSting();
					_targetLocked = true;
				}
				
				_animator.SetBool(_animationIsChargingParameter, true);
			}
			else
			{
				// Otherwise, drain the charge.
				_charge -= _chargeEmptyRate * Time.deltaTime;

				_targetLocked = false;
				
				_animator.SetBool(_animationIsChargingParameter, false);
			}

			// If the charge exceeds 1, "collect" the target. The animator will automatically destroy this target
			// at the end of the destruction animation.
			if (_charge > 1.0f)
			{
				_collected = true;
				if (Collected != null)
				{
					Collected.Invoke(this);
				}
				_animator.SetBool(_animationIsFullyChargedParameter, true);
				_animator.SetBool(_animationIsChargingParameter, false);
				
				_sfx.PlayCollectSting();
				_sfx.FadeOutAudio();
			}

			// Clamp the charge within [0, 1]
			_charge = Mathf.Clamp01(_charge);

			// Scale the target from 0.5 to 1.5 with charge.
			transform.localScale = Vector3.one * (0.5f + _charge);
			
			// Set the fill level based on charge
			_sfx.SetChargeLevel(_charge);
		}

		/// <summary>
		/// Destroys this target. Called at the end of the destruction animation.
		/// </summary>
		public void Destroy()
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}
