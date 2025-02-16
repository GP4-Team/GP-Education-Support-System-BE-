using Serilog;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ESS.Infrastructure;
using ESS.Application;
using ESS.Application.Common.Interfaces;
using ESS.Infrastructure.MultiTenancy.TenantResolution;
using ESS.Infrastructure.MultiTenancy;
using ESS.Infrastructure.Persistence;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using ESS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: new[] { "db", "sql", "postgresql" })
    .AddRedis(
        builder.Configuration["Redis:Configuration"]!,
        name: "redis",
        tags: new[] { "cache", "redis" });

// Add application layer
builder.Services.AddApplication();

// Add infrastructure layer
builder.Services.AddInfrastructure(builder.Configuration);

// Configure Multi-tenancy
TenantConfiguration.AddMultiTenancy(builder.Services, builder.Configuration);
builder.Services.AddScoped<DatabaseMigrationService>();

builder.Services.AddDbContext<TenantDbContext>((serviceProvider, options) =>
{
    var tenantInfo = serviceProvider.GetService<IMultiTenantContextAccessor<EssTenantInfo>>()?
        .MultiTenantContext?.TenantInfo;

    var connectionString = tenantInfo?.ConnectionString
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseNpgsql(connectionString,
        npgsqlOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(3);
        });
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowAll");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add error handling
app.UseExceptionHandler("/error");

// Add tenant resolution before routing
app.UseTenantResolution();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Info = report.Entries.Select(e => new
            {
                Key = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration,
                Exception = e.Value.Exception?.Message
            })
        };
        await System.Text.Json.JsonSerializer.SerializeAsync(
            context.Response.Body,
            result,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)
        );
    }
});

// Initialize database
try
{
    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();

        // Initialize central database
        await initializer.InitializeAsync();
        app.Logger.LogInformation("Central database initialized successfully");

        // Update all databases
        var migrationResult = await migrationService.UpdateAllDatabasesAsync();
        if (migrationResult)
        {
            app.Logger.LogInformation("All databases migrated successfully");
        }
        else
        {
            app.Logger.LogWarning("Some databases failed to migrate. Check migration status for details.");
        }

        // Get and log migration status
        var status = await migrationService.GetMigrationStatusAsync();
        app.Logger.LogInformation("Migration Status - Central DB: {PendingCount} pending, {AppliedCount} applied",
            status.CentralDatabase.PendingMigrations.Count,
            status.CentralDatabase.AppliedMigrations.Count);

        foreach (var tenant in status.TenantDatabases)
        {
            if (tenant.Error != null)
            {
                app.Logger.LogError("Tenant {TenantName} ({TenantId}) migration error: {Error}",
                    tenant.TenantName, tenant.TenantId, tenant.Error);
            }
            else
            {
                app.Logger.LogInformation("Tenant {TenantName} ({TenantId}): {PendingCount} pending, {AppliedCount} applied",
                    tenant.TenantName, tenant.TenantId,
                    tenant.PendingMigrations.Count,
                    tenant.AppliedMigrations.Count);
            }
        }
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while initializing/migrating the databases");
}

app.Run();