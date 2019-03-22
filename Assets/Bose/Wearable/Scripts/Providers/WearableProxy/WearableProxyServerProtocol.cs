using System;
using System.Runtime.InteropServices;

namespace Bose.Wearable.Proxy
{
	/// <summary>
	/// Implements server-size functionality of the WearableProxy Transport Protocol
	/// </summary>
	internal sealed class WearableProxyServerProtocol : WearableProxyProtocolBase
	{
		public event Action KeepAlive;
		public event Action PingQuery;
		public event Action PingResponse;
		public event Action<SensorId, bool> SensorControl;
		public event Action<GestureId, bool> GestureControl;
		public event Action<int> RSSIFilterValueChange;
		public event Action InitiateDeviceSearch;
		public event Action StopDeviceSearch;
		public event Action<string> ConnectToDevice;
		public event Action DisconnectFromDevice;
		public event Action QueryConnectionStatus;
		public event Action QueryUpdateInterval;
		public event Action<SensorUpdateInterval> SetUpdateInterval;
		public event Action QuerySensorStatus;
		public event Action QueryGestureStatus;
		public event Action QueryRotationSource;
		public event Action<RotationSensorSource> SetRotationSource;

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
				case PacketTypeCode.SensorControl:
				{
					bool enabled;
					SensorId sensor = DecodeSensorControl(buffer, ref index, out enabled);
					CheckFooter(buffer, ref index);

					if (SensorControl != null)
					{
						SensorControl.Invoke(sensor, enabled);
					}

					break;
				}
				case PacketTypeCode.SetRssiFilter:
				{
					int value = DecodeRSSIFilterControlPacket(buffer, ref index);
					CheckFooter(buffer, ref index);

					if (RSSIFilterValueChange != null)
					{
						RSSIFilterValueChange.Invoke(value);
					}

					break;
				}
				case PacketTypeCode.InitiateDeviceSearch:
				{
					CheckFooter(buffer, ref index);

					if (InitiateDeviceSearch != null)
					{
						InitiateDeviceSearch.Invoke();
					}

					break;
				}
				case PacketTypeCode.StopDeviceSearch:
				{
					CheckFooter(buffer, ref index);

					if (StopDeviceSearch != null)
					{
						StopDeviceSearch.Invoke();
					}

					break;
				}
				case PacketTypeCode.ConnectToDevice:
				{
					string uid = DecodeDeviceConnectPacket(buffer, ref index);
					CheckFooter(buffer, ref index);

					if (ConnectToDevice != null)
					{
						ConnectToDevice.Invoke(uid);
					}

					break;
				}
				case PacketTypeCode.DisconnectFromDevice:
				{
					CheckFooter(buffer, ref index);

					if (DisconnectFromDevice != null)
					{
						DisconnectFromDevice.Invoke();
					}

					break;
				}
				case PacketTypeCode.QueryConnectionStatus:
				{
					CheckFooter(buffer, ref index);

					if (QueryConnectionStatus != null)
					{
						QueryConnectionStatus.Invoke();
					}

					break;
				}
				case PacketTypeCode.QueryUpdateInterval:
				{
					CheckFooter(buffer, ref index);

					if (QueryUpdateInterval != null)
					{
						QueryUpdateInterval.Invoke();
					}

					break;
				}
				case PacketTypeCode.SetUpdateInterval:
				{
					SensorUpdateInterval interval = DecodeSetUpdateIntervalPacket(buffer, ref index);
					CheckFooter(buffer, ref index);

					if (SetUpdateInterval != null)
					{
						SetUpdateInterval.Invoke(interval);
					}

					break;
				}
				case PacketTypeCode.QuerySensorStatus:
				{
					CheckFooter(buffer, ref index);

					if (QuerySensorStatus != null)
					{
						QuerySensorStatus.Invoke();
					}

					break;
				}
				case PacketTypeCode.GestureControl:
				{
					bool enabled;
					GestureId gestureId = DecodeGestureControl(buffer, ref index, out enabled);
					CheckFooter(buffer, ref index);

					if (GestureControl != null)
					{
						GestureControl.Invoke(gestureId, enabled);
					}

					break;
				}
				case PacketTypeCode.QueryGestureStatus:
				{
					CheckFooter(buffer, ref index);

					if (QueryGestureStatus != null)
					{
						QueryGestureStatus.Invoke();
					}

					break;
				}
				case PacketTypeCode.QueryRotationSource:
				{
					CheckFooter(buffer, ref index);

					if (QueryRotationSource != null)
					{
						QueryRotationSource.Invoke();
					}

					break;
				}
				case PacketTypeCode.SetRotationSource:
				{
					RotationSensorSource source = DecodeRotationSource(buffer, ref index);
					CheckFooter(buffer, ref index);

					if (SetRotationSource != null)
					{
						SetRotationSource.Invoke(source);
					}

					break;
				}
				case PacketTypeCode.SensorFrame:
				case PacketTypeCode.DeviceList:
				case PacketTypeCode.ConnectionStatus:
				case PacketTypeCode.SensorStatus:
				case PacketTypeCode.UpdateIntervalValue:
				case PacketTypeCode.GestureStatus:
				case PacketTypeCode.RotationSourceValue:
					// Known, but contextually-invalid packet
					throw new WearableProxyProtocolException(WearableConstants.ProxyProviderInvalidPacketError);
				default:
					// Unknown or corrupt packet
					throw new WearableProxyProtocolException(WearableConstants.ProxyProviderInvalidPacketError);
			}
		}

		#endregion

		#region Encoding

		public static byte[] EncodeSensorFrameAlloc(SensorFrame frame)
		{
			byte[] buffer = new byte[_headerSize + Marshal.SizeOf(typeof(SensorFrame)) + _footerSize];
			int index = 0;
			EncodeSensorFrame(buffer, ref index, frame);
			return buffer;
		}

		/// <summary>
		/// Encode a <see cref="SensorFrame"/> into the buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="frame"></param>
		public static void EncodeSensorFrame(byte[] buffer, ref int index, SensorFrame frame)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.SensorFrame);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			SerializePacket(buffer, ref index, frame);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeDeviceListAlloc(Device[] devices)
		{
			byte[] buffer = new byte[
				_headerSize +
				Marshal.SizeOf(typeof(DeviceListPacketHeader)) +
				devices.Length * Marshal.SizeOf(typeof(DeviceInfoPacket)) +
				_footerSize];
			int index = 0;
			EncodeDeviceList(buffer, ref index, devices);
			return buffer;
		}

		/// <summary>
		/// Encode a Device List packet into the buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="devices"></param>
		public static void EncodeDeviceList(byte[] buffer, ref int index, Device[] devices)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.DeviceList);
			SerializePacket(buffer, ref index, header);

			// Encode sub-header
			SerializePacket(buffer, ref index, new DeviceListPacketHeader { deviceCount = devices.Length });

			// Encode payload
			for (int i = 0; i < devices.Length; i++)
			{
				DeviceInfoPacket packet = EncodeDeviceInfo(devices[i]);
				SerializePacket(buffer, ref index, packet);
			}

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeConnectionStatusAlloc(ConnectionState state, Device device)
		{
			byte[] buffer = new byte[_headerSize + Marshal.SizeOf(typeof(ConnectionStatusPacket)) + _footerSize];
			int index = 0;
			EncodeConnectionStatus(buffer, ref index, state, device);
			return buffer;
		}

		/// <summary>
		/// Encode a Connection Status packet into the buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="state"></param>
		/// <param name="device"></param>
		public static void EncodeConnectionStatus(byte[] buffer, ref int index, ConnectionState state, Device device)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.ConnectionStatus);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			ConnectionStatusPacket packet = new ConnectionStatusPacket
			{
				statusId = (int)state,
				device = EncodeDeviceInfo(device)
			};
			SerializePacket(buffer, ref index, packet);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		/// <summary>
		/// Encode a Sensor Status packet into the buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="sensor"></param>
		/// <param name="enabled"></param>
		public static void EncodeSensorStatus(byte[] buffer, ref int index, SensorId sensor, bool enabled)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.SensorStatus);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			SensorStatusPacket packet = new SensorStatusPacket
			{
				sensorId = (int)sensor,
				enabled = (byte)(enabled ? 1 : 0)
			};
			SerializePacket(buffer, ref index, packet);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		/// <summary>
		/// Encode a Gesture Status packet into the buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="gesture"></param>
		/// <param name="enabled"></param>
		public static void EncodeGestureStatus(byte[] buffer, ref int index, GestureId gesture, bool enabled)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.GestureStatus);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			GestureStatusPacket packet = new GestureStatusPacket
			{
				gestureId = (int)gesture,
				enabled = (byte)(enabled ? 1 : 0)
			};
			SerializePacket(buffer, ref index, packet);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static byte[] EncodeUpdateIntervalValueAlloc(SensorUpdateInterval interval)
		{
			byte[] buffer = new byte[_headerSize + Marshal.SizeOf(typeof(UpdateIntervalPacket)) + _footerSize];
			int index = 0;
			EncodeUpdateIntervalValue(buffer, ref index, interval);
			return buffer;
		}

		/// <summary>
		/// Encode an update interval value packet into the buffer
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="interval"></param>
		public static void EncodeUpdateIntervalValue(byte[] buffer, ref int index, SensorUpdateInterval interval)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.UpdateIntervalValue);
			SerializePacket(buffer, ref index, header);

			// Encode payload
			UpdateIntervalPacket packet = EncodeUpdateInterval(interval);
			SerializePacket(buffer, ref index, packet);

			// Encode footer
			SerializePacket(buffer, ref index, _footer);
		}

		public static void EncodeRotationSourceValue(byte[] buffer, ref int index, RotationSensorSource source)
		{
			// Encode header
			PacketHeader header = new PacketHeader(PacketTypeCode.RotationSourceValue);
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
		/// Decode a Sensor Control packet from the byte stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="rate"></param>
		/// <returns></returns>
		private static SensorId DecodeSensorControl(byte[] buffer, ref int index, out bool enabled)
		{
			SensorControlPacket status = DeserializePacket<SensorControlPacket>(buffer, ref index);
			enabled = (status.enabled != 0);
			return (SensorId)status.sensorId;
		}

		/// <summary>
		/// Decode a Gesture Control packet from the byte stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="rate"></param>
		/// <returns></returns>
		private static GestureId DecodeGestureControl(byte[] buffer, ref int index, out bool enabled)
		{
			GestureControlPacket status = DeserializePacket<GestureControlPacket>(buffer, ref index);
			enabled = (status.enabled != 0);
			return (GestureId)status.gestureId;
		}

		/// <summary>
		/// Decode an RSSI Filter Control packet from the byte stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private static int DecodeRSSIFilterControlPacket(byte[] buffer, ref int index)
		{
			RSSIFilterControlPacket value = DeserializePacket<RSSIFilterControlPacket>(buffer, ref index);
			return value.threshold;
		}

		/// <summary>
		/// Decode a Device Connect packet from the byte stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private static string DecodeDeviceConnectPacket(byte[] buffer, ref int index)
		{
			DeviceConnectPacket device = DeserializePacket<DeviceConnectPacket>(buffer, ref index);
			unsafe
			{
				return DeserializeFixedString((IntPtr)device.uid.value, sizeof(DeviceUID));
			}
		}

		private static SensorUpdateInterval DecodeSetUpdateIntervalPacket(byte[] buffer, ref int index)
		{
			return DecodeUpdateInterval(buffer, ref index);
		}
		#endregion
	}
}
