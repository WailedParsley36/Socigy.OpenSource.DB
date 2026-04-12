using System;
using System.CodeDom.Compiler;
using Socigy.OpenSource.DB.Migrations;

/*
    This code was generated using Socigy.OpenSource.DB generation tool at 02/16/2026 17:19:36 by Patrik Stohanzl - 71410855+WailedParsley36@users.noreply.github.com
*/

namespace Example.Auth.DB.Socigy.Migrations
{
    [GeneratedCode("Socigy.OpenSource.DB", "1.0.0+88f6256107c0fee72d0d906e044e190c689b4f4f")]
    public class M_Renamed_User_Column : ILocalMigration
    {
        public string Id => _Id;

        public string UpSql => _UpSql;
        public string DownSql => _DownSql;

        public const string _Id = "Renamed_User_Column";
                public const string _PreviousId = "Initial Migration";
        public string PreviousId => _PreviousId;

public const string _UpSql = """
ALTER TABLE "users" RENAME COLUMN "username" TO "userna";
""";
        
public const string _DownSql = """
ALTER TABLE "users" RENAME COLUMN "userna" TO "username";
""";
    }
}

