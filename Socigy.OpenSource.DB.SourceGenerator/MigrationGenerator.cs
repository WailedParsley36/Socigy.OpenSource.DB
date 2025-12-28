using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Socigy.OpenSource.DB.SourceGenerator.Templates;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Socigy.OpenSource.DB.SourceGenerator
{
    public static class MigrationGenerator
    {
        public static void Execute(SourceProductionContext ctx, Compilation compilation)
        {
            // Migrations table, needed everytime
            var migrationTableNamespace = $"{compilation.AssemblyName}.Socigy.Generated";
            ctx.AddSource("Migrations.g.cs", new MigrationTableTemplate() { BaseNamespace = migrationTableNamespace }.TransformText());
            ctx.AddSource("Migrations.table.g.cs", new TableColumnNameClassTemplate()
            {
                ClassName = "Migrations",
                Namespace = migrationTableNamespace,
                TableName = "_scg_migrations",
                Columns = [
                    new TableColumnNameClassTemplate.ColumnInfo() { Name = "Id", DatabaseName = "id" },
                    new TableColumnNameClassTemplate.ColumnInfo() { Name = "HumanId", DatabaseName = "human_id" },
                    new TableColumnNameClassTemplate.ColumnInfo() { Name = "IsRollback", DatabaseName = "is_rollback" },
                    new TableColumnNameClassTemplate.ColumnInfo() { Name = "AppliedAt", DatabaseName = "applied_at" },
                    new TableColumnNameClassTemplate.ColumnInfo() { Name = "ExecutedBy", DatabaseName = "executed_by" },
                ],
            }.TransformText());
            ctx.AddSource("MigrationsSyntaxMethods.table.g.cs", new TableSyntaxGeneratorTemplate()
            {
                ClassName = "Migrations",
                Namespace = migrationTableNamespace,
                DbEnginePrefix = "Postgresql", // TODO: Make this dynamic from the configuration file
                Columns =
                [
                    ("Id", typeof(long).FullName, null),
                    ("HumanId", typeof(string).FullName, null),
                    ("IsRollback", typeof(bool).FullName, null),
                    ("AppliedAt", typeof(DateTime).FullName, null),
                    ("ExecutedBy", typeof(string).FullName, null),
                ],
            }.TransformText());

            // TODO: Generate bidnings for Generated.Migrations table too...
        }
    }
}
