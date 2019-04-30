using System;
using System.Runtime.InteropServices;

namespace Bose.Wearable
{
	/// <summary>
	/// Represents an Wearable device.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Device : IEquatable<Device>
	{
		/// <summary>
		/// The Unique Identifier for this device. On Android this will be the device's address, since
		/// we cannot get the uuid of the device in some circumstances.
		/// </summary>
		public string uid;

		/// <summary>
		/// The name of this device.
		/// </summary>
		public string name;

		/// <summary>
		/// The firmware version of the device.
		/// </summary>
		public string firmwareVersion;

		/// <summary>
		/// The connection state of this device.
		/// </summary>
		public bool isConnected;

		/// <summary>
		/// The RSSI of the device at the time it was first located.
		/// NB: this value is not updated after searching is stopped.
		/// </summary>
		public int rssi;

		/// <summary>
		/// The ProductId of the device.
		/// </summary>
		internal ProductId productId;

		/// <summary>
		/// The VariantId of the device.
		/// </summary>
		internal byte variantId;

		/// <summary>
		/// Returns the <see cref="ProductType"/> of the device.
		/// </summary>
		/// <returns></returns>
		public ProductType GetProductType()
		{
			return WearableTools.GetProductType(productId);
		}

		/// <summary>
		/// Returns the Product <see cref="VariantType"/> of the device.
		/// </summary>
		/// <returns></returns>
		public VariantType GetVariantType()
		{
			return WearableTools.GetVariantType(GetProductType(), variantId);
		}

		#region IEquatable<Device>

		public bool Equals(Device other)
		{
			return uid == other.uid;
		}

		public static bool operator ==(Device lhs, Device rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Device lhs, Device rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Device && Equals((Device) obj);
		}

		public override int GetHashCode()
		{
			return (uid != null ? uid.GetHashCode() : 0);
		}

		#endregion
	}
}
