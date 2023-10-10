using System.Data;
using EmailService.Databases;

namespace EmitService.Databases
{
  public class PostgresDatabase : IDatabase
  {
    private readonly IConfiguration _configuration;

    public PostgresDatabase(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public IDbConnection GetDbConnection()
    {
      return new Npgsql.NpgsqlConnection(_configuration.GetConnectionString("DatabaseEmail"));
    }
  }

}
