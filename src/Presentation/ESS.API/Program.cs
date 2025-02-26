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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.OpenApi.Models;
using ESS.API.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ESS.API.Filters.ValidationActionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.WriteIndented = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ESS API",
        Version = "v1",
        Description = "Education Support System API"
    });

    // Add XML comments for API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

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
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(contextFeature.Error, "Unhandled exception");

            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error. Please try again later."
            });
        }
    });
});

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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Initialize central database with retries
        try
        {
            await initializer.InitializeAsync();
            logger.LogInformation("Central database initialized successfully");

            // Add a delay to ensure the database is fully ready
            await Task.Delay(2000);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize central database. Application will continue, but functionality may be limited");
        }

        // Update all databases
        try
        {
            // Add a delay before attempting migrations
            logger.LogInformation("Waiting for database to be fully initialized before starting migrations...");
            await Task.Delay(5000);

            var migrationResult = await migrationService.UpdateAllDatabasesAsync();
            if (migrationResult)
            {
                logger.LogInformation("All databases migrated successfully");
            }
            else
            {
                logger.LogWarning("Some databases failed to migrate. Check migration status for details.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to migrate tenant databases. Application will continue, but functionality may be limited");
        }

        // Get and log migration status
        try
        {
            // Add a delay before checking migration status
            await Task.Delay(2000);

            var status = await migrationService.GetMigrationStatusAsync();
            logger.LogInformation("Migration Status - Central DB: {PendingCount} pending, {AppliedCount} applied",
                status.CentralDatabase.PendingMigrations.Count,
                status.CentralDatabase.AppliedMigrations.Count);

            foreach (var tenant in status.TenantDatabases)
            {
                if (tenant.Error != null)
                {
                    logger.LogError("Tenant {TenantName} ({TenantId}) migration error: {Error}",
                        tenant.TenantName, tenant.TenantId, tenant.Error);
                }
                else
                {
                    logger.LogInformation("Tenant {TenantName} ({TenantId}): {PendingCount} pending, {AppliedCount} applied",
                        tenant.TenantName, tenant.TenantId,
                        tenant.PendingMigrations.Count,
                        tenant.AppliedMigrations.Count);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get migration status");
        }
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while initializing/migrating the databases");
}

app.UseFileValidation();

app.Run();