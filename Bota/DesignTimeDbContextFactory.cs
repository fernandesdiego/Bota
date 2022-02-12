using Bota.Context;
using Microsoft.EntityFrameworkCore.Design;
using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Bota.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            var asp = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var dot = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var environment = asp == "Development" || dot == "Development" ? "Development" : "Production";

            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"{@Directory.GetCurrentDirectory()}/../Bota/appsettings.{environment}.json").Build();
            var builder = new DbContextOptionsBuilder<ApplicationContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnectionString");
            builder.UseMySql(connectionString, MySqlServerVersion.AutoDetect(connectionString));
            return new ApplicationContext(builder.Options);
        }

    }
}
