FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/Presentation/ESS.API/ESS.API.csproj", "src/Presentation/ESS.API/"]
COPY ["src/Core/ESS.Domain/ESS.Domain.csproj", "src/Core/ESS.Domain/"]
COPY ["src/Core/ESS.Application/ESS.Application.csproj", "src/Core/ESS.Application/"]
COPY ["src/Infrastructure/ESS.Infrastructure/ESS.Infrastructure.csproj", "src/Infrastructure/ESS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Presentation/ESS.API/ESS.API.csproj"

# Copy the rest of the source code
COPY . .

# Build
WORKDIR "/src/src/Presentation/ESS.API"
RUN dotnet build "ESS.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ESS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ESS.API.dll"]