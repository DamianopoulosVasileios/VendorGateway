using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VendorGateway.API;
using VendorGateway.APIs;
using VendorGateway.Application.DependencyInjection;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Common;
using VendorGateway.Configuration;
using VendorGateway.Enums;
using VendorGateway.Filters;
using VendorGateway.Infrastructure.Dependencies;
using VendorGateway.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Vendor Gateway API";
    config.OperationProcessors.Add(new IdempotencyKeyHeaderOperationProcessor());
});

Configuration(builder);

RegisterServices(builder);

SetupHttpClient(builder);

RegisterServicesFromOtherProjects(builder);

EnableServiceResolveOnStartUp(builder);

var app = builder.Build();

UseGlobalExceptionHandler(app);

Migrations(app);

EnableSwagger(app);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

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

static void Configuration(WebApplicationBuilder builder)
{
    builder.Services.Configure<VendorSettings>(builder.Configuration.GetSection(nameof(VendorSettings)));
}

static void SetupHttpClient(WebApplicationBuilder builder)
{
    var serviceProvider = builder.Services.BuildServiceProvider();
    var settings = serviceProvider.GetRequiredService<IOptions<VendorSettings>>().Value;
    foreach (var vendor in settings.VendorDetails.DistinctBy(x => x.Name))
    {
        builder.Services.AddHttpClient(vendor.Name, client =>
        {
            client.BaseAddress = new Uri(vendor.ApiUrl);
            client.Timeout = TimeSpan.FromSeconds(vendor.TimeoutSeconds);
        });
    }
}

static void RegisterServicesFromOtherProjects(WebApplicationBuilder builder)
{
    var connString = builder.Configuration.GetConnectionString("SQLiteConnectionString");
    var mode = builder.Configuration["AppSettings:Mode"];
    builder.Services.AddServicesFromInfrastructure(mode, connString);
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
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = exception switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(new
            {
                message = exception?.Message ?? "Unexpected error"
            });
        });
    });
}

static void Migrations(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
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