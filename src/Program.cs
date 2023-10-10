using EmailService.AsyncDataServicesSubscriber;
using EmailService.Databases;
using EmailService.Extension;
using EmailService.Interfaces;
using EmailService.Middlewares;
using EmailService.Repository;
using EmitService.Databases;
using Microsoft.AspNetCore.Http.Json;
using Serilog;

try
{
  var builder = WebApplication.CreateBuilder(args);
  builder.Services.Configure<JsonOptions>(opt =>
  {
    opt.SerializerOptions.WriteIndented = true;
  });

  SerilogExtension.AddSerilogApi(builder.Configuration);
  builder.Host.UseSerilog(Log.Logger);
  builder.Host.UseSerilog((context, config) =>
  {
    var connectionString = context.Configuration.GetConnectionString("DatabaseEmail");

    config.WriteTo.PostgreSQL(connectionString, "LogsSystem", needAutoCreateTable: true)
      .MinimumLevel.Information()
      .MinimumLevel.Error();


    if (context.HostingEnvironment.IsProduction() == false)
    {
      config.WriteTo.Console().MinimumLevel.Information();
    }

  });

  builder.Services.AddControllers();
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();

  builder.Services.AddSingleton<IDatabase, PostgresDatabase>();
  builder.Services.AddHostedService<MessageBusSubscriber>();
  builder.Services.AddScoped<IEmailRepository, EmailRepository>();

  builder.Services.AddSwaggerGen(c =>
  {
    c.SwaggerDoc("v1", new() { Title = "EmailService API", Version = "v1" });
  });

  var app = builder.Build();

  app.UseMiddleware<ErrorHandlingMiddleware>();
  app.UseMiddleware<RequestSerilLogMiddleware>();

  if (builder.Environment.IsDevelopment())
  {
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmailService API v1"));
  }

  app.UseRouting();
  //app.UseAuthorization();
  app.MapControllers();
  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
  Log.Information("Server Shutting down...");
  Log.CloseAndFlush();
}
