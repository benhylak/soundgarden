using UnityEngine;
using UnityEngine.UI;

namespace Bose.Wearable
{
	/// <summary>
	/// Displays a device info as a clickable button for a user to select.
	/// </summary>
	public class WearableDeviceDisplayButton : MonoBehaviour
	{
		/// <summary>
		/// The label text for this device.
		/// </summary>
		[SerializeField]
		private Text _labelText;

		/// <summary>
		/// The label text for this device.
		/// </summary>
		[SerializeField]
		private Text _labelRSSI;

		/// <summary>
		/// The button on this display.
		/// </summary>
		[SerializeField]
		private Button _button;

		public Device device
		{
			get { return _device; }
		}

		private Device _device;

		/// <summary>
		/// Set local components and add any listeners
		/// </summary>
		private void Awake()
		{
			_button.onClick.AddListener(OnClick);
		}

		/// <summary>
		/// When the device display is clicked, pass the device up to the SelectionController.
		/// </summary>
		private void OnClick()
		{
			var selectionController = GetComponentInParent<ISelectionController<Device>>();
			if (selectionController == null)
			{
				return;
			}

			selectionController.OnSelect(_device);
		}

		/// <summary>
		/// Remove all listeners.
		/// </summary>
		private void OnDestroy()
		{
			_button.onClick.RemoveAllListeners();
		}

		/// <summary>
		/// Set the device on this display.
		/// </summary>
		/// <param name="device"></param>
		public void Set(Device device)
		{
			_device = device;
			_labelText.text = string.Format("{0}", _device.name);
			_labelRSSI.text = string.Format("{0}", _device.rssi);
		}
	}
}
