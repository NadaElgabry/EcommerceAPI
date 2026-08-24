using Serilog;

using Serilog.Events;

namespace EcommerceAPI.Extensions;

public static class SerilogExtensions

{

    public static void AddSerilogLogging(this WebApplicationBuilder builder)

    {

        Log.Logger = new LoggerConfiguration()

            .MinimumLevel.Information()

            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)

            .Enrich.FromLogContext()

            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")

            .WriteTo.File(

                path: "logs/log-.txt",

                rollingInterval: RollingInterval.Day,

                retainedFileCountLimit: 14,

                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")

            .CreateLogger();

        builder.Host.UseSerilog();

    }

}
