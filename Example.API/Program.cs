using Example.Auth.DB;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using Socigy.OpenSource.DB.AuthDb.Extensions;
using Socigy.OpenSource.DB.Core;
using Socigy.OpenSource.DB.SharedDb.Extensions;
using Socigy.OpenSource.DB.UserDb.Extensions;
using System.Data.Common;
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
        .Select(x => new object?[] { x.ID, x.Email, x.Username })

        // SELECT id, emailVerified, username ...
        .Select(x => new object?[] { x.ID, username == "wailed" ? x.Email : x.EmailVerified, x.Username })

        // SELECT email AS "emailer", username = 'this_value' ... 
        .Select(x => new object?[] { Select.Custom($"{User.EmailColumnName} AS \"emailer\", {User.UsernameColumnName} = 'this_value'"), Select.All() })

        // SELECT id, CASE WHEN email = @p0 OR username LIKE ('%Example%') OR username LIKE ('Example%') OR username LIKE ('%Example') THEN true ELSE false END AS "is_example", username ...
        // p0 = 'example@example.com'
        // p1 = 'Example'
        .Select(x => new object?[] {
                    x.ID,
                    Select.Case()
                        .When(x.Email == "exampl@example.com" ||
                              x.Username.Contains("Example") /* LIKE ("%Example%") */ ||
                              x.Username.StartsWith("Example") /* LIKE ("Example%") */ ||
                              x.Username.EndsWith("Example") /* LIKE ("%Example") */)
                        .Then(true)
                        .Else(false)
                        .As("is_example"),
                    x.Username
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
                .Then(OrderBy.Desc(x.Username))
                .Else(x.Email)
        })

        // @pemail = 'example@example.com'
        // ORDER BY creation_date, birth_date DESC, CASE WHEN email == @pemail THEN username DESC ELSE email
        .OrderByDesc(x => new object?[] {
            OrderBy.Asc(x.Email),
            x.BirthDate,
            Select.Case()
                .When(x.Email == "example@example.com")
                .Then(x.Username)
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

await app.EnsureLatestSharedDbMigration();
await app.EnsureLatestAuthDbMigration();
await app.EnsureLatestUserDbMigration();

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

public class UpdateCommandBuilder : SqlCommandBuilder<UpdateCommandBuilder>
{
    public UpdateCommandBuilder()
    {
    }

    public async Task<int> ExecuteAsync()
    {
        return 0;
    }
}