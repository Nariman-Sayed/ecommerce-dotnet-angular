using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecom.IntegrationTests;

public class DatabaseMigratorTests
{
    [Fact]
    public async Task ApplyAsync_WhenDisabled_DoesNotCreateSchema()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        using var context = new MigrationTestDbContext(
            new DbContextOptionsBuilder<MigrationTestDbContext>().UseSqlite(connection).Options);

        await DatabaseMigrator.ApplyAsync(context, migrateOnStartup: false);

        Assert.False(await TableExistsAsync(connection, "TestEntries"));
    }

    [Fact]
    public async Task ApplyAsync_WhenEnabled_AppliesPendingMigrations()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        using var context = new MigrationTestDbContext(
            new DbContextOptionsBuilder<MigrationTestDbContext>().UseSqlite(connection).Options);

        await DatabaseMigrator.ApplyAsync(context, migrateOnStartup: true);

        Assert.True(await TableExistsAsync(connection, "TestEntries"));
    }

    private static async Task<bool> TableExistsAsync(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name";
        command.Parameters.AddWithValue("$name", tableName);

        return (long)(await command.ExecuteScalarAsync())! > 0;
    }
}

internal sealed class MigrationTestDbContext : DbContext
{
    public MigrationTestDbContext(DbContextOptions<MigrationTestDbContext> options)
        : base(options)
    {
    }
}

[DbContext(typeof(MigrationTestDbContext))]
[Migration("20260101000000_CreateMigrationTestTable")]
internal sealed class CreateMigrationTestTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TestEntries",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
            },
            constraints: table => table.PrimaryKey("PK_TestEntries", x => x.Id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TestEntries");
    }
}
