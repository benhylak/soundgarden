using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Bose.Wearable
{
	[StructLayout(LayoutKind.Sequential), Serializable]
	public struct SensorQuaternion
	{
		public Quaternion value;
		public float measurementUncertainty;

		public Vector3 Forward
		{
			get { return value * Vector3.forward; }
		}

		public Vector3 Up
		{
			get { return value * Vector3.up; }
		}

		public Vector3 Right
		{
			get { return value * Vector3.right; }
		}

		public static implicit operator Quaternion(SensorQuaternion x)
		{
			return x.value;
		}
	}
}
