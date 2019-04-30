#if UNITY_EDITOR

using NUnit.Framework;
using UnityEngine;

namespace Bose.Wearable.Proxy.Tests
{
	[TestFixture]
	public class WearableProxyProtocolTests
	{
		private string _lastPacketName;

		/// <summary>
		/// Asserts that struct sizes match those specified by the transport protocol
		/// </summary>
		[Test]
		public void AssertStructSizes()
		{
			WearableProxyProtocolBase.AssertCorrectStructSizes();
		}

		/// <summary>
		/// Test roundtrip encoding -> decoding for client-to-server packets
		/// </summary>
		[Test]
		public void TestClientToServer()
		{
			WearableProxyServerProtocol serverProtocol = new WearableProxyServerProtocol();

			int serverIndex = 0;
			byte[] serverBuffer = new byte[1024];

			// Register callbacks for packet processing
			serverProtocol.KeepAlive += () => { _lastPacketName = "KeepAlive"; };
			serverProtocol.PingQuery += () => { _lastPacketName = "PingQuery"; };
			serverProtocol.PingResponse += () => { _lastPacketName = "PingResponse"; };
			serverProtocol.SensorControl += (id, enabled) =>
			{
				_lastPacketName = "SensorControl";
				Assert.AreEqual(SensorId.Gyroscope, id);
				Assert.AreEqual(enabled, true);
			};
			serverProtocol.GestureControl += (id, enabled) =>
			{
				_lastPacketName = "GestureControl";
				Assert.AreEqual(GestureId.DoubleTap, id);
				Assert.AreEqual(enabled, true);
			};
			serverProtocol.RSSIFilterValueChange += value =>
			{
				_lastPacketName = "SetRSSI";
				Assert.AreEqual(-40, value);
			};
			serverProtocol.InitiateDeviceSearch += () => { _lastPacketName = "StartSearch"; };
			serverProtocol.StopDeviceSearch += () => { _lastPacketName = "StopSearch"; };
			serverProtocol.ConnectToDevice += uid =>
			{
				_lastPacketName = "ConnectToDevice";
				Assert.AreEqual("00000000-0000-0000-0000-000000000000", uid);
			};
			serverProtocol.DisconnectFromDevice += () => { _lastPacketName = "DisconnectFromDevice"; };
			serverProtocol.QueryConnectionStatus += () => { _lastPacketName = "QueryConnection"; };
			serverProtocol.QueryUpdateInterval += () => { _lastPacketName = "QueryUpdateInterval"; };
			serverProtocol.SetUpdateInterval += interval =>
			{
				_lastPacketName = "SetUpdateInterval";
				Assert.AreEqual(SensorUpdateInterval.OneHundredSixtyMs, interval);
			};
			serverProtocol.QuerySensorStatus += () => { _lastPacketName = "QuerySensorStatus"; };
			serverProtocol.QueryGestureStatus += () => { _lastPacketName = "QueryGestureStatus"; };

			serverProtocol.QueryRotationSource += () => { _lastPacketName = "QueryRotationSource"; };
			serverProtocol.SetRotationSource += source =>
			{
				_lastPacketName = "SetRotationSource";
				Assert.AreEqual(RotationSensorSource.NineDof, source);
			};

			// Encode
			WearableProxyProtocolBase.EncodeKeepAlive(serverBuffer, ref serverIndex);
			WearableProxyProtocolBase.EncodePingQuery(serverBuffer, ref serverIndex);
			WearableProxyProtocolBase.EncodePingResponse(serverBuffer, ref serverIndex);
			WearableProxyClientProtocol.EncodeSensorControl(serverBuffer, ref serverIndex, SensorId.Gyroscope, true);
			WearableProxyClientProtocol.EncodeGestureControl(serverBuffer, ref serverIndex, GestureId.DoubleTap, true);
			WearableProxyClientProtocol.EncodeRSSIFilterControl(serverBuffer, ref serverIndex, -40);
			WearableProxyClientProtocol.EncodeInitiateDeviceSearch(serverBuffer, ref serverIndex);
			WearableProxyClientProtocol.EncodeStopDeviceSearch(serverBuffer, ref serverIndex);
			WearableProxyClientProtocol.EncodeConnectToDevice(serverBuffer, ref serverIndex, "00000000-0000-0000-0000-000000000000");
			WearableProxyClientProtocol.EncodeDisconnectFromDevice(serverBuffer, ref serverIndex);
			WearableProxyClientProtocol.EncodeQueryConnectionStatus(serverBuffer, ref serverIndex);
			WearableProxyClientProtocol.EncodeQueryUpdateInterval(serverBuffer, ref serverIndex);
			WearableProxyClientProtocol.EncodeSetUpdateInterval(serverBuffer, ref serverIndex, SensorUpdateInterval.OneHundredSixtyMs);
			WearableProxyClientProtocol.EncodeQuerySensorStatus(serverBuffer, ref serverIndex);
			WearableProxyProtocolBase.EncodeKeepAlive(serverBuffer, ref serverIndex);
			WearableProxyClientProtocol.EncodeQueryRotationSource(serverBuffer, ref serverIndex);
			WearableProxyClientProtocol.EncodeSetRotationSource(serverBuffer, ref serverIndex, RotationSensorSource.NineDof);

			// Decode
			serverIndex = 0;
			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("KeepAlive", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("PingQuery", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("PingResponse", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("SensorControl", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("GestureControl", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("SetRSSI", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("StartSearch", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("StopSearch", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("ConnectToDevice", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("DisconnectFromDevice", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("QueryConnection", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("QueryUpdateInterval", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("SetUpdateInterval", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("QuerySensorStatus", _lastPacketName);

			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("KeepAlive", _lastPacketName);
			
			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("QueryRotationSource", _lastPacketName);
			
			serverProtocol.ProcessPacket(serverBuffer, ref serverIndex);
			Assert.AreEqual("SetRotationSource", _lastPacketName);
		}

		/// <summary>
		/// Test roundtrip encoding -> decoding for server-to-client packets
		/// </summary>
		[Test]
		public void TestServerToClient()
		{
			WearableProxyClientProtocol clientProtocol = new WearableProxyClientProtocol();

			int clientIndex = 0;
			byte[] clientBuffer = new byte[1024];

			// Register callbacks for packet processing
			clientProtocol.KeepAlive += () => { _lastPacketName = "KeepAlive"; };

			clientProtocol.NewSensorFrame += frame =>
			{
				_lastPacketName = "SensorFrame";
				Assert.AreEqual(123.45f, frame.timestamp);
				Assert.AreEqual(0.1f, frame.deltaTime);
				Assert.AreEqual(Vector3.right, frame.acceleration.value);
				Assert.AreEqual(SensorAccuracy.Low, frame.acceleration.accuracy);
				Assert.AreEqual(Vector3.up, frame.angularVelocity.value);
				Assert.AreEqual(SensorAccuracy.High, frame.angularVelocity.accuracy);
				Assert.AreEqual(new Quaternion(1.0f, 2.0f, 3.0f, 4.0f), frame.rotation.value);
				Assert.AreEqual(15.0, frame.rotation.measurementUncertainty);
				Assert.AreEqual(GestureId.DoubleTap, frame.gestureId);
			};
			clientProtocol.DeviceList += devices =>
			{
				_lastPacketName = "DeviceList";
				Assert.IsTrue(devices.Length == 3);
				Assert.IsTrue(devices[0].name == "Product Name");
				Assert.IsTrue(devices[0].productId == ProductId.Undefined);
				Assert.IsTrue(devices[0].firmwareVersion == "0.13.2f");
				Assert.IsTrue(devices[1].name == "Corey's Device");
				Assert.IsTrue(devices[1].productId == ProductId.BoseFrames);
				Assert.IsTrue(devices[1].variantId == (byte)BoseFramesVariantId.BoseFramesAlto);
				Assert.IsTrue(devices[2].name == "Michael's Headphones");
				Assert.IsTrue(devices[2].productId == ProductId.BoseFrames);
				Assert.IsTrue(devices[2].variantId == (byte)BoseFramesVariantId.BoseFramesRondo);
			};
			clientProtocol.ConnectionStatus += (state, device) =>
			{
				_lastPacketName = "ConnectionStatus";
				Assert.IsTrue(state == WearableProxyProtocolBase.ConnectionState.Connected);
				Assert.IsTrue(device != null);
				Assert.IsTrue(device.Value.name == "Product Name");
				Assert.AreEqual(ProductId.BoseFrames, device.Value.productId);
				Assert.AreEqual((byte)BoseFramesVariantId.BoseFramesAlto, device.Value.variantId);
				Assert.IsTrue(device.Value.uid == "00000000-0000-0000-0000-000000000000");
			};
			clientProtocol.SensorStatus += (id, enabled) =>
			{
				_lastPacketName = "SensorStatus";
				Assert.AreEqual(SensorId.Gyroscope, id);
				Assert.AreEqual(true, enabled);
			};
			clientProtocol.GestureStatus += (id, enabled) =>
			{
				_lastPacketName = "GestureStatus";
				Assert.AreEqual(GestureId.DoubleTap, id);
				Assert.AreEqual(true, enabled);
			};
			clientProtocol.SensorUpdateIntervalValue += interval =>
			{
				_lastPacketName = "UpdateIntervalValue";
				Assert.AreEqual(SensorUpdateInterval.FortyMs, interval);
			};
			clientProtocol.RotationSourceValue += source =>
			{
				_lastPacketName = "RotationSourceValue";
				Assert.AreEqual(RotationSensorSource.NineDof, source);
			};

			WearableProxyProtocolBase.EncodeKeepAlive(clientBuffer, ref clientIndex);
			WearableProxyServerProtocol.EncodeSensorFrame(
				clientBuffer, ref clientIndex,
				new SensorFrame
				{
					timestamp = 123.45f,
					deltaTime = 0.1f,
					acceleration = new SensorVector3
					{
						value = Vector3.right,
						accuracy = SensorAccuracy.Low
					},
					angularVelocity = new SensorVector3
					{
						value = Vector3.up,
						accuracy = SensorAccuracy.High
					},
					rotation = new SensorQuaternion
					{
						value = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f),
						measurementUncertainty = 15.0f
					},
					gestureId = GestureId.DoubleTap
				});
			WearableProxyServerProtocol.EncodeDeviceList(
				clientBuffer, ref clientIndex,
				new[]
				{
					new Device
					{
						isConnected = false,
						name = "Product Name",
						firmwareVersion = "0.13.2f",
						productId = ProductId.Undefined,
						variantId = (byte)BoseFramesVariantId.Undefined,
						rssi = -30,
						uid = "00000000-0000-0000-0000-000000000000"
					},
					new Device
					{
						isConnected = false,
						name = "Corey's Device",
						productId = ProductId.BoseFrames,
						variantId = (byte)BoseFramesVariantId.BoseFramesAlto,
						rssi = -40,
						uid = "00000000-0000-0000-0000-000000000000"
					},
					new Device
					{
						isConnected = false,
						name = "Michael's Headphones",
						productId = ProductId.BoseFrames,
						variantId = (byte)BoseFramesVariantId.BoseFramesRondo,
						rssi = -55,
						uid = "00000000-0000-0000-0000-000000000000"
					}
				});
			WearableProxyServerProtocol.EncodeConnectionStatus(
				clientBuffer, ref clientIndex,
				WearableProxyProtocolBase.ConnectionState.Connected,
				new Device
				{
					isConnected = true,
					name = "Product Name",
					productId = ProductId.BoseFrames,
					variantId = (byte)BoseFramesVariantId.BoseFramesAlto,
					rssi = -30,
					uid = "00000000-0000-0000-0000-000000000000"
				});
			WearableProxyServerProtocol.EncodeSensorStatus(
				clientBuffer,
				ref clientIndex,
				SensorId.Gyroscope,
				true);
			WearableProxyServerProtocol.EncodeUpdateIntervalValue(
				clientBuffer,
				ref clientIndex,
				SensorUpdateInterval.FortyMs);
			WearableProxyProtocolBase.EncodeKeepAlive(clientBuffer, ref clientIndex);
			WearableProxyServerProtocol.EncodeRotationSourceValue(
				clientBuffer,
				ref clientIndex,
				RotationSensorSource.NineDof);

			// Decode
			clientIndex = 0;
			clientProtocol.ProcessPacket(clientBuffer, ref clientIndex);
			Assert.AreEqual(_lastPacketName, "KeepAlive");

			clientProtocol.ProcessPacket(clientBuffer, ref clientIndex);
			Assert.AreEqual(_lastPacketName, "SensorFrame");

			clientProtocol.ProcessPacket(clientBuffer, ref clientIndex);
			Assert.AreEqual(_lastPacketName, "DeviceList");

			clientProtocol.ProcessPacket(clientBuffer, ref clientIndex);
			Assert.AreEqual(_lastPacketName, "ConnectionStatus");

			clientProtocol.ProcessPacket(clientBuffer, ref clientIndex);
			Assert.AreEqual(_lastPacketName, "SensorStatus");

			clientProtocol.ProcessPacket(clientBuffer, ref clientIndex);
			Assert.AreEqual(_lastPacketName, "UpdateIntervalValue");

			clientProtocol.ProcessPacket(clientBuffer, ref clientIndex);
			Assert.AreEqual(_lastPacketName, "KeepAlive");
			
			clientProtocol.ProcessPacket(clientBuffer, ref clientIndex);
			Assert.AreEqual(_lastPacketName, "RotationSourceValue");
		}

		/// <summary>
		/// Ensure that encoded packets from both sources match those defined by the protocol.
		/// </summary>
		[Test]
		public void TestPacketEncoding()
		{
			byte targetVersion = 0x05;
			byte[] expectedBuffer = {
				0x00, targetVersion, 0x45, 0x53, 0x4F, 0x42, // [0] Keep-alive
				0x00, targetVersion, 0x45, 0x53, 0x4F, 0x42, // [6] Keep-alive
				0x01, targetVersion,             // [12] Sensor frame
				0x66, 0xE6, 0xF6, 0x42, // timestamp = 123.45f
				0xCD, 0xCC, 0xCC, 0x3D, // delta = 0.1f
				0x00, 0x00, 0x80, 0x3F, // acc.x = 1.0f
				0x00, 0x00, 0x00, 0x00, // acc.y = 0.0f
				0x00, 0x00, 0x00, 0x00, // acc.z = 0.0f
				0x01, 0x00, 0x00, 0x00, // acc.err = Low
				0x00, 0x00, 0x00, 0x00, // ang.x = 0.0f
				0x00, 0x00, 0x80, 0x3F, // ang.y = 1.0f
				0x00, 0x00, 0x00, 0x00, // ang.z = 0.0f
				0x03, 0x00, 0x00, 0x00, // ang.err = High
				0x00, 0x00, 0x80, 0x3F, // rot.x = 1.0f
				0x00, 0x00, 0x00, 0x40, // rot.y = 2.0f
				0x00, 0x00, 0x40, 0x40, // rot.z = 3.0f
				0x00, 0x00, 0x80, 0x40, // rot.w = 4.0f
				0x00, 0x00, 0x70, 0x41, // rot.err = 15.0f
				0x81, 0x00, 0x00, 0x00, // gesture = double tap
				0x45, 0x53, 0x4F, 0x42, // terminator
				0x02, targetVersion,										// [102] Device list
				0x03, 0x00, 0x00, 0x00,                               // [104] 3 entries
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, // [108] UID = "000..."
				0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30,
				0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30,
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
				0x50, 0x72, 0x6F, 0x64, 0x75, 0x63, 0x74, 0x20, // [144] name = "Product Name"
				0x4E, 0x61, 0x6D, 0x65, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x50, 0x72, 0x6F, 0x64, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0xE2, 0xFF, 0xFF, 0xFF,                               // rssi = -30
				0x00, 0x00, 0x00, // [176] product = undef, variant = undef
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, // [179] UID = "000..."
				0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30,
				0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30,
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
				0x43, 0x6F, 0x72, 0x65, 0x79, 0x27, 0x73, 0x20, // [215] name = "Corey's Device"
				0x44, 0x65, 0x76, 0x69, 0x63, 0x65, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x50, 0x72, 0x6F, 0x64, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0xD8, 0xFF, 0xFF, 0xFF, // [247] rssi = -40
				0x2C, 0x40, 0x01, // [251] product = frames, variant = alto
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D, // [254] UID = "000..."
				0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30,
				0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30,
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
				0x4D, 0x69, 0x63, 0x68, 0x61, 0x65, 0x6C, 0x27, // [290] name = "Michael's Headphones"
				0x73, 0x20, 0x48, 0x65, 0x61, 0x64, 0x70, 0x68,
				0x6F, 0x6E, 0x65, 0x73, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x50, 0x72, 0x6F, 0x64, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0xC9, 0xFF, 0xFF, 0xFF, // [322] rssi = -55
				0x2C, 0x40, 0x02, // product = frames, variant = rondo
				0x45, 0x53, 0x4F, 0x42, // terminator
				0x06, targetVersion,                                           // Connection status
				0x02, 0x00, 0x00, 0x00,                               // status = connected
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2d, // UID = "000..."
				0x30, 0x30, 0x30, 0x30, 0x2d, 0x30, 0x30, 0x30, 0x30,
				0x2d, 0x30, 0x30, 0x30, 0x30, 0x2d, 0x30, 0x30, 0x30,
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
				0x50, 0x72, 0x6f, 0x64, 0x75, 0x63, 0x74, 0x20, // name = "Product Name"
				0x4e, 0x61, 0x6d, 0x65, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x50, 0x72, 0x6F, 0x64, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0xE2, 0xFF, 0xFF, 0xFF, // rssi = -30
				0x2C, 0x40, 0x02, // product = frames, variant = rondo
				0x45, 0x53, 0x4F, 0x42, // terminator
				0x07, targetVersion,             // Sensor status
				0x01, 0x00, 0x00, 0x00, // Sensor = gyroscope
				0x01,  // status = enabled
				0x45, 0x53, 0x4F, 0x42, // terminator
				0x70, targetVersion,             // Sensor control
				0x01, 0x00, 0x00, 0x00, // Sensor = gyroscope
				0x00, // status = disabled
				0x45, 0x53, 0x4F, 0x42, // terminator
				0x71, targetVersion,             // Set RSSI value
				0xD8, 0xFF, 0xFF, 0xFF, // threshold = -40
				0x45, 0x53, 0x4F, 0x42, // terminator
				0x72, targetVersion, 0x45, 0x53, 0x4F, 0x42, // initiate device search
				0x73, targetVersion, 0x45, 0x53, 0x4F, 0x42, // stop device search
				0x74, targetVersion,                                           // connect to device
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2d, // UID = "000..."
				0x30, 0x30, 0x30, 0x30, 0x2d, 0x30, 0x30, 0x30, 0x30,
				0x2d, 0x30, 0x30, 0x30, 0x30, 0x2d, 0x30, 0x30, 0x30,
				0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
				0x45, 0x53, 0x4F, 0x42, // terminator
				0x75, targetVersion, 0x45, 0x53, 0x4F, 0x42, // disconnect from device
				0x76, targetVersion, 0x45, 0x53, 0x4F, 0x42, // query device status
				0x77, targetVersion, 0x45, 0x53, 0x4F, 0x42, // query update interval
				0x08, targetVersion, // query interval value
				0x01, 0x00, 0x00, 0x00, // 160 ms
				0x45, 0x53, 0x4F, 0x42, // terminator
				0x78, targetVersion, // set update interval
				0x03, 0x00, 0x00, 0x00, // 40ms
				0x45, 0x53, 0x4F, 0x42, // terminator
				0x79, targetVersion, 0x45, 0x53, 0x4F, 0x42, // query sensor status
				0x7b, targetVersion, 0x45, 0x53, 0x4F, 0x42, // query gesture status
				0x7c, targetVersion, 0x45, 0x53, 0x4F, 0x42, // query rotation source
				0x10, targetVersion, 0x01, 0x00, 0x00, 0x00, 0x45, 0x53, 0x4F, 0x42, // rotation source = ninedof
				0x7d, targetVersion, 0x01, 0x00, 0x00, 0x00, 0x45, 0x53, 0x4F, 0x42, // set rotation source = ninedof
				0xFF, 0xFF, 0xFF, 0xFF, // leftover filler
				0xFF, 0xFF, 0xFF, 0xFF,
				0xFF, 0xFF, 0xFF, 0xFF,
				0xFF, 0xFF, 0xFF, 0xFF
			};

			byte[] testBuffer = new byte[expectedBuffer.Length];
			int writeIndex = 0;

			// Start the buffer off with unused data to ensure it is properly overwritten
			for (int i = 0; i < testBuffer.Length; i++)
			{
				testBuffer[i] = 0xFF;
			}

			// Encode all packet types into the buffer
			WearableProxyProtocolBase.EncodeKeepAlive(testBuffer, ref writeIndex);
			WearableProxyProtocolBase.EncodeKeepAlive(testBuffer, ref writeIndex);
			WearableProxyServerProtocol.EncodeSensorFrame(
				testBuffer, ref writeIndex,
				new SensorFrame
				{
					timestamp = 123.45f,
					deltaTime = 0.1f,
					acceleration = new SensorVector3
					{
						value = Vector3.right,
						accuracy = SensorAccuracy.Low
					},
					angularVelocity = new SensorVector3
					{
						value = Vector3.up,
						accuracy = SensorAccuracy.High
					},
					rotation = new SensorQuaternion
					{
						value = new Quaternion(1.0f, 2.0f, 3.0f, 4.0f),
						measurementUncertainty = 15.0f
					},
					gestureId = GestureId.DoubleTap
				});
			WearableProxyServerProtocol.EncodeDeviceList(
				testBuffer, ref writeIndex,
				new[]
				{
					new Device
					{
						isConnected = false,
						name = "Product Name",
						firmwareVersion = "Prod",
						productId = ProductId.Undefined,
						variantId = (byte)BoseFramesVariantId.Undefined,
						rssi = -30,
						uid = "00000000-0000-0000-0000-000000000000"
					},
					new Device
					{
						isConnected = false,
						name = "Corey's Device",
						firmwareVersion = "Prod",
						productId = ProductId.BoseFrames,
						variantId = (byte)BoseFramesVariantId.BoseFramesAlto,
						rssi = -40,
						uid = "00000000-0000-0000-0000-000000000000"
					},
					new Device
					{
						isConnected = false,
						name = "Michael's Headphones",
						firmwareVersion = "Prod",
						productId = ProductId.BoseFrames,
						variantId = (byte)BoseFramesVariantId.BoseFramesRondo,
						rssi = -55,
						uid = "00000000-0000-0000-0000-000000000000"
					}
				});
			WearableProxyServerProtocol.EncodeConnectionStatus(
				testBuffer, ref writeIndex,
				WearableProxyProtocolBase.ConnectionState.Connected,
				new Device
				{
					isConnected = true,
					name = "Product Name",
					firmwareVersion = "Prod",
					productId = ProductId.BoseFrames,
					variantId = (byte)BoseFramesVariantId.BoseFramesRondo,
					rssi = -30,
					uid = "00000000-0000-0000-0000-000000000000"
				});
			WearableProxyServerProtocol.EncodeSensorStatus(
				testBuffer, ref writeIndex,
				SensorId.Gyroscope, true);
			WearableProxyClientProtocol.EncodeSensorControl(testBuffer, ref writeIndex, SensorId.Gyroscope, false);
			WearableProxyClientProtocol.EncodeRSSIFilterControl(testBuffer, ref writeIndex, -40);
			WearableProxyClientProtocol.EncodeInitiateDeviceSearch(testBuffer, ref writeIndex);
			WearableProxyClientProtocol.EncodeStopDeviceSearch(testBuffer, ref writeIndex);
			WearableProxyClientProtocol.EncodeConnectToDevice(testBuffer, ref writeIndex, "00000000-0000-0000-0000-000000000000");
			WearableProxyClientProtocol.EncodeDisconnectFromDevice(testBuffer, ref writeIndex);
			WearableProxyClientProtocol.EncodeQueryConnectionStatus(testBuffer, ref writeIndex);
			WearableProxyClientProtocol.EncodeQueryUpdateInterval(testBuffer, ref writeIndex);
			WearableProxyServerProtocol.EncodeUpdateIntervalValue(testBuffer, ref writeIndex, SensorUpdateInterval.OneHundredSixtyMs);
			WearableProxyClientProtocol.EncodeSetUpdateInterval(testBuffer, ref writeIndex, SensorUpdateInterval.FortyMs);
			WearableProxyClientProtocol.EncodeQuerySensorStatus(testBuffer, ref writeIndex);
			WearableProxyClientProtocol.EncodeQueryGestureStatus(testBuffer, ref writeIndex);
			WearableProxyClientProtocol.EncodeQueryRotationSource(testBuffer, ref writeIndex);
			WearableProxyServerProtocol.EncodeRotationSourceValue(testBuffer, ref writeIndex, RotationSensorSource.NineDof);
			WearableProxyClientProtocol.EncodeSetRotationSource(testBuffer, ref writeIndex, RotationSensorSource.NineDof);

			// Check buffers match
			Assert.AreEqual(expectedBuffer, testBuffer);
		}
	}
}

#endif
