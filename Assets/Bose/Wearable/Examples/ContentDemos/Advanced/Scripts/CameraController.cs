using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Provides a very simple touch camera controller that orbits around the origin at a fixed distance and elevation.
	/// </summary>
	public class CameraController : MonoBehaviour
	{
		/// <summary>
		/// The distance from the origin.
		/// </summary>
		[SerializeField]
		protected float _distance;

		/// <summary>
		/// Elevation from horizontal in degrees. Positive values look down on the origin from above.
		/// </summary>
		[SerializeField]
		protected float _elevation;

		/// <summary>
		/// Control-Display ratio of the controller. The camera will rotate by this many degrees when swiping completely
		/// across the screen.
		/// </summary>
		[SerializeField]
		protected float _cdRatio;

		private float _azimuth;

		private void Start()
		{
			Input.multiTouchEnabled = true;
			Input.simulateMouseWithTouches = false;
			_azimuth = 0.0f;
		}

		private void OnEnable()
		{
			SingleTouchRecognizer.Instance.TouchMoved += OnTouchMoved;
		}

		private void OnDisable()
		{
			if (SingleTouchRecognizer.Instance != null)
			{
				SingleTouchRecognizer.Instance.TouchMoved -= OnTouchMoved;
			}
		}

		private void OnTouchMoved(Touch touch)
		{
			_azimuth += _cdRatio * touch.deltaPosition.x / Screen.width;
		}

		private void LateUpdate()
		{
			transform.rotation = Quaternion.Euler(_elevation, _azimuth, 0.0f);
			transform.position = -transform.forward * _distance;
		}
	}
}
