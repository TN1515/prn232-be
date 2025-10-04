using System;
using System.IO;
using Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace API.Context;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DBContext>
{
    public DBContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<DBContext>();
    optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        return new DBContext(optionsBuilder.Options);
    }
}
