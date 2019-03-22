using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Marks the object as do not destroy
	/// </summary>
	public class DontDestroyOnLoad : MonoBehaviour
	{
		private void Awake()
		{
			DontDestroyOnLoad(this);
		}
	}
}
