using Microsoft.Data.SqlClient;
using System.Data;

namespace AdminManagement.Context
{
    public class AdminContext
    {
        private readonly IConfiguration _configuration;

        private readonly string _connection;

        public AdminContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connection = _configuration.GetConnectionString("AdminConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connection);
    }
}
