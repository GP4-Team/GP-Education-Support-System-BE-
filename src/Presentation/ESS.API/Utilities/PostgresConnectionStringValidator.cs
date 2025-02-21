using Npgsql;
using System;

namespace ESS.API.Utilities;

public static class PostgresConnectionStringValidator
{
    public static (bool IsValid, string ErrorMessage) ValidateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return (false, "Connection string cannot be empty");
        }

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);

            // Check for required properties
            if (string.IsNullOrWhiteSpace(builder.Host))
                return (false, "Host is required in the connection string");

            if (string.IsNullOrWhiteSpace(builder.Database))
                return (false, "Database name is required in the connection string");

            if (string.IsNullOrWhiteSpace(builder.Username))
                return (false, "Username is required in the connection string");

            return (true, string.Empty);
        }
        catch (ArgumentException ex)
        {
            return (false, $"Invalid connection string format: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error validating connection string: {ex.Message}");
        }
    }

    public static string GetSampleConnectionString()
    {
        return "Host=postgres;Database=tenant1;Username=postgres;Password=postgres;";
    }
}