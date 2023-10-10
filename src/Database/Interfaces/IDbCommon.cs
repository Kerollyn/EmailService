namespace EmailService.Databases
{
  public interface IDbCommon
  {
    System.Data.IDbConnection GetDbConnection();

  }
}
