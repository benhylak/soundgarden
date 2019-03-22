using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Main logic for the Advanced Demo. Begins a calibration routine on start, then spawns a new target on the surface
	/// of the icosphere. Spawns new targets after existing ones are collected.
	/// </summary>
	public class AdvancedDemoController : MonoBehaviour
	{
		/// <summary>
		/// A list of points corresponding to vertices on the icosphere. Does not include points that are difficult to
		/// look at.
		/// </summary>
		private readonly Vector3[] _spawnPoints =
		{
			new Vector3(0.7235999703407288f, -0.4472149908542633f, -0.5257200002670288f),
			new Vector3(-0.27638500928878784f, -0.4472149908542633f, -0.8506399989128113f),
			new Vector3(-0.8944249749183655f, -0.4472149908542633f, 0.0f),
			new Vector3(-0.27638500928878784f, -0.4472149908542633f, 0.8506399989128113f),
			new Vector3(0.7235999703407288f, -0.4472149908542633f, 0.5257200002670288f),
			new Vector3(0.27638500928878784f, 0.4472149908542633f, -0.8506399989128113f),
			new Vector3(-0.7235999703407288f, 0.4472149908542633f, -0.5257200002670288f),
			new Vector3(-0.7235999703407288f, 0.4472149908542633f, 0.5257200002670288f),
			new Vector3(0.27638500928878784f, 0.4472149908542633f, 0.8506399989128113f),
			new Vector3(0.8944249749183655f, 0.4472149908542633f, 0.0f)
		};

		/// <summary>
		/// The prefab to spawn when creating a new target.
		/// </summary>
		[SerializeField]
		protected GameObject _targetPrefab;

		/// <summary>
		/// The rotation matcher component attached to the rotation widget.
		/// </summary>
		[SerializeField]
		protected RotationMatcher _widgetRotationMatcher;

		/// <summary>
		/// The minimum time to wait before calibrating. Gives the user the opportunity to read and comprehend the
		/// calibration message.
		/// </summary>
		[SerializeField]
		protected float _minCalibrationTime;

		/// <summary>
		/// The maximum time to wait while calibrating.
		/// </summary>
		[SerializeField]
		protected float _maxCalibrationTime;

		/// <summary>
		/// The maximum allowable rotational velocity in degrees per second while calibrating. Waits for rotation to
		/// fall beneath this level before calibrating.
		/// </summary>
		[SerializeField]
		protected float _calibrationMotionThreshold;

		/// <summary>
		/// The amount of time to wait between target spawns in seconds.
		/// </summary>
		[SerializeField]
		protected float _spawnDelay;

		/// <summary>
		/// Invoked when calibration is complete.
		/// </summary>
		public event Action CalibrationCompleted;

		private WearableControl _wearableControl;

		private bool _calibrating;
		private float _calibrationStartTime;
		private int _lastSpawnPointIndex;
		private Quaternion _referenceRotation;

		private void Awake()
		{
			// Grab an instance of the WearableControl singleton. This is the primary access point to the wearable SDK.
			_wearableControl = WearableControl.Instance;
		}

		private void Start()
		{
			// Begin calibration immediately.
			StartCalibration();
		}

		/// <summary>
		/// Begin the calibration routine. Waits for <see cref="_minCalibrationTime"/>, then until rotational
		/// velocity falls below <see cref="_calibrationMotionThreshold"/> before sampling the rotation sensor.
		/// Will not calibrate for longer than <see cref="_maxCalibrationTime"/>.
		/// </summary>
		private void StartCalibration()
		{
			_calibrating = true;
			_calibrationStartTime = Time.unscaledTime;
		}

		/// <summary>
		/// Spawns a new target on the surface of the icosphere.
		/// </summary>
		private void SpawnTarget()
		{
			// Randomly select a new spawn point not equal to the previous point.
			_lastSpawnPointIndex = (Random.Range(1, _spawnPoints.Length / 2) + _lastSpawnPointIndex) % _spawnPoints.Length;

			// Create a new target object at that point, parented to the controller.
			GameObject target = Instantiate(_targetPrefab, transform);
			target.transform.position = _spawnPoints[_lastSpawnPointIndex] * 0.5f;

			// Subscribe to the new target's collection event.
			TargetController targetController = target.GetComponent<TargetController>();
			targetController.Collected += OnCollected;

			// Pass on the reference rotation from calibration to the target controller.
			targetController.ReferenceRotation = _referenceRotation;
		}

		/// <summary>
		/// Invoked when a target is collected.
		/// </summary>
		/// <param name="target">The target that was collected</param>
		private void OnCollected(TargetController target)
		{
			// Spawn a new target after a delay.
			Invoke("SpawnTarget", _spawnDelay);
		}

		private void Update()
		{
			if (_calibrating)
			{
				// While calibrating, continuously sample the gyroscope and wait for it to fall below a motion
				// threshold. When that happens, or a timeout is exceeded, grab a sample from the rotation sensor and
				//  use that as the reference rotation.
				SensorFrame frame = _wearableControl.LastSensorFrame;

				bool didWaitEnough = Time.unscaledTime > _calibrationStartTime + _minCalibrationTime;
				bool isStationary = frame.angularVelocity.value.magnitude < _calibrationMotionThreshold;
				bool didTimeout = Time.unscaledTime > _calibrationStartTime + _maxCalibrationTime;

				if ((didWaitEnough && isStationary) || didTimeout)
				{
					_referenceRotation = frame.rotation;
					_calibrating = false;

					// Pass along the reference to the rotation matcher on the widget.
					_widgetRotationMatcher.SetRelativeReference(frame.rotation);

					if (CalibrationCompleted != null)
					{
						CalibrationCompleted.Invoke();
					}

					// Spawn the first target after calibration completes.
					Invoke("SpawnTarget", _spawnDelay);
				}
			}
		}
	}
}
