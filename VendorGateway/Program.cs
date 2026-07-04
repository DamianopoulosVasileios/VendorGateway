using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VendorGateway.API;
using VendorGateway.APIs;
using VendorGateway.Common;
using VendorGateway.Configuration;
using VendorGateway.Enums;
using VendorGateway.Infrastructure;
using VendorGateway.Infrastructure.Persistence;
using VendorGateway.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Vendor Gateway API";
});

builder.Services.Configure<VendorSettings>(builder.Configuration.GetSection(nameof(VendorSettings)));
builder.Services.AddSingleton<VendorsConfiguration>();

builder.Services.AddScoped<IApiResponseReader, ApiResponseReader>();

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
