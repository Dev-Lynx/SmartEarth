using NLog;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Services
{
    public class PostgreSQLDatabaseManager// : IDatabaseManager
    {
        #region Properties
        ILogger Logger { get; }
        public bool Initialized { get; private set; }

        #region Internals
        Npgsql.NpgsqlConnection Connection { get; set; }
        #endregion

        #endregion


        #region Constructors
        public PostgreSQLDatabaseManager(ILogger logger)
        {
            Logger = logger;
            Initialize();
        }
        #endregion

        #region Methods
        void Initialize()
        {
            OpenConnection();
            Initialized = true;
        }

        void OpenConnection()
        {
            var connectionString = $"Server={Core.DATABASE_SERVER_NAME};Port={Core.DATABASE_PORT};User Id={Core.DATABASE_USERNAME};Password={Core.DATABASE_PASSWORD}";
            Connection = new Npgsql.NpgsqlConnection(connectionString);
            Connection.Open();
        }
        #endregion
    }
}
