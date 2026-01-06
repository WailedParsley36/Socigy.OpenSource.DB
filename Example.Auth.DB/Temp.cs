using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Socigy.OpenSource.DB.Core.Migrations;
using Socigy.OpenSource.DB.Core;
using Socigy.OpenSource.DB.Migrations;
using Example.Shared.DB.Socigy.Generated;

#nullable enable

namespace Example.User.DB.Socigy.Generated
{
    public static partial class UserDb
    {
        public class MigrationManager : IMigrationManager
        {
            private static readonly List<ILocalMigration> _localMigrationsOrderedDesc =
            [
              new Example.User.DB.Socigy.Migrations.M_Initial_Migration(),
                ];

            private Dictionary<string, ILocalMigration> _localMigrations = _localMigrationsOrderedDesc.ToDictionary(static x => x.Id, static x => x);
            public Dictionary<string, ILocalMigration> LocalMigrations => _localMigrations;


            private readonly ILogger _Logger;
            private readonly IDbConnectionFactory _ConnectionFactory;
            public MigrationManager(ILogger<MigrationManager> logger, [FromKeyedServices("UserDb")] IDbConnectionFactory connectionFactory)
            {
                _Logger = logger;
                _ConnectionFactory = connectionFactory;
            }

            public Task EnsureLatestVersion()
            {
                return EnsureMigration(_localMigrationsOrderedDesc.First());
            }
            public Task EnsureMigration(string migrationId)
            {
                if (!_localMigrations.TryGetValue(migrationId, out ILocalMigration? migration))
                    throw new MissingMemberException($"Missing local migration with ID {migrationId}");

                return EnsureMigration(migration);
            }
            public async Task EnsureMigration(ILocalMigration migration)
            {
                await _ConnectionFactory.EnsureDbExists();

                var dbVersion = await GetCurrentMigrationVersion();
                int sourceMigrationIndex = -1;
                int targetMigrationIndex = -1;

                if (dbVersion == null)
                    sourceMigrationIndex = _localMigrationsOrderedDesc.Count;
                else
                {
                    sourceMigrationIndex = _localMigrationsOrderedDesc.FindIndex(x => x.Id == dbVersion.HumanId);
                    if (sourceMigrationIndex < 0)
                    {
                        throw new InvalidDataException($"Unable to find local migration with ID {dbVersion.HumanId} that is in the Database. Aborting!");
                    }
                }

                targetMigrationIndex = _localMigrationsOrderedDesc.FindIndex(x => x.Id == migration.Id);
                if (targetMigrationIndex < 0) throw new InvalidDataException($"Target migration {migration.Id} not found locally.");


                if (sourceMigrationIndex == targetMigrationIndex)
                {
                    _Logger.LogInformation($"SharedDb is already at version: {migration.Id}");
                    return;
                }

                using var connection = _ConnectionFactory.Create();
                await connection.OpenAsync();

                if (sourceMigrationIndex > targetMigrationIndex)
                {
                    for (int i = sourceMigrationIndex - 1; i >= targetMigrationIndex; i--)
                    {
                        var migToApply = _localMigrationsOrderedDesc[i];
                        _Logger.LogInformation($"Applying UP migration: {migToApply.Id} - {migToApply.GetType().Name}");

                        using var command = connection.CreateCommand();
                        command.CommandText = migToApply.UpSql;
                        await command.ExecuteNonQueryAsync();

                        // Record the migration
                        await SharedDb.Migration.InsertAsync(new SharedDb.Migration
                        {
                            HumanId = migToApply.Id,
                            AppliedAt = DateTime.UtcNow,
                            ExecutedBy = $"{Environment.UserName} - {Environment.MachineName}",
                            IsRollback = false
                        }, connection);
                    }
                }
                else
                {
                    for (int i = sourceMigrationIndex; i < targetMigrationIndex; i++)
                    {
                        var migToApply = _localMigrationsOrderedDesc[i];
                        _Logger.LogInformation($"Applying DOWN migration: {migToApply.Id} - {migToApply.GetType().Name}");

                        using var command = connection.CreateCommand();
                        command.CommandText = migToApply.DownSql;
                        await command.ExecuteNonQueryAsync();

                        // Record the rollback
                        await SharedDb.Migration.InsertAsync(new SharedDb.Migration
                        {
                            HumanId = migToApply.Id,
                            AppliedAt = DateTime.UtcNow,
                            ExecutedBy = $"{Environment.UserName} - {Environment.MachineName}",
                            IsRollback = true
                        }, connection);
                    }
                }
            }

            public async Task<ILocalMigration?> GetCurrentLocalMigrationVersion()
            {
                var latestVersion = await GetCurrentMigrationVersion();
                if (latestVersion == null || !_localMigrations.TryGetValue(latestVersion.HumanId, out var result))
                    return null;

                return result;
            }

            public async Task<IMigration?> GetCurrentMigrationVersion()
            {
                try
                {
                    using var connection = _ConnectionFactory.Create();

                    var versions = UserDb.Migration.Query()
                        .WithConnection(connection)
                        .OrderByDesc(x => new object[] { x.AppliedAt })
                        .ExecuteAsync();

                    return await versions.FirstAsync();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}

#nullable disable

