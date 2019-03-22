using System;

namespace Bose.Wearable
{
	/// <summary>
	/// Represents a specific gesture type with an associated value. The associated values reflect the values for each GestureId in the underlying SDK.
	/// </summary>
	[Serializable]
	public enum GestureId
	{
		None = 0x0,
		DoubleTap = 0x81,
		HeadNod = 0x82,
		HeadShake = 0x83
	}
}
