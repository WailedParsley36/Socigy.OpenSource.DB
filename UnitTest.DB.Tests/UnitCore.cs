using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Socigy.OpenSource.DB.Core;
using Socigy.OpenSource.DB.Core.Migrations;
using Socigy.OpenSource.DB.TestDb.Extensions;

namespace UnitTest.DB.Tests
{
    public static class UnitCore
    {
        public static bool IsInitialized { get; private set; }
        public static IDbConnectionFactory ConnectionFactory { get; private set; }

        public static async Task InitializeHostAsync()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;
            var builder = Host.CreateApplicationBuilder();
            builder.AddTestDb();

            var app = builder.Build();
            //var migrationManager = app.Services.GetRequiredKeyedService<IMigrationManager>("TestDb");

            ConnectionFactory = app.Services.GetRequiredKeyedService<IDbConnectionFactory>("TestDb");
            await ConnectionFactory.EnsureDbExists();

            await app.StartAsync();
        }
    }
}
