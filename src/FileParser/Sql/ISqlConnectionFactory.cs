using System.Data;

namespace FileParser.Sql;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
