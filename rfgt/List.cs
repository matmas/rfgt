using System;
using System.Collections;
using System.Text;
using ByteFX.Data.MySqlClient;

namespace rfgt
{
	class ServerList
	{
		private ArrayList serversList = new ArrayList();
		private Hashtable serversTimestamps = new Hashtable();
		private ArrayList serversToBeRemoved = new ArrayList();
		const int serversMaxCount = 65535;
		//Mysql mysql = new Mysql();

		public ServerList()
		{
			Reload();
			
			//mysql.Connect("192.168.1.155", "test", "test", "test");
			//Console.WriteLine((string)mysql.QueryToArray("SELECT * FROM test")[0]);
		}

		~ServerList()
		{
			//mysql.Close();
		}

		public void Reload()
		{
			serversToBeRemoved.Clear();
			foreach (string server in serversList)
			{
				if ((DateTime)serversTimestamps[server] == DateTime.MaxValue.AddYears(-1))
				{
					Program.PrintWithTimeStamp("Removing static server " + server);
					serversToBeRemoved.Add(server);
				}
			}
			foreach (string server in serversToBeRemoved)
			{
				serversList.Remove(server);
				serversTimestamps.Remove(server);
			}
			LoadList("servers.txt");
		}

		void LoadList(string filename)
		{
			if (System.IO.File.Exists(filename))
			{
				System.IO.StreamReader sr = new System.IO.StreamReader(filename);
				string text = sr.ReadToEnd();
				char[] delimeters = { '\n' };
				string[] lines = text.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);
				foreach (string line in lines)
				{
					if (line.Contains(":") && System.Text.RegularExpressions.Regex.IsMatch(line, "[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+:[0-9]+"))
					{
						if (!serversList.Contains(line))
						{
							Program.PrintWithTimeStamp("Adding static server " + line);
							serversList.Add(line);
							serversTimestamps.Add(line, DateTime.MaxValue.AddYears(-1));
						}
					}
				}
				sr.Close();
			}
		}

		public void Refresh()
		{
			serversToBeRemoved.Clear();
			foreach (string server in serversList)
			{
				if (((DateTime)serversTimestamps[server]).AddSeconds(256) < DateTime.Now)
				{
					Program.PrintWithTimeStamp("Removing dead server " + server);
					serversToBeRemoved.Add(server);
				}
			}
			foreach (string server in serversToBeRemoved)
			{
				serversList.Remove(server);
				serversTimestamps.Remove(server);
			}
		}

		public void Register(string client)
		{
			if (!serversList.Contains(client) && serversList.Count < serversMaxCount)
			{
				serversList.Add(client);
				const string logFilename = "servers.log";
				System.IO.StringReader sr = new System.IO.StringReader(logFilename);
				bool unique = !(sr.ReadToEnd().Contains(client));
				sr.Close();
				if (unique)
				{
					System.IO.StreamWriter sw = new System.IO.StreamWriter(logFilename, true);
					sw.WriteLine(client);
					sw.Close();
				}
			}
			serversTimestamps[client] = DateTime.Now;

		}

		public void Unregister(string client)
		{
			if (serversList.Contains(client))
			{
				serversList.Remove(client);
				serversTimestamps.Remove(client);
			}
		}

		public int Count()
		{
			return serversList.Count;
			//return 0;
		}

		public string this[int i]
		{
			get
			{
				//if (i == 0)
					
				return (string)serversList[i];
				//return "0";
			}
		}
	}
}
