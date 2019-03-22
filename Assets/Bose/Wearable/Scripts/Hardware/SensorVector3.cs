using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Bose.Wearable
{
	[StructLayout(LayoutKind.Sequential), Serializable]
	public struct SensorVector3
	{
		public Vector3 value;
		public SensorAccuracy accuracy;

		public static implicit operator Vector3(SensorVector3 x)
		{
			return x.value;
		}
	}
}
