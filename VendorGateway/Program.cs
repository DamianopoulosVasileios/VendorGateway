using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VendorGateway.API;
using VendorGateway.APIs;
using VendorGateway.Common;
using VendorGateway.Configuration;
using VendorGateway.Enums;
using VendorGateway.Infrastructure;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;
using VendorGateway.Infrastructure.Repositories.Account;
using VendorGateway.Infrastructure.Repositories.Order;
using VendorGateway.Infrastructure.Repositories.Product;
using VendorGateway.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Vendor Gateway API";
});

builder.Services.AddSingleton<VendorsConfiguration>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IApiResponseReader, ApiResponseReader>();
builder.Services.AddScoped<IAccountQueries, AccountQueries>();
builder.Services.AddScoped<IProductCommands, ProductCommands>();
builder.Services.AddScoped<IOrderCommands, OrderCommands>();
builder.Services.AddScoped<IOrderQueries, OrderQueries>();
builder.Services.AddScoped<IApiResponseReader, ApiResponseReader>();
builder.Services.AddScoped<IAccountCommands, AccountCommands>();
builder.Services.AddScoped<IProductQueries, ProductQueries>();
builder.Services.AddScoped<IAccountQueries, AccountQueries>();
builder.Services.AddScoped<IProductCommands, ProductCommands>();
builder.Services.AddScoped<IOrderCommands, OrderCommands>();
builder.Services.AddScoped<IOrderQueries, OrderQueries>();
builder.Services.AddScoped<IApiResponseReader, ApiResponseReader>();
builder.Services.AddScoped<IAccountCommands, AccountCommands>();
builder.Services.AddScoped<IProductQueries, ProductQueries>();

builder.Services.Configure<VendorSettings>(builder.Configuration.GetSection(nameof(VendorSettings)));
var defaultVendor = builder.Configuration["DefaultVendor"];
if (defaultVendor == Vendors.FakeStore.ToString())
{
    builder.Services.AddScoped<IProductsApiClient, FakeStoreProductsApiClient>();
    builder.Services.AddScoped<IAccountsApiClient, FakeStoreAccountsApiClient>();
}

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

builder.Services.AddExceptionClassifierInfrastructure();
builder.Services.AddInfrastructure(builder.Configuration);

var mode = builder.Configuration["AppSettings:Mode"];
var dbPath = DbPathResolver.GetPath(mode);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));


builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

var app = builder.Build();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(settings =>
    {
        settings.EnableTryItOut = true;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
