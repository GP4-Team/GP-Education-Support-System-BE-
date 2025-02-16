# 1. For Central Database (in ESS.Infrastructure project directory)
dotnet ef migrations add InitialCentral -c ApplicationDbContext -o Persistence/Migrations/Central

# 2. For Tenant Database Template (in ESS.Infrastructure project directory)
dotnet ef migrations add InitialTenant -c TenantDbContext -o Persistence/Migrations/Tenant