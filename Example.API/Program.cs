using Example.Auth.DB;
using Npgsql;
using NpgsqlTypes;
using Socigy.OpenSource.DB.AuthDb.Extensions;
using Socigy.OpenSource.DB.CommandBuilders.Postgresql;
using Socigy.OpenSource.DB.Core;
using Socigy.OpenSource.DB.Core.CommandBuilders;
using Socigy.OpenSource.DB.Core.Delegates;
using Socigy.OpenSource.DB.Core.Interfaces;
using Socigy.OpenSource.DB.Core.Parsers;
using Socigy.OpenSource.DB.Core.Parsers.Postgresql;
using Socigy.OpenSource.DB.SharedDb.Extensions;
using Socigy.OpenSource.DB.UserDb.Extensions;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using static Socigy.OpenSource.DB.Core.SyntaxHelper.DB;

async Task TestAsync(DbConnection connection)
{
    string username = "wailed";

    // TODO: SQL Procedures mapping with conversions out
    // CREATE PROCEDURE GetUsersByEmail(IN email_param VARCHAR(255))

    // TODO: JOINs in SQL -> LATER LATER LATER
    //users = User.JoinCourses((x, y) => x.Email == y.TeacherEmail, JoinType.None)
    //    .Where()
    //    .WithConnection(connection)
    //    .ExecuteAsync();

    await user.Update()
        .WithConnection(connection)
        .Set(x => x.EmailVerified, true)
        .Where(x => x.Email == "")
        .ExecuteAsync();

    await user.Update()
        .WithConnection(connection)
        .WithAllFields()
        .WithFields(x => new object?[] { x.Username })
        .ExceptFields(x => new object?[] { x.ID, x.Email })
        .Where()
        .ExecuteAsync();

    User.Delete()
        .Where(x => x.Email.Contains("@gmail.com") || x.Username == username)
        .ExecuteAsync();

    var user = new User()
    {

    };
    await User.InsertAsync(user, connection);

    var build = new UpdateCommandBuilder<User>();

    await build
        .WithConnection(connection)

        .ExecuteAsync();


    var users = User.Query()
        .WithConnection(connection)
        .Limit(25)
        .Offset(25)
        .ExecuteAsync();

    // WHERE birth_date < @p0 AND ((SELECT 1 FROM ... custom sql) = 11 OR email IS NOT NULL)
    users = User.Query(x => x.BirthDate < DateTime.Today.AddYears(-18) && (Query.Custom("(SELECT 1 FROM ... custom sql) = 11") || x.Email != null))
        .Top(100)
        .WithConnection(connection)
        .ExecuteAsync();

    users = User.Query(x => x.ParentId == Guid.NewGuid() || x.IsChild && x.Visibility == UserVisibility.Public)
        // SELECT id,email,username ....
        .Select(x => new object?[] { x.ID, x.Email, x.Userna })

        // SELECT id, emailVerified, username ...
        .Select(x => new object?[] { x.ID, username == "wailed" ? x.Email : x.EmailVerified, x.Userna })

        // SELECT email AS "emailer", username = 'this_value' ... 
        .Select(x => new object?[] { Select.Custom($"{User.EmailColumnName} AS \"emailer\", {User.UsernaColumnName} = 'this_value'"), Select.All() })

        // SELECT id, CASE WHEN email = @p0 OR username LIKE ('%Example%') OR username LIKE ('Example%') OR username LIKE ('%Example') THEN true ELSE false END AS "is_example", username ...
        // p0 = 'example@example.com'
        // p1 = 'Example'
        .Select(x => new object?[] {
                    x.ID,
                    Select.Case()
                        .When(x.Email == "exampl@example.com" ||
                              x.Userna.Contains("Example") /* LIKE ("%Example%") */ ||
                              x.Userna.StartsWith("Example") /* LIKE ("Example%") */ ||
                              x.Userna.EndsWith("Example") /* LIKE ("%Example") */)
                        .Then(true)
                        .Else(false)
                        .As("is_example"),
                    x.Userna
        })
        .Select(x => new object?[]
        {
            Select.Case()
                .When(x.IsChild == true && x.IconUrl != null && x.IconUrl.StartsWith("https://images.socigy.com"))
                .Then("test")
                .Else(x.Email)
                .As(User.EmailColumnName)
        })

        // @pemail = 'example@example.com'
        // ORDER BY creation_date, birth_date DESC, CASE WHEN email == @pemail THEN username DESC ELSE email
        .OrderBy(x => new object?[] {
            x.Email,
            OrderBy.Desc(x.BirthDate),
            Select.Case()
                .When(x.Email == "example@example.com")
                .Then(OrderBy.Desc(x.Userna))
                .Else(x.Email)
        })

        // @pemail = 'example@example.com'
        // ORDER BY creation_date, birth_date DESC, CASE WHEN email == @pemail THEN username DESC ELSE email
        .OrderByDesc(x => new object?[] {
            OrderBy.Asc(x.Email),
            x.BirthDate,
            Select.Case()
                .When(x.Email == "example@example.com")
                .Then(x.Userna)
                .Else(OrderBy.Asc(x.Email))
        })
        .WithConnection(connection)
        .ExecuteAsync();

    await foreach (var user in users)
    {
        var isExample = user.GetCustomValue<bool>("is_example");
        if (!isExample)
            Console.WriteLine(user.Email);
    }
}

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddLogging(logging => logging.AddConsole());

