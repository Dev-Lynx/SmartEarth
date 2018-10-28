using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Demos.DatabaseManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var connString = "Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase";

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Insert some data
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO data (some_field) VALUES (@p)";
                    cmd.Parameters.AddWithValue("p", "Hello world");
                    cmd.ExecuteNonQuery();
                }

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand("SELECT some_field FROM data", conn))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        Console.WriteLine(reader.GetString(0));
            }
        }
    }
}
