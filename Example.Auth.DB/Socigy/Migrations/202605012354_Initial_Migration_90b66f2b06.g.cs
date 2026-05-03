using System;
using System.CodeDom.Compiler;
using Socigy.OpenSource.DB.Migrations;

/*
    This code was generated using Socigy.OpenSource.DB generation tool at 05/01/2026 23:54:51 by Patrik Stohanzl - 71410855+WailedParsley36@users.noreply.github.com
*/

namespace Example.Auth.DB.Socigy.Migrations
{
    [GeneratedCode("Socigy.OpenSource.DB", "1.0.0+8f8a3ba420048a3b432b2c3cc25b0522763a8ac1")]
    public class M_202605012354_Initial_Migration_90b66f2b06 : ILocalMigration
    {
        public string Id => _Id;

        public string UpSql => _UpSql;
        public string DownSql => _DownSql;

        public const string _Id = "202605012354_Initial_Migration_90b66f2b06";
                public const string _PreviousId = "202605011518_Added_procedures_73149000fa";
        public string PreviousId => _PreviousId;

public const string _UpSql = """
ALTER TABLE "users" DROP CONSTRAINT IF EXISTS "CHCK_users_7891533e80bb41b4932bd9453d50a59e";
ALTER TABLE "users" ADD CONSTRAINT "CHCK_users_3f1b083b6a7342a0ac044bf6eb520615" CHECK (LENGTH(email) < 25);
""";
        
public const string _DownSql = """
ALTER TABLE "users" ADD CONSTRAINT "CHCK_users_7891533e80bb41b4932bd9453d50a59e" CHECK (LEN(email) < 25);
ALTER TABLE "users" DROP CONSTRAINT IF EXISTS "CHCK_users_3f1b083b6a7342a0ac044bf6eb520615";
""";
    }
}

