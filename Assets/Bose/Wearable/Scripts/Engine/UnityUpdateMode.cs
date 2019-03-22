using System;

namespace Bose.Wearable
{
	/// <summary>
	/// The Update method in Unity that Sensor data should be polled and made available at.
	/// </summary>
	[Serializable]
	public enum UnityUpdateMode
	{
		Update,
		FixedUpdate,
		LateUpdate
	}
}
