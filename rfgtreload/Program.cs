using System;
using System.Collections;
using System.Text;

namespace rfgtreload
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length != 1)
				{
					Console.WriteLine("usage: rfgtreload.exe <host>:<port>");
					return;
				}
				UdpClient udp = new UdpClient();
				udp.Connect(int.Parse(args[0].Split(':')[1]), System.Net.Dns.GetHostAddresses(args[0].Split(':')[0])[0]);

				byte[] reloadRequest = { 0xaa, 0xaa, 0xaa, 0xaa };
		
				udp.Send(reloadRequest);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}
	}
}
