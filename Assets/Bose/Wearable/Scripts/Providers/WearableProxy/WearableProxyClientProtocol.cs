using System;
using System.Runtime.InteropServices;

namespace Bose.Wearable.Proxy
{
	/// <summary>
	/// Implements client-side functionality of the WearableProxy Transport Protocol
	/// </summary>
	internal sealed class WearableProxyClientProtocol : WearableProxyProtocolBase
	{
		public event Action KeepAlive;
		public event Action PingQuery;
		public event Action PingResponse;
		public event Action<SensorFrame> NewSensorFrame;
		public event Action<Device[]> DeviceList;
		public event Action<ConnectionState, Device?> ConnectionStatus;
		public event Action<SensorId, bool> SensorStatus;
		public event Action<GestureId, bool> GestureStatus;
		public event Action<SensorUpdateInterval> SensorUpdateIntervalValue;
		public event Action<RotationSensorSource> RotationSourceValue;

		#region Decoding

		/// <summary>
		/// Consume a packet from the buffer if possible, then advance the buffer index.
		/// </summary>
		/// <param name="buffer">Byte buffer to decode</param>
		/// <param name="index">(Ref) Index to read into buffer</param>
		/// <exception cref="WearableProxyProtocolException">Thrown when a packet cannot be decoded and the buffer
		/// must be discarded.</exception>
		/// <exception cref="IndexOutOfRangeException">Thrown when a packet was partially consumed but ran out of
		/// buffer contents.</exception>
		public override void ProcessPacket(byte[] buffer, ref int index)
		{
			PacketTypeCode packetType = DecodePacketType(buffer, ref index);

			switch (packetType)
			{
				case PacketTypeCode.KeepAlive:
				{
					CheckFooter(buffer, ref index);

					if (KeepAlive != null)
					{
						KeepAlive.Invoke();
					}

					break;
				}
				case PacketTypeCode.PingQuery:
				{
					CheckFooter(buffer, ref index);

					if (PingQuery != null)
					{
						PingQuery.Invoke();
					}

					break;
				}
				case PacketTypeCode.PingResponse:
				{
					CheckFooter(buffer, ref index);

					if (PingResponse != null)
					{
						PingResponse.Invoke();
					}

					break;
				}
				case PacketTypeCode.SensorFrame:
				{
					SensorFrame frame = DecodeSensorFrame(buffer, ref index);
					CheckFooter(buffer, ref index);

					if (NewSensorFrame != null)
					{
						NewSensorFrame.Invoke(frame);
					}

					break;
				}
				case PacketTypeCode.DeviceList:
				{
					Device[] devices = DecodeDeviceList(buffer, ref index);
					CheckFooter(buffer, ref index);

					if (DeviceList != null)
					{
						DeviceList.Invoke(devices);
					}

					break;
				}
				case PacketTypeCode.ConnectionStatus:
				{
					Device? device;
					ConnectionState status = DecodeConnectionStatus(buffer, ref index, out device);
					CheckFooter(buffer, ref index);

					if (ConnectionStatus != null)
					{
						ConnectionStatus.Invoke(status, device);
					}

					break;
				}
				case PacketTypeCode.SensorStatus:
				{
					bool enabled;
					SensorId sensor = DecodeSensorStatus(buffer, ref index, out enabled);
					CheckFooter(buffer, ref index);

					if (SensorStatus != null)
					{
						SensorStatus.Invoke(sensor, enabled);
					}

					break;
				}
				case PacketTypeCode.UpdateIntervalValue:
				{
					SensorUpdateInterval rate = DecodeUpdateInterval(buffer, ref index);
					CheckFooter(buffer, ref index);

					if (SensorUpdateIntervalValue != null)
					{
						SensorUpdateIntervalValue.Invoke(rate);
					}

					break;
				}
				case PacketTypeCode.GestureStatus:
				{
					bool enabled;
					GestureId gesture = DecodeGestureStatus(buffer, ref index, out enabled);
					CheckFooter(buffer, ref index);

					if (GestureStatus != null)
					{
						GestureStatus.Invoke(gesture, enabled);
					}

					break;
				}
				case PacketTypeCode.RotationSourceValue:
				{
					RotationSensorSource source = DecodeRotationSource(buffer, ref index);
					CheckFooter(buffer, ref index);

					if (RotationSourceValue != null)
					{
						RotationSourceValue.Invoke(source);
					}

					break;
				}
				case PacketTypeCode.SensorControl:
				case PacketTypeCode.SetRssiFilter:
				case PacketTypeCode.InitiateDeviceSearch:
				case PacketTypeCode.StopDeviceSearch:
				case PacketTypeCode.ConnectToDevice:
				case PacketTypeCode.DisconnectFromDevice:
				case PacketTypeCode.QueryConnectionStatus:
				case PacketTypeCode.QueryUpdateInterval:
				case PacketTypeCode.SetUpdateInterval:
				case PacketTypeCode.QuerySensorStatus:
				case PacketTypeCode.GestureControl:
				case PacketTypeCode.QueryRotationSource:
				case PacketTypeCode.SetRotationSource:
					// This is a known, but contextually-invalid packet type
					throw new WearableProxyProtocolException(WearableConstants.ProxyProviderInvalidPacketError);
				default:
					// This is an unknown or invalid packet type
					throw new WearableProxyProtocolException(WearableConstants.ProxyProviderInvalidPacketError);
			}
		}

