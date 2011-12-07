using System;
using System.Net;
using System.Net.Sockets;

namespace rfgt
{
	class UdpServer
	{
		public Socket socket;
		public IPEndPoint localEndPoint;
		public EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

		public UdpServer(int sourcePort)
		{
			Listen(sourcePort, IPAddress.Any);
		}

		public UdpServer(int sourcePort, string ip)
		{
			if ( !Listen(sourcePort, IPAddress.Parse(ip)))
				Listen(sourcePort, IPAddress.Any);
		}

		bool Listen(int sourcePort, IPAddress ip)
		{
			localEndPoint = new IPEndPoint(ip, sourcePort);
			socket = new Socket(localEndPoint.Address.AddressFamily,
				SocketType.Dgram,
				ProtocolType.Udp);
			bool success = true;
			try
			{
				socket.Bind(localEndPoint);
			}
			catch
			{
				success = false;
			}
			return success;
		}

		public int Receive(byte[] data)
		{
			int length;
			try
			{
				length = socket.ReceiveFrom(data, 0, data.Length, SocketFlags.None, ref remoteEndPoint);
			}
			catch
			{
				length = -1;
			}
			return length;
		}

		public void Send(byte[] data)
		{
			socket.SendTo(data, 0, data.Length, SocketFlags.None, remoteEndPoint);
		}

		public string GetClient()
		{
			return remoteEndPoint.ToString();
		}

		public void Close()
		{
			socket.Close();
		}

		~UdpServer()
		{
			Close();
		}
	}
}