builder.WebHost.UseKestrelHttpsConfiguration();
builder.Configuration.AddJsonFile("appsettings.json");

builder.AddSharedDb();
builder.AddAuthDb();
builder.AddUserDb();

var app = builder.Build();

// TODO: Bit flags in roles

await app.EnsureLatestSharedDbMigration();
await app.EnsureLatestAuthDbMigration();
await app.EnsureLatestUserDbMigration();

await app.RunAsync();

//var Configuration = app.Services.GetRequiredService<IConfiguration>();

//var connection = new NpgsqlConnection(Configuration.GetConnectionString("Default"));
//connection.ConnectionString.Replace("Database=AuthDb", "Database=postgres");

//await TestAsync(connection);


public class DeleteCommandBuilder : SqlCommandBuilder<DeleteCommandBuilder>
{
    public DeleteCommandBuilder()
    {
    }

    public async Task<int> ExecuteAsync()
    {
        return 0;
    }
}

#nullable enable

namespace Socigy.OpenSource.DB.CommandBuilders.Postgresql
{
    public class UpdateCommandBuilder<T> : SqlCommandBuilder<UpdateCommandBuilder<T>>
        where T : IDbTable
    {
#if NET6_0_OR_GREATER
        protected System.Data.Common.DbBatch? _Batch;

        /// <summary>
        /// Specifies the batch to use for subsequent database operations.
        /// </summary>
        /// <remarks>If <paramref name="batch"/> is <see langword="null"/>, the method attempts to create a new
        /// batch from the current connection or transaction. The batch is used to group multiple database commands for
        /// execution as a single unit.</remarks>
        /// <param name="batch">The <see cref="DbBatch"/> instance to associate with the operation, or <see langword="null"/> to create a new
        /// batch from the current connection or transaction.</param>
        /// <returns>The current instance with the specified batch applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="batch"/> is <see langword="null"/> and neither a connection nor a transaction is
        /// specified.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="batch"/> is <see langword="null"/> and the provided transaction does not have an
        /// associated <see cref="DbConnection"/>.</exception>
        public UpdateCommandBuilder<T> WithBatch(DbBatch? batch)
        {
            if (batch == null)
            {
                if (_Connection == null && _Transaction == null)
                    throw new ArgumentNullException(nameof(batch), "If batch is null, either connection or transaction must be specified!");

                _Batch = _Connection?.CreateBatch() ?? _Transaction!.Connection?.CreateBatch() ?? throw new InvalidOperationException("The provided transaction has no DbConnection from which a DbBatch could be created");
                _Batch.Transaction = _Transaction;
            }
            else
                _Batch = batch;

            return this;
        }

        /// <summary>
        /// Adds a new command to the current batch operation.
        /// </summary>
        /// <remarks>This method should be called only after a batch has been initialized using
        /// WithBatch(). Attempting to add to a batch without initialization will result in an exception.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if no batch has been provided. Call WithBatch() before invoking this method.</exception>
        public void AddToBatch()
        {
            if (_Batch == null)
                throw new InvalidOperationException("Cannot add to batch when no DbBatch was provided. Please call WithBatch() first.");

            var batchCommand = _Batch.CreateBatchCommand();
            _Batch.BatchCommands.Add(batchCommand);
        }

        /// <summary>
        /// Adds a new command to the current database batch asynchronously.
        /// </summary>
        /// <remarks>This method should be called only after a batch has been initialized using
        /// WithBatch(). It is typically used to accumulate multiple commands for execution as a single batch
        /// operation.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no database batch has been provided. Call WithBatch() before invoking this method.</exception>
        public async Task AddToBatchAsync()
        {
            if (_Batch == null)
                throw new InvalidOperationException("Cannot add to batch when no DbBatch was provided. Please call WithBatch() first.");

            var batchCommand = _Batch.CreateBatchCommand();
            _Batch.BatchCommands.Add(batchCommand);
        }
#endif

        private readonly T _TableRow;
        private Expression<Func<T, bool>> _WhereClause;

        public UpdateCommandBuilder(T rowInstance)
        {
            _TableRow = rowInstance;
        }

        public UpdateCommandBuilder<T> Where(Expression<Func<T, bool>> where)
        {
            _WhereClause = where;
            return this;
        }

        private ISqlVisitor GetWhereVisitor(ParameterExpression param, GetColumnName getColName, DbCommand command)
        {
            return new PostgresqlWhereVisitor(param, getColName, command);
        }

        public async Task<int> ExecuteAsync()
        {
#if NET6_0_OR_GREATER
            if (_Batch != null)
                throw new InvalidOperationException("Cannot execute command when DbBatch was provided.");
#endif

            if (_Connection == null)
                throw new InvalidOperationException("No DbConnection provided.");

            if (_Connection.State != System.Data.ConnectionState.Open)
                await _Connection.OpenAsync();

            await using var command = _Connection.CreateCommand() as NpgsqlCommand;
            if (command == null) return 0;

            if (_Transaction != null)
                command.Transaction = _Transaction as NpgsqlTransaction;

            var columnNames = new List<string>();
            var paramNames = new List<string>();

            foreach (var row in _ColumnInfo)
            {
                string colName = row.Key;
                object? value = row.Value.Value;
                Type type = row.Value.Type;

                columnNames.Add($"\"{colName}\"");

                string paramName = $"@{colName}";
                paramNames.Add(paramName);

                var param = new NpgsqlParameter(paramName, value ?? DBNull.Value);

                // If the value is null, we MUST specify the type explicitly
                // If the value exists, Npgsql can usually infer it, but being explicit doesn't hurt.
                if (value == null || value == DBNull.Value)
                {
                    param.NpgsqlDbType = GetDbType(type);
                }

                command.Parameters.Add(param);
            }

            string? where = null;
            if (_WhereClause != null)
                where = GetWhereVisitor(_WhereClause.Parameters[0], /*TODO: <#= ClassName #>*/ User.GetColumnDbName, command).Parse(_WhereClause);

            command.CommandText = $@"
        UPDATE ""{_TableRow.GetTableName()}"" 
        {setStatements}
        {where}";

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected;
        }

        public NpgsqlDbType GetDbType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            return type switch
            {
                Type t when t == typeof(int) => NpgsqlDbType.Integer,
                Type t when t == typeof(long) => NpgsqlDbType.Bigint,
                Type t when t == typeof(string) => NpgsqlDbType.Text,
                Type t when t == typeof(bool) => NpgsqlDbType.Boolean,
                Type t when t == typeof(DateTime) => NpgsqlDbType.Timestamp,
                Type t when t == typeof(float) => NpgsqlDbType.Real,
                Type t when t == typeof(double) => NpgsqlDbType.Double,
                Type t when t == typeof(decimal) => NpgsqlDbType.Numeric,
                Type t when t == typeof(Guid) => NpgsqlDbType.Uuid,
                Type t when t == typeof(byte[]) => NpgsqlDbType.Bytea,
                Type t when t == typeof(short) => NpgsqlDbType.Smallint,
                Type t when t == typeof(char) => NpgsqlDbType.Char,
                // Fallback or specific handling for JSON, Arrays, etc.
                _ => NpgsqlDbType.Text
            };
        }
    }
}

#nullable disable