		#endregion

		#region Encoding

		public static byte[] EncodeSensorControlAlloc(SensorId sensor, bool enabled)
		{
			byte[] buffer = new byte[_headerSize + Marshal.SizeOf(typeof(SensorControlPacket)) + _footerSize];
			int index = 0;
			EncodeSensorControl(buffer, ref index, sensor, enabled);
			return buffer;
		}

		/// <summary>
		/// Encode a Sensor Control packet into the specified buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="sensor"></param>
		/// <param name="enabled"></param>
		public static void EncodeSensorControl(byte[] buffer, ref int index, SensorId sensor, bool enabled)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.SensorControl);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			SensorControlPacket packet = new SensorControlPacket
			{
				sensorId = (int) sensor,
				enabled = (byte)(enabled ? 1 : 0)
			};
			SerializePacket(buffer, ref index, packet);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		/// <summary>
		/// Encode a Gesture Control packet into the specified buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="gesture"></param>
		/// <param name="enabled"></param>
		public static void EncodeGestureControl(byte[] buffer, ref int index, GestureId gesture, bool enabled)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.GestureControl);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			GestureControlPacket packet = new GestureControlPacket
			{
				gestureId = (int)gesture,
				enabled = (byte)(enabled ? 1 : 0)
			};
			SerializePacket(buffer, ref index, packet);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeRSSIFilterControlAlloc(int threshold)
		{
			byte[] buffer = new byte[_headerSize + Marshal.SizeOf(typeof(RSSIFilterControlPacket)) + _footerSize];
			int index = 0;
			EncodeRSSIFilterControl(buffer, ref index, threshold);
			return buffer;
		}

		/// <summary>
		/// Encode an RSSI Filter packet into the specified buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="threshold"></param>
		public static void EncodeRSSIFilterControl(byte[] buffer, ref int index, int threshold)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.SetRssiFilter);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			RSSIFilterControlPacket packet = new RSSIFilterControlPacket
			{
				threshold = threshold
			};
			SerializePacket(buffer, ref index, packet);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeInitiateDeviceSearchAlloc()
		{
			byte[] buffer = new byte[_headerSize + _footerSize];
			int index = 0;
			EncodeInitiateDeviceSearch(buffer, ref index);
			return buffer;
		}

		/// <summary>
		/// Encode an Initiate Device Search packet into the specified buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		public static void EncodeInitiateDeviceSearch(byte[] buffer, ref int index)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.InitiateDeviceSearch);
			SerializePacket(buffer, ref index, header);

			// No payload

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeStopDeviceSearchAlloc()
		{
			byte[] buffer = new byte[_headerSize + _footerSize];
			int index = 0;
			EncodeStopDeviceSearch(buffer, ref index);
			return buffer;
		}

		/// <summary>
		/// Encode a Stop Device Search packet into the specified buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		public static void EncodeStopDeviceSearch(byte[] buffer, ref int index)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.StopDeviceSearch);
			SerializePacket(buffer, ref index, header);

			// No payload

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeConnectToDeviceAlloc(string uid)
		{
			byte[] buffer = new byte[_headerSize + DeviceUID.Length + _footerSize];
			int index = 0;
			EncodeConnectToDevice(buffer, ref index, uid);
			return buffer;
		}

		/// <summary>
		/// Encode a Connect to Device packet into the specified buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="uid"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void EncodeConnectToDevice(byte[] buffer, ref int index, string uid)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.ConnectToDevice);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			DeviceConnectPacket packet = new DeviceConnectPacket();
			unsafe
			{
				SerializeFixedString(uid, (IntPtr) packet.uid.value, DeviceUID.Length);
			}
			SerializePacket(buffer, ref index, packet);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeDisconnectFromDeviceAlloc()
		{
			byte[] buffer = new byte[_headerSize + _footerSize];
			int index = 0;
			EncodeDisconnectFromDevice(buffer, ref index);
			return buffer;
		}

		/// <summary>
		/// Encode a Disconnect from Device packet into the specified buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		public static void EncodeDisconnectFromDevice(byte[] buffer, ref int index)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.DisconnectFromDevice);
			SerializePacket(buffer, ref index, header);

			// No payload

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeQueryConnectionStatusAlloc()
		{
			byte[] buffer = new byte[_headerSize + _footerSize];
			int index = 0;
			EncodeQueryConnectionStatus(buffer, ref index);
			return buffer;
		}

		/// <summary>
		/// Encode a Query Connection Status packet into the specified buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		public static void EncodeQueryConnectionStatus(byte[] buffer, ref int index)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.QueryConnectionStatus);
			SerializePacket(buffer, ref index, header);

			// No payload

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeQueryUpdateIntervalAlloc()
		{
			byte[] buffer = new byte[_headerSize + _footerSize];
			int index = 0;
			EncodeQueryUpdateInterval(buffer, ref index);
			return buffer;
		}

		/// <summary>
		/// Encode a Query Update Rate packet into the specified buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		public static void EncodeQueryUpdateInterval(byte[] buffer, ref int index)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.QueryUpdateInterval);
			SerializePacket(buffer, ref index, header);

			// No payload

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static void EncodeSetUpdateInterval(byte[] buffer, ref int index, SensorUpdateInterval interval)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.SetUpdateInterval);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			UpdateIntervalPacket packet = EncodeUpdateInterval(interval);
			SerializePacket(buffer, ref index, packet);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static void EncodeQuerySensorStatus(byte[] buffer, ref int index)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.QuerySensorStatus);
			SerializePacket(buffer, ref index, header);

			// No payload

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static void EncodeQueryGestureStatus(byte[] buffer, ref int index)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.QueryGestureStatus);
			SerializePacket(buffer, ref index, header);

			// No payload

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static void EncodeQueryRotationSource(byte[] buffer, ref int index)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.QueryRotationSource);
			SerializePacket(buffer, ref index, header);
			
			// No payload
			
			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static void EncodeSetRotationSource(byte[] buffer, ref int index, RotationSensorSource source)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.SetRotationSource);
			SerializePacket(buffer, ref index, header);
			
			// Encode payload
			RotationSourcePacket packet = EncodeRotationSource(source);
			SerializePacket(buffer, ref index, packet);
			
			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		#endregion

		#region Private

		/// <summary>
		/// Decode a sensor frame from a byte stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private static SensorFrame DecodeSensorFrame(byte[] buffer, ref int index)
		{
			return DeserializePacket<SensorFrame>(buffer, ref index);;
		}

		/// <summary>
		/// Decode a device list from a byte stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private static Device[] DecodeDeviceList(byte[] buffer, ref int index)
		{
			DeviceListPacketHeader header = DeserializePacket<DeviceListPacketHeader>(buffer, ref index);

			Device[] devices = new Device[header.deviceCount];
			for (int i = 0; i < header.deviceCount; i++)
			{
				DeviceInfoPacket deviceInfo = DeserializePacket<DeviceInfoPacket>(buffer, ref index);
				Device device = DecodeDeviceInfo(deviceInfo, ConnectionState.Disconnected);
				device.isConnected = false;
				devices[i] = device;
			}

			return devices;
		}

		/// <summary>
		/// Decode a Connection Status packet from a byte stream. If state is Failed, the device will be null.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="device"></param>
		/// <returns></returns>
		private static ConnectionState DecodeConnectionStatus(byte[] buffer, ref int index, out Device? device)
		{
			ConnectionStatusPacket status = DeserializePacket<ConnectionStatusPacket>(buffer, ref index);
			ConnectionState state = (ConnectionState)status.statusId;
			if (state == ConnectionState.Failed)
			{
				device = null;
				return state;
			}

			device = DecodeDeviceInfo(status.device, state);
			return state;
		}

		/// <summary>
		/// Decode a Sensor Status packet from a byte stream.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="enabled"></param>
		/// <returns></returns>
		private static SensorId DecodeSensorStatus(byte[] buffer, ref int index, out bool enabled)
		{
			SensorStatusPacket status = DeserializePacket<SensorStatusPacket>(buffer, ref index);
			enabled = (status.enabled != 0);
			return (SensorId)status.sensorId;
		}

		/// <summary>
		/// Decode a Gesture Status packet from a byte stream.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="enabled"></param>
		/// <returns></returns>
		private static GestureId DecodeGestureStatus(byte[] buffer, ref int index, out bool enabled)
		{
			GestureStatusPacket status = DeserializePacket<GestureStatusPacket>(buffer, ref index);
			enabled = (status.enabled != 0);
			return (GestureId)status.gestureId;
		}

		#endregion
	}
}
