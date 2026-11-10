using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IRacingLeague.API.Swagger;

public sealed class ExampleRequestBodyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody is null || !operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
            return;

        var route = $"{context.ApiDescription.HttpMethod} {context.ApiDescription.RelativePath?.Split('?')[0]}";
        var example = route switch
        {
            "POST api/v1/auth/register" => Object(
                ("userName", new OpenApiString("Ayrton Senna")),
                ("email", new OpenApiString("driver@example.com")),
                ("password", new OpenApiString("change-this-password")),
                ("tag", new OpenApiString("SEN")),
                ("licenseClass", new OpenApiString("Rookie"))),
            "POST api/v1/auth/login" => Object(
                ("email", new OpenApiString("driver@example.com")),
                ("password", new OpenApiString("change-this-password"))),
            "POST api/v1/leagues" or "PUT api/v1/leagues/{id}" => Object(
                ("name", new OpenApiString("Sunday Cup")),
                ("discipline", new OpenApiString("Road")),
                ("isPublic", new OpenApiBoolean(true)),
                ("maxDrivers", new OpenApiInteger(24)),
                ("entryFee", new OpenApiDouble(0))),
            "POST api/v1/leagues/{leagueId}/races" or "PUT api/v1/races/{id}" => Object(
                ("track", new OpenApiString("Spa-Francorchamps")),
                ("car", new OpenApiString("GT3")),
                ("scheduledAt", new OpenApiString("2027-03-14T14:00:00Z")),
                ("lapCount", new OpenApiInteger(20)),
                ("ambientTempC", new OpenApiDouble(18)),
                ("isCompleted", new OpenApiBoolean(false))),
            "POST api/v1/leagues/{leagueId}/members" => Object(
                ("carNumber", new OpenApiInteger(44)),
                ("teamName", new OpenApiString("Team Lotus")),
                ("ballastKg", new OpenApiDouble(0))),
            "POST api/v1/races/{raceId}/results" or "PUT api/v1/races/{raceId}/results/{id}" => Object(
                ("registrationId", new OpenApiInteger(12)),
                ("position", new OpenApiInteger(1)),
                ("fastestLapSeconds", new OpenApiDouble(92.5)),
                ("points", new OpenApiInteger(25)),
                ("incidentPoints", new OpenApiInteger(0)),
                ("dnf", new OpenApiBoolean(false)),
                ("notes", new OpenApiString("Clean race"))),
            "PUT api/v1/users/{id}" => Object(
                ("userName", new OpenApiString("Ayrton Senna")),
                ("tag", new OpenApiString("SEN")),
                ("licenseClass", new OpenApiString("Rookie"))),
            _ => null
        };

        if (example is not null)
        {
            mediaType.Examples["Example request"] = new OpenApiExample
            {
                Summary = "Example request body",
                Value = example
            };
        }
    }

    private static OpenApiObject Object(params (string Key, IOpenApiAny Value)[] entries)
    {
        var value = new OpenApiObject();
        foreach (var (key, item) in entries)
            value[key] = item;
        return value;
    }
}
