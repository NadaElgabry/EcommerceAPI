using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Infrastructure.Contexts;
using EcommerceAPI.Infrastructure.Persistence.Seed;
using EcommerceAPI.Infrastructure.Services.Search.Indexing;
using Serilog;

namespace EcommerceAPI.Extensions;

public static class WebApplicationExtensions
{
    public static  WebApplication UseAppPipelineAsync(this WebApplication app)
    {
        app.UseExceptionHandler();

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
/*        else
        {
            app.UseHttpsRedirection();
        }*/
        app.UseCors("Dev");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}