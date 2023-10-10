using System.Data;
using System.Text;
using Dapper;

namespace EmailService.Extension
{
  public static class RepositoryExtensions
  {
    public static Task<int> InsertPostgresAsync(this IDbConnection connection,
                                                string sql,
                                                object param,
                                                IDbTransaction transaction = null)
    {
      var newSql = new StringBuilder();
      newSql.Append(sql);
      newSql.Append(" ");
      newSql.Append(" RETURNING id ");

      return connection.QueryFirstOrDefaultAsync<int>(newSql.ToString(),
                                                      param,
                                                      transaction,
                                                      null,
                                                      null);
    }
  }
}
