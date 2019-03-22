namespace Bose.Wearable.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="SensorUpdateInterval"/> enum.
	/// </summary>
	public static class SensorUpdateIntervalExtensions
	{
		/// <summary>
		/// Returns true if <see cref="SensorUpdateInterval"/> <paramref name="interval"/> is slower than
		/// <see cref="SensorUpdateInterval"/> <paramref name="otherInterval"/>.
		/// </summary>
		/// <param name="interval"></param>
		/// <param name="otherInterval"></param>
		/// <returns></returns>
		public static bool IsSlowerThan(this SensorUpdateInterval interval, SensorUpdateInterval otherInterval)
		{
			var intervalSeconds = WearableTools.SensorUpdateIntervalToSeconds(interval);
			var otherIntervalSeconds = WearableTools.SensorUpdateIntervalToSeconds(otherInterval);

			return intervalSeconds > otherIntervalSeconds;
		}
	}
}
