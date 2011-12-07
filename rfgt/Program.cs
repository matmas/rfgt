using System;
using System.Collections;
using System.Text;

namespace rfgt
{
	class Program
	{
		static ServerList serverList = new ServerList();

		static bool ComparePacket(byte[] buffer, int length, byte[] data)
		{
			for (int i = 0; i < length; i++)
			{
				if (buffer[i] != data[i])
					return false;
			}
			return true;
		}

		static string GetIPAdress(string client)
		{
			return client.Split(':')[0];
		}

		static string GetPort(string client)
		{
			return client.Split(':')[1];
		}

		static int ComparePacket(byte[] buffer, int length, byte[] dataPart1, byte[] dataPart2)
		{
			if (length != dataPart1.Length + 2 + dataPart2.Length)
				return -1;
			for (int i = 0; i < dataPart1.Length; i++)
			{
				if (buffer[i] != dataPart1[i])
					return -1;
			}
			for (int i = 0; i < dataPart2.Length; i++)
			{
				if (buffer[dataPart1.Length + 2 + i] != dataPart2[i])
					return -1;
			}
			int packetID;
			packetID = buffer[dataPart1.Length + 1];
			packetID <<= 8;
			packetID |= buffer[dataPart1.Length + 0];
			return packetID;
		}

		static string PacketToString(byte[] buffer, int length)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				sb.Append(buffer[i].ToString("X") + " ");
			}
			return sb.ToString();
		}

		public static void PrintWithTimeStamp(string s)
		{
			Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + s);
		}

		static byte High(ushort us)
		{
			return (byte)(us >> 8);
		}

		static byte Low(ushort us)
		{
			return (byte)(us & 0xff);
		}

		static void Main(string[] args)
		{
			try
			{
				bool demo = false;
				foreach (string arg in args)
				{
					if (arg == "--demo")
						demo = true;
				}
				byte ver;
				if (demo)
				{
					ver = 0x02;
				}
				else
				{
					ver = 0x16;
				}
				byte[] registerRequest = { 0x02, 0x06, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00 };
				byte[] unregisterRequest = { 0x02, 0x06, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00 };
				byte[] registrationAcknowledge = { 0x02, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00 };

				byte[] listDownloadRequest = { 0x02, 0x06, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00 };
				byte[] headerPart1 = { 0x02, 0x06, 0x06, 0x00 };
				byte[] headerPart2 =							{ ver, 0x00 };
				byte[] headerPart3 =											{ 0x00, 0x00 };
				byte[] endOfListResponsePart1 = { 0x02, 0x06, 0x07, 0x00 };
				byte[] endOfListResponsePart2 =								{ ver, 0x00, 0x0a, 0x00 };
				byte[] thanksPart1 = { 0x02, 0x06, 0x01, 0x00 };
				byte[] thanksPart2 =							{ ver, 0x00, 0x0a, 0x00, 0x00 };

				byte[] reloadRequest = { 0xaa, 0xaa, 0xaa, 0xaa };

				byte[] data = new Byte[2048];
				int length;

				ushort packetID = 0;
				int tempPacketID;
				int defaultPort;
				if (demo)
				{
					defaultPort = 18474;
				}
				else
				{
					defaultPort = 18444;
				}

				UdpServer udp = new UdpServer(defaultPort);
				Console.WriteLine("Matmas's experimental Red Faction "
					+ (demo ? "Demo " : "")
					+ "Game Tracker");
				Console.WriteLine("Thanks to Mr.H for the idea and some help with decoding rfgt protocol.");
				PrintWithTimeStamp("Listening on port " + defaultPort + "/udp...");

				while (true)
				{
					length = udp.Receive(data);
					if (length == -1)
					{
						continue;
					}

					serverList.Refresh();

					if (ComparePacket(data, length, registerRequest))
					{
						PrintWithTimeStamp("Registering server " + udp.GetClient());
						udp.Send(registrationAcknowledge);
						serverList.Register(udp.GetClient());
					}
					else if (ComparePacket(data, length, unregisterRequest))
					{
						PrintWithTimeStamp("Unregistering server " + udp.GetClient());
						udp.Send(registrationAcknowledge);
						serverList.Unregister(udp.GetClient());
					}
					else if (ComparePacket(data, length, listDownloadRequest))
					{
						PrintWithTimeStamp("Sending serverList to " + udp.GetClient());
						int headerSize = headerPart1.Length + 2 + headerPart2.Length + 5 + headerPart3.Length;
						int remainingServers = serverList.Count();
						int sentServers = 0;
						do
						{
							int serversInPacket = Math.Min(82, remainingServers);
							ushort dataSize = (ushort)(headerSize + serversInPacket * 6);
							byte[] list = new byte[dataSize];
							for (int i = 0; i < headerPart1.Length; i++)
							{
								list[i] = headerPart1[i];
							}
							list[headerPart1.Length + 0] = Low(packetID);
							list[headerPart1.Length + 1] = High(packetID);
							for (int i = 0; i < headerPart2.Length; i++)
							{
								list[headerPart1.Length + 2 + i] = headerPart2[i];
							}
							list[headerPart1.Length + 2 + headerPart2.Length + 0] = Low(dataSize);
							list[headerPart1.Length + 2 + headerPart2.Length + 1] = High(dataSize);
							list[headerPart1.Length + 2 + headerPart2.Length + 2] = (byte)serversInPacket;
							list[headerPart1.Length + 2 + headerPart2.Length + 3] = Low((ushort)serverList.Count());
							list[headerPart1.Length + 2 + headerPart2.Length + 4] = High((ushort)serverList.Count());
							for (int i = 0; i < headerPart3.Length; i++)
							{
								list[headerPart1.Length + 2 + headerPart2.Length + 5 + i] = headerPart3[i];
							}
							for (int i = 0; i < serversInPacket; i++)
							{
								string ip = GetIPAdress(serverList[sentServers + i]);
								string port = GetPort(serverList[sentServers + i]);
								string[] ipParts = ip.Split('.');
								for (int j = 0; j < 4; j++)
									list[headerSize + i * 6 + j] = byte.Parse(ipParts[j]);
								list[headerSize + i * 6 + 4] = High(ushort.Parse(port));
								list[headerSize + i * 6 + 5] = Low(ushort.Parse(port));
							}
							udp.Send(list);
							remainingServers -= serversInPacket;
							sentServers += serversInPacket;
							packetID++;
						} while (remainingServers > 0);
						byte[] endOfListResponse = new byte[endOfListResponsePart1.Length + 2 + endOfListResponsePart2.Length];
						for (int i = 0; i < endOfListResponsePart1.Length; i++)
						{
							endOfListResponse[i] = endOfListResponsePart1[i];
						}
						endOfListResponse[endOfListResponsePart1.Length + 0] = Low(packetID);
						endOfListResponse[endOfListResponsePart1.Length + 1] = High(packetID++);
						for (int i = 0; i < endOfListResponsePart2.Length; i++)
						{
							endOfListResponse[endOfListResponsePart1.Length + 2 + i] = endOfListResponsePart2[i];
						}
						udp.Send(endOfListResponse);
					}
					else if ((tempPacketID = ComparePacket(data, length, thanksPart1, thanksPart2)) != -1)
					{
					}
					else if (ComparePacket(data, length, reloadRequest))
					{
						PrintWithTimeStamp("Received reload request from " + udp.GetClient());
						serverList.Reload();
					}
					else
					{
						PrintWithTimeStamp("Received unrecognized packet from " + udp.GetClient() + ": " + PacketToString(data, length));
					}
				}
			}
			catch (Exception e)
			{
				PrintWithTimeStamp("error: " + e.Message);
				PrintWithTimeStamp(e.StackTrace);
			}
		}
	}
}
