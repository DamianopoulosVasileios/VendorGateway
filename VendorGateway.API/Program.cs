using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using VendorGateway.API;
using VendorGateway.API.Filters;
using VendorGateway.Application.DependencyInjection;
using VendorGateway.Application.Diagnostics;
using VendorGateway.Application.Dtos.Authentication;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Infrastructure.API;
using VendorGateway.Infrastructure.APIs;
using VendorGateway.Infrastructure.APIs.Configuration;
using VendorGateway.Infrastructure.Dependencies;
using VendorGateway.Infrastructure.Enums;
using VendorGateway.Infrastructure.Helpers;
using VendorGateway.Infrastructure.Persistence;
using VendorGateway.Infrastructure.Repositories.Authorization;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var environmentName = config["AppSettings:Mode"];

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = environmentName
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Vendor Gateway API";
    config.OperationProcessors.Add(new IdempotencyKeyHeaderOperationProcessor());

    var jwtScheme = new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste just the raw JWT token (no 'Bearer' prefix needed)."
    };

    config.AddSecurity("JWT", jwtScheme);

    config.DocumentProcessors.Add(
        new NSwag.Generation.Processors.Security.SecurityDefinitionAppender(
            "JWT", new[] { "JWT" }, jwtScheme));

});


AddConfiguration(builder);

RegisterAndConfigureAuthorization(builder);

AddLogger(builder);

SetupOpenTelemetry(builder);

SetupHealthChecks(builder);

RegisterServices(builder);

SetupHttpClient(builder);

SetupRateLimiter(builder);

RegisterServicesFromOtherProjects(builder);

EnableServiceResolveOnStartUp(builder);

var app = builder.Build();

UseGlobalExceptionHandler(app);

Migrations(app);

EnableSwagger(app);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapHealthChecks("/health").AllowAnonymous();

app.MapControllers().RequireRateLimiting("fixed");

app.Run();


static void RegisterServices(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<VendorsConfiguration>();
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddScoped<IApiResponseReader, ApiResponseReader>();
    var defaultVendor = builder.Configuration["DefaultVendor"];

    switch (defaultVendor)
    {
        case var _ when defaultVendor == Vendors.FakeStore.ToString():
            builder.Services.AddScoped<IProductsApiClient, FakeStoreProductsApiClient>();
            builder.Services.AddScoped<IAccountsApiClient, FakeStoreAccountsApiClient>();
            break;
        default:
            throw new InvalidOperationException(
                $"Unsupported or missing DefaultVendor configuration value: '{defaultVendor}'.");
    }
}

static void AddConfiguration(WebApplicationBuilder builder)
{
    builder.Services.Configure<VendorSettings>(builder.Configuration.GetSection(nameof(VendorSettings)));
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));
}

static void SetupHttpClient(WebApplicationBuilder builder)
{
    var settings = builder.Configuration.GetSection(nameof(VendorSettings)).Get<VendorSettings>()
                ?? throw new InvalidDataException("Cannot read vendor settings from configuration.");

    foreach (var vendor in settings.VendorDetails.DistinctBy(x => x.Name))
    {
        builder.Services
            .AddHttpClient(vendor.Name, client =>
            {
                client.BaseAddress = new Uri(vendor.ApiUrl);
                client.Timeout = TimeSpan.FromSeconds(vendor.TimeoutSeconds);
            })
            .AddStandardResilienceHandler(options =>
            {
                var oneRequestTimeout = 10;
                var totalRetries = 3;
                var delay = 2;
                var totalRequestTimeout = oneRequestTimeout * totalRetries + (delay * totalRetries);

                // Total timeout for the whole request + retries
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(totalRequestTimeout);

                // Timeout for a single HTTP attempt
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(oneRequestTimeout);

                // Retry policy
                options.Retry.MaxRetryAttempts = totalRetries;
                options.Retry.Delay = TimeSpan.FromSeconds(delay);
                options.Retry.BackoffType = DelayBackoffType.Exponential;

                // Circuit breaker
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
            });
    }
}

static void SetupOpenTelemetry(WebApplicationBuilder builder)
{
    builder.Services.AddMetrics();

    var isDevelopment = builder.Environment.IsDevelopment();

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("VendorGateway.API"))
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("Microsoft.EntityFrameworkCore")
                .AddMeter(VendorGatewayMetrics.MeterName);

            if (isDevelopment)
                metrics.AddConsoleExporter();
            else
                metrics.AddOtlpExporter(o => o.Endpoint = new Uri(GetOtlpEndpoint(builder.Configuration)));
        })
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();

            if (isDevelopment)
                tracing.AddConsoleExporter();
            else
                tracing.AddOtlpExporter(o => o.Endpoint = new Uri(GetOtlpEndpoint(builder.Configuration)));
        });
}

static string GetOtlpEndpoint(IConfiguration config) =>
    config["OpenTelemetry:OtlpEndpoint"]
        ?? throw new InvalidDataException("OpenTelemetry:OtlpEndpoint must be configured for non-Development environments.");

static void SetupHealthChecks(WebApplicationBuilder builder)
{
    builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>(name: "sqlite-db");
}

static void SetupRateLimiter(WebApplicationBuilder builder)
{
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("fixed", limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromSeconds(1);
            limiterOptions.QueueLimit = 10;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
}

static void RegisterServicesFromOtherProjects(WebApplicationBuilder builder)
{
    var mode = builder.Configuration["AppSettings:Mode"];
    builder.Services.AddServicesFromInfrastructure(mode);
    builder.Services.AddServicesFromApplication();
}

static void EnableServiceResolveOnStartUp(WebApplicationBuilder builder)
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateOnBuild = true;
        options.ValidateScopes = true;
    });
}

static void UseGlobalExceptionHandler(WebApplication app)
{
    app.UseExceptionHandler(appError =>
    {
        appError.Run(async context =>
        {
            var logger = context.RequestServices
                .GetRequiredService<ILogger<Program>>();

            var exception = context.Features
                .Get<IExceptionHandlerFeature>()?
                .Error;

            var exceptionMessage = exception?.InnerException?.Message ?? exception?.Message ?? "Unexpected error";

            logger.LogError(exception, "Unhandled exception occurred. Path: {Path}. Message: {Message}", context.Request.Path, exceptionMessage);

            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidDataException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;

            // Only expose exception details for client-actionable errors; hide internals on genuine server failures.
            var responseMessage = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exceptionMessage;

            await context.Response.WriteAsJsonAsync(new
            {
                message = responseMessage
            });
        });
    });
}

static void Migrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

static void EnableSwagger(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseOpenApi();
        app.UseSwaggerUi(settings =>
        {
            settings.EnableTryItOut = true;
        });
    }
}

static void AddLogger(WebApplicationBuilder builder)
{
    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    });
}

static void RegisterAndConfigureAuthorization(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IAuthorizationQueries, AuthorizationQueries>();
    builder.Services.AddScoped<IAuthorizationCommands, AuthorizationCommands>();
    builder.Services.AddScoped<IAuthorizationHandler, ExistingUserHandler>();

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = options.DefaultPolicy;

        options.AddPolicy("ExistingUser", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(
                new ExistingUserRequirement());
        });

    });

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>()
                ?? throw new InvalidDataException("Cannot read authentication details from configuration.");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });
}