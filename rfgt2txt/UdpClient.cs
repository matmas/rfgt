using System;
using System.Net;
using System.Net.Sockets;

namespace rfgt2txt
{
	public class UdpClient
	{
		public bool connected = false;
		public Socket socket;
		public IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);
		public EndPoint remoteEndPoint;

		public void Connect(int destPort, IPAddress ip)
		{
			remoteEndPoint = new IPEndPoint(ip, destPort);
			socket = new Socket(localEndPoint.Address.AddressFamily,
				SocketType.Dgram,
				ProtocolType.Udp);
			socket.Bind(localEndPoint);
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
			if (socket != null)
				socket.Close();
		}

		~UdpClient()
		{
			Close();
		}
	}
}