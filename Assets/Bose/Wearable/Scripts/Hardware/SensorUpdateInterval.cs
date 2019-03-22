using System;

namespace Bose.Wearable
{
	/// <summary>
	/// Update interval values that a sensor can be set to. The default values correspond to the default values in the underlying SDK
	/// </summary>
	[Serializable]
	public enum SensorUpdateInterval
	{
		ThreeHundredTwentyMs = 0,
		OneHundredSixtyMs = 1,
		EightyMs = 2,
		FortyMs = 3,
		TwentyMs = 4
	}
}
