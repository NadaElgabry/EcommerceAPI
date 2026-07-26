# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj/solution files first (layer caching: only re-restores if these change)
COPY ["EcommerceAPI.slnx", "./"]
COPY ["EcommerceAPI/EcommerceAPI.csproj", "EcommerceAPI/"]
COPY ["EcommerceAPI.Domain/EcommerceAPI.Domain.csproj", "EcommerceAPI.Domain/"]
COPY ["EcommerceAPI.Application/EcommerceAPI.Application.csproj", "EcommerceAPI.Application/"]
COPY ["EcommerceAPI.Infrastructure/EcommerceAPI.Infrastructure.csproj", "EcommerceAPI.Infrastructure/"]

RUN dotnet restore "EcommerceAPI/EcommerceAPI.csproj"

# Now copy everything else and build
COPY . .
WORKDIR "/src/EcommerceAPI"
RUN dotnet build "EcommerceAPI.csproj" -c Release -o /app/build

# ---- Publish stage ----
FROM build AS publish
RUN dotnet publish "EcommerceAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Final runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs folder (Serilog writes here) so it exists even before first write
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "EcommerceAPI.dll"]