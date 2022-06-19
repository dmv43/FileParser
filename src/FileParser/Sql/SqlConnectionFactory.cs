using System.Data;
using Microsoft.Data.SqlClient;
using static FileParser.Constants.Constants;

namespace FileParser.Sql;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(SqlConnectionString);
    }
}
