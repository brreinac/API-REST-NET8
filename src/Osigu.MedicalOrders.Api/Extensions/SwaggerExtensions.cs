using Microsoft.OpenApi.Models;

namespace Osigu.MedicalOrders.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Osigu Medical Orders API",
                    Version = "v1",
                    Description =
                        "REST API for registering and querying medical orders."
                });

            options.AddServer(
                new OpenApiServer
                {
                    Url = "/"
                });
        });

        return services;
    }
}
