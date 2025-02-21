using Microsoft.Extensions.Configuration;

namespace ESS.API.Utilities;

public static class PostgresConnectionStringGenerator
{
    public static string GenerateConnectionString(
        IConfiguration configuration,
        string tenantIdentifier,
        string? customDatabaseName = null)
    {
        // Get central database connection string as template
        var centralConnectionString = configuration.GetConnectionString("DefaultConnection");

        // Parse the central connection string to extract host, port, username, password
        var connectionParts = centralConnectionString!.Split(';')
            .Select(part => part.Trim())
            .Where(part => !string.IsNullOrEmpty(part))
            .ToDictionary(
                part => part.Split('=')[0].ToLowerInvariant(),
                part => part.Contains('=') ? part.Substring(part.IndexOf('=') + 1) : string.Empty
            );

        // Sanitize tenant identifier for use in database name (remove special chars)
        var sanitizedIdentifier = new string(tenantIdentifier
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .ToArray())
            .ToLowerInvariant();

        // Generate database name
        var databaseName = customDatabaseName ?? $"tenant_{sanitizedIdentifier}";

        // Build new connection string
        var newConnectionString = new System.Text.StringBuilder();

        // Add host if available
        if (connectionParts.TryGetValue("host", out var host))
            newConnectionString.Append($"Host={host};");

        // Add port if available
        if (connectionParts.TryGetValue("port", out var port))
            newConnectionString.Append($"Port={port};");

        // Add database name
        newConnectionString.Append($"Database={databaseName};");

        // Add username if available
        if (connectionParts.TryGetValue("username", out var username))
            newConnectionString.Append($"Username={username};");

        // Add password if available
        if (connectionParts.TryGetValue("password", out var password))
            newConnectionString.Append($"Password={password};");

        // Add any additional parameters from configuration
        var additionalParams = configuration.GetSection("Postgres:AdditionalConnectionParams").Get<Dictionary<string, string>>();
        if (additionalParams != null)
        {
            foreach (var param in additionalParams)
            {
                newConnectionString.Append($"{param.Key}={param.Value};");
            }
        }

        return newConnectionString.ToString();
    }
}