using System;
using System.CodeDom.Compiler;
using Socigy.OpenSource.DB.Migrations;

/*
    This code was generated using Socigy.OpenSource.DB generation tool at 01/06/2026 13:54:45 by WAILEARDOS\user1
*/

namespace Example.Shared.DB.Socigy.Migrations
{
    [GeneratedCode("Socigy.OpenSource.DB", "1.0.0+88f6256107c0fee72d0d906e044e190c689b4f4f")]
    public class M_Initial_Migration : ILocalMigration
    {
        public string Id => _Id;

        public string UpSql => _UpSql;
        public string DownSql => _DownSql;

        public const string _Id = "Initial Migration";
        #nullable enable
        public string? PreviousId => null;
        #nullable disable

public const string _UpSql = """
CREATE TABLE "shared" (
    "id" uuid,
    CONSTRAINT "PK_shared" PRIMARY KEY ("id")
);
CREATE TABLE "_scg_migrations" (
    "id" bigint,
    "human_id" text,
    "applied_at" timestamp without time zone,
    "is_rollback" boolean DEFAULT false,
    "executed_by" text,
    CONSTRAINT "PK__scg_migrations" PRIMARY KEY ("id")
);
""";
        
public const string _DownSql = """
DROP TABLE IF EXISTS "_scg_migrations" CASCADE;
DROP TABLE IF EXISTS "shared" CASCADE;
""";
    }
}

