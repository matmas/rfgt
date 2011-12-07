using System;
using System.Collections;
using System.Text;

namespace rfgt2txt
{
	class Program
	{
		static byte High(ushort us)
		{
			return (byte)(us >> 8);
		}

		static byte Low(ushort us)
		{
			return (byte)(us & 0xff);
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

		static int ComparePacket(byte[] buffer, int length, byte[] dataPart1)
		{
			for (int i = 0; i < dataPart1.Length; i++)
			{
				if (buffer[i] != dataPart1[i])
					return -1;
			}
			int packetID;
			packetID = buffer[dataPart1.Length + 1];
			packetID <<= 8;
			packetID |= buffer[dataPart1.Length + 0];
			return packetID;
		}

		static byte[] GetThanks(ushort packetID, ushort ver)
		{
			byte[] response = new byte[thanksPart1.Length + 2 + thanksPart2.Length];
			for (int i = 0; i < thanksPart1.Length; i++)
				response[i] = thanksPart1[i];
			response[thanksPart1.Length + 0] = Low(packetID);
			response[thanksPart1.Length + 1] = High(packetID);
			for (int i = 0; i < thanksPart2.Length; i++)
				response[thanksPart1.Length + 2 + i] = thanksPart2[i];
			response[thanksPart1.Length + 2 + 0] = Low(ver);
			response[thanksPart1.Length + 2 + 1] = High(ver);
			return response;
		}

		static byte ver = 0xFF;
		static byte[] listDownloadRequest = { 0x02, 0x06, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00 };
		static byte[] headerPart1 = { 0x02, 0x06, 0x06, 0x00 };
		static byte[] headerPart2 =							{ ver, 0x00 };
		static byte[] headerPart3 =											{ 0x00, 0x00 };
		static byte[] endOfListResponsePart1 = { 0x02, 0x06, 0x07, 0x00 };
		static byte[] endOfListResponsePart2 =								{ ver, 0x00, 0x0a, 0x00 };
		static byte[] thanksPart1 = { 0x02, 0x06, 0x01, 0x00 };
		static byte[] thanksPart2 =							{ ver, 0x00, 0x0a, 0x00, 0x00 };

		static int Main(string[] args)
		{
			try
			{
				if (args.Length != 1)
				{
					Console.WriteLine("Matmas's experimental rfgt2txt (Red Faction Game Tracker Downloader)");
					Console.WriteLine("Thanks to Mr.H for the idea and some help with decoding rfgt protocol.");
					Console.WriteLine("usage: rfgt2txt.exe <host>:<port>");
					return 1;
				}

				UdpClient udp = new UdpClient();
				udp.Connect(int.Parse(args[0].Split(':')[1]), System.Net.Dns.GetHostAddresses(args[0].Split(':')[0])[0]);

				udp.Send(listDownloadRequest);

				byte[] data = new Byte[2048];
				int length;

				ArrayList serverList = new ArrayList();

				while (true)
				{
					length = udp.Receive(data);

					int packetID;
					if ((packetID = ComparePacket(data, length, headerPart1)) != -1)
					{
						ushort ver = (ushort)((int)data[headerPart1.Length + 2 + 1] << 8 | (int)data[headerPart1.Length + 2 + 0]);
						udp.Send(GetThanks((ushort)packetID, ver));
						int headerSize = headerPart1.Length + 2 + headerPart2.Length + 5 + headerPart3.Length;
						for (int i = 0; i < (length - headerSize) / 6; i++)
						{
							string server = "";
							for (int j = 0; j < 4; j++)
							{
								server += data[headerSize + i * 6 + j].ToString();
								if (j < 3)
									server += '.';
							}
							server += ':';
							int high = data[headerSize + i * 6 + 4];
							int low = data[headerSize + i * 6 + 5];
							int port = high;
							port <<= 8;
							port |= low;
							server += port.ToString();
							serverList.Add(server);
							Console.WriteLine(server);
						}
						continue;
					}

					if ((packetID = ComparePacket(data, length, endOfListResponsePart1)) != -1)
					{
						ushort ver = (ushort)((int)data[endOfListResponsePart1.Length + 2 + 1] << 8 | (int)data[endOfListResponsePart2.Length + 2 + 0]);
						udp.Send(GetThanks((ushort)packetID, ver));
						break;
					}
				}
				udp.Close();

				foreach (string server in serverList)
				{
					Console.WriteLine(server);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			return 0;
		}
	}
}
