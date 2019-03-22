using System;

namespace Bose.Wearable.Proxy
{
	/// <summary>
	/// Represents an exception generated while encoding or decoding data using the Wearable Proxy Protocol
	/// </summary>
	public class WearableProxyProtocolException : Exception
	{
		public WearableProxyProtocolException(string message) : base(message)
		{

		}
	}
}
