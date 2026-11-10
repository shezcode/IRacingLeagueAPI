using System.Text;
using IRacingLeague.Business;
using IRacingLeague.Data;
using IRacingLeague.Data.EF;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

// Controllers are the chosen presentation style for DWES2 (mirrors the BankApp reference).
builder.Services.AddControllers();

// JWT bearer authentication. Tokens are validated against the JWT settings in
// appsettings (overridable via JWT__SecretKey etc. as environment variables).
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]!))
        };
    });
builder.Services.AddAuthorization();

// EF Core against PostgreSQL. The connection string is overridable via the
// ConnectionStrings__ServerDB environment variable (used by Docker Compose in Step 20).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ServerDB")));

// Composition root: EF repositories replace the CLI's in-memory/JSON ones, so the
// reused Business services run unchanged. Scoped to match the DbContext lifetime.
builder.Services.AddScoped<ILeagueRepository, EfLeagueRepository>();
builder.Services.AddScoped<IRaceRepository, EfRaceRepository>();
builder.Services.AddScoped<IRegistrationRepository, EfRegistrationRepository>();
builder.Services.AddScoped<IResultRepository, EfResultRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();

builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRaceService, RaceService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IStandingsService, StandingsService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Shared private-zone read rule used by every league-scoped controller.
builder.Services.AddScoped<IRacingLeague.API.Authorization.LeagueViewPolicy>();

// Swagger / OpenAPI. EndpointsApiExplorer feeds the generator from the controller metadata.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "iRacing League API",
        Version = "v1",
        Description = "DWES2 API for the iRacing League Manager — reuses the DWES1 Models and Business layers."
    });

    // Let Swagger UI send a Bearer token via the "Authorize" button.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Paste the JWT (without the 'Bearer ' prefix).",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Apply pending EF Core migrations on boot so the containerized DB is
// schema-ready without a manual `dotnet ef database update`. Idempotent: a no-op when
// the schema is current. Retry transient connection errors during database startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var migrationLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    for (int attempt = 1; ; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (attempt < 12)
        {
            migrationLogger.LogWarning(ex, "Database not ready (attempt {Attempt}); retrying in 5s", attempt);
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }

    DbSeeder.Seed(db);
}

// Swagger is left on in every environment so the skeleton is verifiable in the container too.
app.UseSwagger();
app.UseSwaggerUI();

// Authentication must run before authorization (Step 18 adds [Authorize] policies).
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/health/ready", async (
    AppDbContext db,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        var isReady = await db.Database.CanConnectAsync(cancellationToken);
        return isReady
            ? Results.Ok(new { status = "ready", checks = new { postgres = "healthy" } })
            : Results.Json(
                new { status = "not_ready", checks = new { postgres = "unhealthy" } },
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception exception)
    {
        logger.LogWarning(exception, "Readiness check failed for PostgreSQL");
        return Results.Json(
            new { status = "not_ready", checks = new { postgres = "unhealthy" } },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapControllers();

app.Run();
