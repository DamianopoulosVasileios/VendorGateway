using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Serilog;
using System.Threading.RateLimiting;
using VendorGateway.API.Filters;
using VendorGateway.Application.DependencyInjection;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Infrastructure.API;
using VendorGateway.Infrastructure.APIs;
using VendorGateway.Infrastructure.APIs.Configuration;
using VendorGateway.Infrastructure.Dependencies;
using VendorGateway.Infrastructure.Enums;
using VendorGateway.Infrastructure.Helpers;
using VendorGateway.Infrastructure.Persistence;

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
});

AddConfiguration(builder);

AddLogger(builder);

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

app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");

app.UseRateLimiter();

app.Run();


static void RegisterServices(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<VendorsConfiguration>();
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddScoped<IApiResponseReader, ApiResponseReader>();
    var defaultVendor = builder.Configuration["DefaultVendor"];
    if (defaultVendor == Vendors.FakeStore.ToString())
    {
        builder.Services.AddScoped<IProductsApiClient, FakeStoreProductsApiClient>();
        builder.Services.AddScoped<IAccountsApiClient, FakeStoreAccountsApiClient>();
    }
}

static void AddConfiguration(WebApplicationBuilder builder)
{
    builder.Services.Configure<VendorSettings>(builder.Configuration.GetSection(nameof(VendorSettings)));
}

static void SetupHttpClient(WebApplicationBuilder builder)
{
    var serviceProvider = builder.Services.BuildServiceProvider();
    var settings = serviceProvider.GetRequiredService<IOptions<VendorSettings>>().Value;
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

            var message = $"Unhandled exception occurred. Path: {context.Request.Path}. Message: {exception?.InnerException?.Message ?? exception?.Message ?? "Unexpected error"}";

            logger.LogError(message);

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = exception switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(new
            {
                message
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