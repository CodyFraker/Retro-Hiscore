using Npgsql;

namespace RetroHiscore.Api.Infrastructure;

public static class AdminOperationalErrorFormatter
{
    public const string SchemaOutOfDateMessage =
        "Database schema is out of date. Redeploy the API so migrations can run.";

    public const string GenericDispatchFailureMessage =
        "Discord dispatch failed. See server logs for details.";

    public static string ForAdminDisplay(Exception exception)
    {
        if (TryGetPostgresException(exception, out var pg))
        {
            if (pg.SqlState == PostgresErrorCodes.UndefinedColumn
                || pg.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                return SchemaOutOfDateMessage;
            }
        }

        return GenericDispatchFailureMessage;
    }

    public static string ForAdminDisplay(string? rawMessage)
    {
        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            return GenericDispatchFailureMessage;
        }

        if (rawMessage.Contains("42703:", StringComparison.Ordinal)
            || (rawMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
                && rawMessage.Contains("column", StringComparison.OrdinalIgnoreCase)))
        {
            return SchemaOutOfDateMessage;
        }

        if (rawMessage.Length <= 120 && !LooksLikeInternalDatabaseError(rawMessage))
        {
            return rawMessage;
        }

        return GenericDispatchFailureMessage;
    }

    private static bool LooksLikeInternalDatabaseError(string message)
        => message.Contains("Npgsql", StringComparison.Ordinal)
            || message.Contains("PostgresException", StringComparison.Ordinal)
            || message.Contains("42703:", StringComparison.Ordinal);

    private static bool TryGetPostgresException(Exception exception, out PostgresException postgres)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException pg)
            {
                postgres = pg;
                return true;
            }
        }

        postgres = null!;
        return false;
    }
}
