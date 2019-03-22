using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

namespace Bose.Wearable.Proxy
{
	/// <summary>
	/// Acts as a client for a WearableProxy server, allowing control of a remote device over a network connection.
	/// </summary>
	[Serializable]
	public class WearableProxyProvider : WearableProviderBase
	{

		#region Provider Unique

		public event Action ProxyConnected;
		public event Action ProxyDisconnected;

		public bool IsConnectedToProxy
		{
			get { return _server.Connected; }
		}

		/// <summary>
		/// Connect to a proxy at the specified address and port. Blocks up to the specified network timeout.
		/// </summary>
		/// <param name="hostname"></param>
		/// <param name="port"></param>
		/// <param name="onSuccess">Invoked on successful connection</param>
		/// <param name="onFailure">Invoked on failed connection</param>
		public void Connect(string hostname, int port, Action onSuccess = null, Action<Exception> onFailure = null)
		{
			if (_server.Connected)
			{
				return;
			}

			try
			{
				_server.Connect(hostname, port);
				_server.GetStream().WriteTimeout = (int)(_networkTimeout * 1000);
			}
			catch (Exception exception)
			{
				Debug.LogFormat(WearableConstants.ProxyProviderConnectionFailedWarning, hostname, port.ToString());

				if (onFailure == null)
				{
					throw;
				}
				else
				{
					onFailure.Invoke(exception);
				}
			}

			_hostname = hostname;
			_portNumber = port;

			if (onSuccess != null)
			{
				onSuccess.Invoke();
			}

			if (ProxyConnected != null)
			{
				ProxyConnected.Invoke();
			}
		}

		/// <summary>
		/// Disconnect from the connected server.
		/// </summary>
		public void Disconnect()
		{
			if (!_server.Connected)
			{
				return;
			}

			_server.Close();

			_hostname = string.Empty;
			_portNumber = 0;

			if (ProxyDisconnected != null)
			{
				ProxyDisconnected.Invoke();
			}
		}

		#endregion

		#region Provider API

		internal override void SearchForDevices(Action<Device[]> onDevicesUpdated)
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeInitiateDeviceSearch(_transmitBuffer, ref _transmitIndex);
			SendTransmitBuffer();
			_searchingForDevices = true;
			_onDevicesUpdatedCallback = onDevicesUpdated;
		}

