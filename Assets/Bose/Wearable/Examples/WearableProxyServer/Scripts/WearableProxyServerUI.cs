using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

namespace Bose.Wearable.Proxy
{
	[RequireComponent(typeof(WearableProxyServer))]
	public class WearableProxyServerUI : MonoBehaviour
	{
		private const string ConnectedClientsMessage = "Clients Connected: {0}";
		private const string ServerStoppedMessage = "Server Stopped";
		private const string HostnamePortMessage = "Host: {0}:{1}";

		private const string DnsAddress = "8.8.8.8";
		private const int DnsPortNumber = 65530;
		private const string LoopbackAddress = "127.0.0.1";

		[SerializeField]
		private Text _connectedClientsText;

		[SerializeField]
		private Text _hostnamePortNumberText;

		private WearableProxyServer _proxyServer;

		private void Awake()
		{
			_proxyServer = GetComponent<WearableProxyServer>();
			_hostnamePortNumberText.text = string.Format(HostnamePortMessage, GetLocalIp(), _proxyServer.PortNumber.ToString());
		}

		public void StartServer()
		{
			_proxyServer.StartServer();
		}

		public void StopServer()
		{
			_proxyServer.StopServer();
		}

		private void Update()
		{
			if (_proxyServer.ServerRunning)
			{
				_connectedClientsText.text = string.Format(ConnectedClientsMessage, _proxyServer.ConnectedClients.ToString());
			}
			else
			{
				_connectedClientsText.text = ServerStoppedMessage;
			}
		}

		private static string GetLocalIp()
		{
			try
			{
				using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
				{
					socket.Connect(DnsAddress, DnsPortNumber);
					IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
					if (endPoint != null)
					{
						return endPoint.Address.ToString();
					}
					else
					{
						return LoopbackAddress;
					}
				}
			}
			catch (SocketException)
			{
				return Dns.GetHostName();
			}
		}
	}
}
