using System;

namespace Bose.Wearable
{
	/// <summary>
	/// The VariantId of a BoseFrames hardware device.
	/// </summary>
	[Serializable]
	public enum BoseFramesVariantId : byte
	{
		Undefined = 0,
		BoseFramesAlto = 0x01,
		BoseFramesRondo = 0x02
	}
}
