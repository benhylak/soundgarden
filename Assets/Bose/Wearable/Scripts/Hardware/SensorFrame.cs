using System;
using System.Runtime.InteropServices;

namespace Bose.Wearable
{
	/// <summary>
	/// Represents a set of updates from all sensors on the Wearable device at a particular point in time. Active sensors
	/// will have updated values while inactive sensor values will remain at their default value. Sensors set to
	/// different update rates will have their latest value carried forward in subsequent updates until their next
	/// update tick.
	/// </summary>
	[StructLayout(LayoutKind.Sequential), Serializable]
	public struct SensorFrame
	{
		public float timestamp;
		public float deltaTime;
		public SensorVector3 acceleration;
		public SensorVector3 angularVelocity;
		public SensorQuaternion rotation;
		public GestureId gestureId;
	}
}
