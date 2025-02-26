# 1. For Central Database (in ESS.Infrastructure project directory)
dotnet ef migrations add Media -c ApplicationDbContext -o Persistence/Migrations/Central

# 2. For Tenant Database Template (in ESS.Infrastructure project directory)
dotnet ef migrations add MediaTenant -c TenantDbContext -o Persistence/Migrations/Tenant


