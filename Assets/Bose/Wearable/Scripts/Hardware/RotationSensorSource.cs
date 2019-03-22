namespace Bose.Wearable
{
	/// <summary>
	/// When using SixDof, the "Game Rotation" sensor, which uses the gyro and accel, is used to determine the current heading.
	/// When using NineDof, the "Rotation" sensor, which uses the gyro, accel, and megnetometer, is used.
	/// </summary>
	public enum RotationSensorSource
	{
		SixDof,
		NineDof
	}
}
