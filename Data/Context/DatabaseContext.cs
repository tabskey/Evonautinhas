using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace Evonautinhas.Data.Context
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext(string connectionString = null)
        {
            _connectionString = connectionString
                ?? ConfigurationManager.ConnectionStrings["TesteEscola"].ConnectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}