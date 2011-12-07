using ByteFX.Data.MySqlClient;
using System.Data;

namespace rfgt
{
	public class Mysql
	{
		private MySqlConnection mysql;

		public void Connect(string server, string user, string password, string database)
		{
			string myConn = "Data Source=" + server + ";User ID=" + user + ";Database=" + database + ";Password=" + password;
			mysql = new MySqlConnection(myConn);
			mysql.Open();
		}

		public MySqlDataAdapter MakeDataAdapter(string query)
		{
			return new MySqlDataAdapter(query, mysql);
		}

		public object[] QueryToArray(string query)
		{
			MySqlDataReader rd;
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = mysql;
			cmd.CommandText = query;
			rd = cmd.ExecuteReader();
			if (!rd.Read())
				return null;
			object[] vals = new object[rd.FieldCount];
			rd.GetValues(vals);
			rd.Close();
			return vals;
		}

		public DataTable QueryToTable(string query, string table)
		{
			MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, mysql);
			DataSet dataSet = new DataSet();
			dataAdapter.Fill(dataSet, table);
			return dataSet.Tables[0];
		}

		public void Close()
		{
			mysql.Close();
		}
	}
}