		internal override void StopSearchingForDevices()
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeStopDeviceSearch(_transmitBuffer, ref _transmitIndex);
			SendTransmitBuffer();
			_searchingForDevices = false;
		}

		internal override void ConnectToDevice(Device device, Action onSuccess, Action onFailure)
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeConnectToDevice(_transmitBuffer, ref _transmitIndex, device.uid);
			SendTransmitBuffer();
			_connectingToDevice = true;
			_deviceConnectSuccessCallback = onSuccess;
			_deviceConnectFailureCallback = onFailure;
		}

		internal override void DisconnectFromDevice()
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeDisconnectFromDevice(_transmitBuffer, ref _transmitIndex);
			SendTransmitBuffer();

			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				_gestureStatus[WearableConstants.GestureIds[i]] = false;
			}

			if (_connectedDevice != null)
			{
				// We can immediately disconnect the client without waiting for a response.
				OnDeviceDisconnected(_connectedDevice.Value);
				_connectedDevice = null;
				ResetDeviceStatus();
			}
		}

		internal override SensorUpdateInterval GetSensorUpdateInterval()
		{
			// If we've never received data, there's not much we can do but warn the user, request the data, and return defaults
			if (_sensorUpdateInterval == null)
			{
				Debug.LogWarning(WearableConstants.ProxyProviderNoDataWarning);
				_transmitIndex = 0;
				WearableProxyClientProtocol.EncodeQueryUpdateInterval(_transmitBuffer, ref _transmitIndex);
				SendTransmitBuffer();
				return WearableConstants.DefaultUpdateInterval;
			}
			else
			{
				return _sensorUpdateInterval.Value;
			}
		}

		internal override void SetSensorUpdateInterval(SensorUpdateInterval updateInterval)
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeSetUpdateInterval(_transmitBuffer, ref _transmitIndex, updateInterval);
			SendTransmitBuffer();
		}

		internal override RotationSensorSource GetRotationSource()
		{
			// If we've never received data, there's not much we can do but warn the user, request the data, and return defaults
			if (_rotationSource == null)
			{
				Debug.LogWarning(WearableConstants.ProxyProviderNoDataWarning);
				_transmitIndex = 0;
				WearableProxyClientProtocol.EncodeQueryRotationSource(_transmitBuffer, ref _transmitIndex);
				SendTransmitBuffer();
				return WearableConstants.DefaultRotationSource;
			}
			else
			{
				return _rotationSource.Value;
			}
		}

		internal override void SetRotationSource(RotationSensorSource source)
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeSetRotationSource(_transmitBuffer, ref _transmitIndex, source);
			SendTransmitBuffer();
		}

		internal override void StartSensor(SensorId sensorId)
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeSensorControl(_transmitBuffer, ref _transmitIndex, sensorId, true);
			SendTransmitBuffer();
		}

		internal override void StopSensor(SensorId sensorId)
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeSensorControl(_transmitBuffer, ref _transmitIndex, sensorId, false);
			SendTransmitBuffer();
		}

		internal override bool GetSensorActive(SensorId sensorId)
		{
			// If we've never received data, there's not much we can do but warn the user, request the data, and return defaults
			if (_sensorStatus[sensorId] == null)
			{
				Debug.LogWarning(WearableConstants.ProxyProviderNoDataWarning);
				_transmitIndex = 0;
				WearableProxyClientProtocol.EncodeQuerySensorStatus(_transmitBuffer, ref _transmitIndex);
				SendTransmitBuffer();
				return false;
			}
			else
			{
				return _sensorStatus[sensorId].Value;
			}
		}

		internal override void EnableGesture(GestureId gestureId)
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeGestureControl(_transmitBuffer, ref _transmitIndex, gestureId, true);
			SendTransmitBuffer();
		}

		internal override void DisableGesture(GestureId gestureId)
		{
			_transmitIndex = 0;
			WearableProxyClientProtocol.EncodeGestureControl(_transmitBuffer, ref _transmitIndex, gestureId, false);
			SendTransmitBuffer();
		}

		internal override bool GetGestureEnabled(GestureId gestureId)
		{
			// If we've never received data, there's not much we can do but warn the user, request the data, and return defaults
			if (_gestureStatus[gestureId] == null)
			{
				Debug.LogWarning(WearableConstants.ProxyProviderNoDataWarning);
				_transmitIndex = 0;
				WearableProxyClientProtocol.EncodeQueryGestureStatus(_transmitBuffer, ref _transmitIndex);
				SendTransmitBuffer();
				return false;
			}
			else
			{
				return _gestureStatus[gestureId].Value;
			}
		}


		internal override void OnEnableProvider()
		{
			if (_enabled)
			{
				return;
			}

			if (!string.IsNullOrEmpty(_hostname) && _portNumber != 0)
			{
				// If we were previously connected, try to reconnect, but ignore failures here
				try
				{
					Connect(_hostname, _portNumber);
				}
				catch
				{
					// Suppress errors; this is a convenience feature.
				}
			}

			base.OnEnableProvider();
		}

		internal override void OnDisableProvider()
		{
			if (!_enabled)
			{
				return;
			}

			base.OnDisableProvider();

			// If connected, temporarily disconnect until provider is re-enabled
			if (_server.Connected)
			{
				Disconnect();
			}
		}

		internal override void OnUpdate()
		{
			if (!_enabled)
			{
				return;
			}

			if (!_server.Connected)
			{
				return;
			}

			_currentSensorFrames.Clear();

			// Receive data from the server
			try
			{
				NetworkStream stream = _server.GetStream();
				while (stream.DataAvailable)
				{

					int bufferSpaceRemaining = _receiveBuffer.Length - _receiveIndex;
					if (bufferSpaceRemaining <= 0)
					{
						// Can't fit any more packets or consume more buffer; dump the buffer to free space.
						Debug.LogWarning(WearableConstants.ProxyProviderBufferFullWarning);
						_receiveIndex = 0;
						bufferSpaceRemaining = _receiveBuffer.Length;
					}

					int actualBytesRead = stream.Read(_receiveBuffer, _receiveIndex, bufferSpaceRemaining);
					_receiveIndex += actualBytesRead;

					ProcessReceiveBuffer();
				}
			}
			catch (Exception exception)
			{
				// The server has disconnected, or some other error
				HandleProxyDisconnect(exception);
			}

		}
		#endregion

		#region Private

		[SerializeField]
		private float _networkTimeout;

		[SerializeField]
		private int _portNumber;

		[SerializeField]
		private string _hostname;

		private readonly Dictionary<SensorId, bool?> _sensorStatus;
		private SensorUpdateInterval? _sensorUpdateInterval;
		private RotationSensorSource? _rotationSource;

		// Gestures
		private readonly Dictionary<GestureId, bool?> _gestureStatus;

		private readonly TcpClient _server;

		private readonly WearableProxyClientProtocol _protocol;
		private bool _issuedWarningLastPacket;
		private int _receiveIndex;
		private readonly byte[] _receiveBuffer;
		private int _transmitIndex;
		private readonly byte[] _transmitBuffer;

		private bool _searchingForDevices;
		private Action<Device[]> _onDevicesUpdatedCallback;

		private bool _connectingToDevice;
		private Device _deviceToConnect;
		private Action _deviceConnectFailureCallback;
		private Action _deviceConnectSuccessCallback;

		internal WearableProxyProvider()
		{
			_sensorStatus = new Dictionary<SensorId, bool?>();
			_gestureStatus = new Dictionary<GestureId, bool?>();
			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}
			
				_gestureStatus.Add(WearableConstants.GestureIds[i], false);
			}

			ResetDeviceStatus();

			_networkTimeout = 1.0f;

			_server = new TcpClient();

			_protocol = new WearableProxyClientProtocol();
			_protocol.ConnectionStatus += OnConnectionStatus;
			_protocol.DeviceList += OnDeviceList;
			_protocol.KeepAlive += OnKeepAlive;
			_protocol.SensorStatus += OnSensorStatus;
			_protocol.GestureStatus += OnGestureStatus;
			_protocol.NewSensorFrame += OnNewSensorFrame;
			_protocol.SensorUpdateIntervalValue += OnUpdateIntervalValue;
			_protocol.PingQuery += OnPingQuery;
			_protocol.RotationSourceValue += OnRotationSourceValue;

			_portNumber = 0;
			_hostname = string.Empty;

			_receiveIndex = 0;
			_receiveBuffer = new byte[WearableProxyProtocolBase.SuggestedServerToClientBufferSize];
			_transmitIndex = 0;
			_transmitBuffer = new byte[WearableProxyProtocolBase.SuggestedClientToServerBufferSize];

			_issuedWarningLastPacket = false;
		}

		/// <summary>
		/// Called when a Keep Alive packet is received
		/// </summary>
		private void OnKeepAlive()
		{
			// No-op
		}

		/// <summary>
		/// Called when a Sensor Status packet is received. Updates the internal sensor state.
		/// </summary>
		/// <param name="sensorId"></param>
		/// <param name="enabled"></param>
		private void OnSensorStatus(SensorId sensorId, bool enabled)
		{
			_sensorStatus[sensorId] = enabled;
		}

		/// <summary>
		/// Called when a Gesture Status packet is received. Updates the internal Gesture state.
		/// </summary>
		/// <param name="gestureId"></param>
		/// <param name="enabled"></param>
		private void OnGestureStatus(GestureId gestureId, bool enabled)
		{
			_gestureStatus[gestureId] = enabled;
		}

		/// <summary>
		/// Called when a new Sensor Frame is received. Updates the stored frames.
		/// </summary>
		/// <param name="frame"></param>
		private void OnNewSensorFrame(SensorFrame frame)
		{
			_currentSensorFrames.Add(frame);
			_lastSensorFrame = frame;

			OnSensorsOrGestureUpdated(frame);
		}

		/// <summary>
		/// Called when a Connection Status packet is received. Invokes connection & disconnection events as needed.
		/// </summary>
		/// <param name="state"></param>
		/// <param name="device"></param>
		private void OnConnectionStatus(WearableProxyProtocolBase.ConnectionState state, Device? device)
		{
			switch (state)
			{
				case WearableProxyProtocolBase.ConnectionState.Disconnected:
					if (_connectedDevice != null)
					{
						OnDeviceDisconnected(_connectedDevice.Value);
					}
					_connectedDevice = null;

					ResetDeviceStatus();
					break;

				case WearableProxyProtocolBase.ConnectionState.Connecting:
					if (device != null)
					{
						OnDeviceConnecting(device.Value);
					}
					break;

				case WearableProxyProtocolBase.ConnectionState.Connected:
					if (device != null && (_connectedDevice == null || _connectedDevice != device))
					{
						// If this is a newly connected device, fire off events
						_connectedDevice = device;

						if (_connectingToDevice)
						{
							// Indicates the connection was initiated by this client
							if (_deviceConnectSuccessCallback != null)
							{
								_deviceConnectSuccessCallback.Invoke();
							}
						}

						OnDeviceConnected(device.Value);
					}
					else
					{
						// Otherwise, it's just a query, and doesn't need anything special.
						_connectedDevice = device;
					}

					_connectingToDevice = false;
					break;

				case WearableProxyProtocolBase.ConnectionState.Failed:
					_connectedDevice = null;

					if (_connectingToDevice)
					{
						// Indicates the connection was initiated by this client
						if (_deviceConnectFailureCallback != null)
						{
							_deviceConnectFailureCallback.Invoke();
						}
					}

					_connectingToDevice = false;
					break;

				default:
					Debug.LogWarning(WearableConstants.ProxyProviderInvalidPacketError);
					break;
			}
		}

		/// <summary>
		/// Called when a Device List packet is received. If the client initiated the request, return the list to the user.
		/// </summary>
		/// <param name="devices"></param>
		private void OnDeviceList(Device[] devices)
		{
			if (_searchingForDevices)
			{
				// This is in response to a search
				if (_onDevicesUpdatedCallback != null)
				{
					_onDevicesUpdatedCallback.Invoke(devices);
				}
			}
			else
			{
				// Unsolicited device list indicates that someone else on the network is searching
				// This is safe to ignore.
			}
		}

		private void OnUpdateIntervalValue(SensorUpdateInterval interval)
		{
			_sensorUpdateInterval = interval;
		}

		private void OnRotationSourceValue(RotationSensorSource source)
		{
			_rotationSource = source;
		}

		private void OnPingQuery()
		{
			_transmitIndex = 0;
			WearableProxyProtocolBase.EncodePingResponse(_transmitBuffer, ref _transmitIndex);
			SendTransmitBuffer();
		}

		/// <summary>
		/// Send the accumulated transmit buffer to the connected server.
		/// </summary>
		private void SendTransmitBuffer()
		{
			if (!_server.Connected)
			{
				// If we're not connected, we can't really do anything here. Show a warning and quit.
				Debug.LogWarning(WearableConstants.ProxyProviderNotConnectedWarning);
				return;
			}

			try
			{
				NetworkStream stream = _server.GetStream();
				stream.WriteTimeout = (int) (1000 * _networkTimeout);
				stream.Write(_transmitBuffer, 0, _transmitIndex);
			}
			catch (Exception exception)
			{
				HandleProxyDisconnect(exception);
			}
		}

		/// <summary>
		/// Process all packets in the buffer and delegate to relevant packet events. If a partial packet is left at the
		/// end of the buffer, it is copied back to the front to be processed next time data arrives.
		/// Dumps the buffer if a corrupt or unknown packet is encountered.
		/// </summary>
		private void ProcessReceiveBuffer()
		{

			int packetIndex = 0;
			while (packetIndex < _receiveIndex)
			{
				int packetStart = packetIndex;
				try
				{
					_protocol.ProcessPacket(_receiveBuffer, ref packetIndex);
				}
				catch (WearableProxyProtocolException exception)
				{
					// A packet could not be parsed, which means the whole buffer needs to be thrown away.
					if (!_issuedWarningLastPacket)
					{
						// Only issue warnings if we've previously parsed a packet correctly. This prevents flooding
						// in the case of mismatched versions, etc.
						Debug.LogWarning(exception.ToString());
						_issuedWarningLastPacket = true;
					}

					_receiveIndex = 0;
					return;
				}
				catch (IndexOutOfRangeException)
				{
					// The packet could not be completely decoded, meaning it is likely split across buffers.
					// Copy the fragment to the beginning of the buffer and try again the next time a buffer comes in.
					for (int i = packetStart; i < _receiveBuffer.Length; i++)
					{
						_receiveBuffer[i - packetStart] = _receiveBuffer[i];
					}

					// Position the receive index right after the partial packet
					_receiveIndex = _receiveBuffer.Length - packetStart;
					return;

				}
				_issuedWarningLastPacket = false;
			}

			_receiveIndex = 0;
		}

		/// <summary>
		/// Called when the proxy disconnected because of a socket error. Handles retries and event invocation.
		/// </summary>
		/// <param name="exception"></param>
		private void HandleProxyDisconnect(Exception exception = null)
		{
			// TODO: Automatic reconnection attempts

			if (exception != null)
			{
				Debug.Log(exception.ToString());
			}

			_server.Close();
			if (ProxyDisconnected != null)
			{
				ProxyDisconnected.Invoke();
			}
		}

		/// <summary>
		/// Call when we don't know the status of the device.
		/// </summary>
		private void ResetDeviceStatus()
		{
			_sensorStatus[SensorId.Gyroscope] = null;
			_sensorStatus[SensorId.Accelerometer] = null;
			_sensorStatus[SensorId.Rotation] = null;
			_sensorUpdateInterval = null;
			_rotationSource = null;

			GestureId[] gestures = WearableConstants.GestureIds;
			for (int i = 0; i < gestures.Length; ++i)
			{
				if (gestures[i] != GestureId.None)
				{
					_gestureStatus[gestures[i]] = null;
				}
			}
		}


		#endregion
	}
}
