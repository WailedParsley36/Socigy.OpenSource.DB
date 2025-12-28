using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace UnitTest.DB.Tests
{
    public static class DbConnectionFactory
    {
        private const string ConnectionString = "Server=127.0.0.1;Port=5432;Database=UnitTest.DB;Userid=postgres;Password=1234;Protocol=3;Pooling=true;MinPoolSize=1;MaxPoolSize=20;ConnectionLifeTime=15";

        public static async Task InitializeAsync()
        {

        }

        public static NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }
    }
}
