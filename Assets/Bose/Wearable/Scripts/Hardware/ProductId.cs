using System;

namespace Bose.Wearable
{
	/// <summary>
	/// The ProductId of a hardware device.
	/// </summary>
	[Serializable]
	public enum ProductId : ushort
	{
		Undefined = 0,
		BoseFrames = 0x402C,
		QuietComfort35Two = 0x4020
	}
}
