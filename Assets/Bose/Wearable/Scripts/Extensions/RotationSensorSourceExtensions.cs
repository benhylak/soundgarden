namespace Bose.Wearable.Extensions
{
	/// <summary>
	/// Extension methods for the <see cref="RotationSensorSource"/> enum type.
	/// </summary>
	public static class RotationSensorSourceExtensions
	{
		/// <summary>
		/// Returns true if <see cref="RotationSensorSource"/> <paramref name="self"/> is a lower priority
		/// value than <see cref="RotationSensorSource"/> <paramref name="other"/> based on lower rotation
		/// accuracy.
		/// </summary>
		/// <param name="self"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool IsLowerPriority(this RotationSensorSource self, RotationSensorSource other)
		{
			return self == RotationSensorSource.SixDof &&
			       other == RotationSensorSource.NineDof;
		}
	}
}
