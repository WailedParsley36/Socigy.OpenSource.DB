using Socigy.OpenSource.DB.Migrations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Socigy.OpenSource.DB.Core.Migrations
{
#nullable enable
    public interface IMigrationManager
    {
        Dictionary<string, ILocalMigration> LocalMigrations { get; }

        Task<IMigration> GetCurrentMigrationVersion();
        Task<ILocalMigration?> GetCurrentLocalMigrationVersion();

        Task EnsureLatestVersion();
    }
#nullable disable
}
