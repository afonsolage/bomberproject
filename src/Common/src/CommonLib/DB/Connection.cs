#if _SERVER

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.DB
{
    public abstract class ConnectionFactory
    {
        private static readonly Dictionary<string, string> _connStringMap = new Dictionary<string, string>();

        public static void LoadConfigFile(string path)
        {
            var lines = File.ReadAllLines(path);

            foreach(var line in lines)
            {
                var strs = line.Split('|');

                if (strs.Length != 2)
                    continue;

                AddConnectionString(strs[0], strs[1]);
            }
        }

        public static void AddConnectionString(string name, string data)
        {
            _connStringMap[name] = data;
        }

        internal static MySqlConnection GetConnection(string name)
        {
            var connectionString = _connStringMap[name];
            return new MySqlConnection(connectionString);
        }
    }

    public class DBConnection : IDisposable
    {
        private MySqlConnection _connection;

        public DBConnection(string databaseName)
        {
            _connection = ConnectionFactory.GetConnection(databaseName);
            _connection.Open();
        }

        public int Execute(string command, params object[] parameters)
        {
            var cmd = new MySqlCommand(command, _connection);

            int paramIdx = 1;
            foreach(object param in parameters)
            {
                cmd.Parameters.AddWithValue("@p" + paramIdx++, param);
            }

            return cmd.ExecuteNonQuery();
        }

        public MySqlDataReader Query(string command, params object[] parameters)
        {
            var cmd = new MySqlCommand(command, _connection);

            int paramIdx = 1;
            foreach (object param in parameters)
            {
                cmd.Parameters.AddWithValue("@p" + paramIdx++, param);
            }

            return cmd.ExecuteReader();
        }

        public void Dispose()
        {
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }
}

#endif