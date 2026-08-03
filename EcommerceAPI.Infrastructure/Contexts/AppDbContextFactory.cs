using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcommerceAPI.Infrastructure.Contexts
{
    public class AppDbContextFactory
        : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            string currentDirectory =
                Directory.GetCurrentDirectory();

            string[] possiblePaths =
            {
                Path.Combine(
                    currentDirectory,
                    "appsettings.json"
                ),

                Path.Combine(
                    currentDirectory,
                    "EcommerceAPI",
                    "appsettings.json"
                ),

                Path.Combine(
                    currentDirectory,
                    "..",
                    "EcommerceAPI",
                    "appsettings.json"
                )
            };

            string? appSettingsPath =
                possiblePaths.FirstOrDefault(File.Exists);

            if (appSettingsPath is null)
            {
                throw new InvalidOperationException(
                    "Could not find EcommerceAPI/appsettings.json."
                );
            }

            string json =
                File.ReadAllText(appSettingsPath);

            using JsonDocument document =
                JsonDocument.Parse(json);

            JsonElement root =
                document.RootElement;

            if (!root.TryGetProperty(
                    "ConnectionStrings",
                    out JsonElement connectionStrings))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings section is missing."
                );
            }

            if (!connectionStrings.TryGetProperty(
                    "DefaultConnection",
                    out JsonElement defaultConnection))
            {
                throw new InvalidOperationException(
                    "DefaultConnection is missing."
                );
            }

            string? connectionString =
                defaultConnection.GetString();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "DefaultConnection is empty."
                );
            }

            var optionsBuilder =
                new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(
                optionsBuilder.Options
            );
        }
    }
}