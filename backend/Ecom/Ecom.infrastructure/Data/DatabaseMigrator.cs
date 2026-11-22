using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ecom.infrastructure.Data;

/// <summary>
/// Applies pending database migrations when startup migration is enabled.
/// </summary>
public static class DatabaseMigrator
{
    /// <summary>
    /// Applies pending migrations to the supplied context when <paramref name="migrateOnStartup"/> is true.
    /// </summary>
    /// <param name="context">The database context to migrate.</param>
    /// <param name="migrateOnStartup">Whether startup migration is enabled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async Task ApplyAsync(
        DbContext context,
        bool migrateOnStartup,
        CancellationToken cancellationToken = default)
    {
        if (!migrateOnStartup)
            return;

        // MigrateAsync creates the database before applying migrations. Skip it
        // when an existing database is already current, which happens on every
        // local restart with a persisted data volume.
        var databaseCreator = context.GetService<IRelationalDatabaseCreator>();
        if (await databaseCreator.ExistsAsync(cancellationToken)
            && !(await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            return;
        }

        await context.Database.MigrateAsync(cancellationToken);
    }
}
