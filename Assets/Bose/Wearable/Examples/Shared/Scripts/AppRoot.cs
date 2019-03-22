
using UnityEngine.SceneManagement;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// AppRoot is the common entry point for the app and facilitates start-up behavior.
	/// </summary>
	public class AppRoot : Singleton<AppRoot>
	{
		private void Start()
		{
		}

		private void Update()
		{
			if (WearableControl.Instance.ConnectedDevice.HasValue && SceneManager.GetActiveScene().name != "desales")
			{			
				SceneManager.LoadScene("desales");
			}
		}
	}
}
