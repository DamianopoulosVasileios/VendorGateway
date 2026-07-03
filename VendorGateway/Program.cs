using Microsoft.Extensions.Options;
using VendorGateway.API;
using VendorGateway.APIs;
using VendorGateway.Configuration;
using VendorGateway.Enums;
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

var serviceProvider = builder.Services.BuildServiceProvider();

var defaultVendor = builder.Configuration["DefaultVendor"];

if (defaultVendor == Vendors.FakeStore.ToString())
{
    builder.Services.AddScoped<IProductsApiClient, FakeStoreProductsApiClient>();
    builder.Services.AddScoped<IUsersApiClient, FakeStoreAccountsApiClient>();
}

var settings = serviceProvider.GetRequiredService<IOptions<VendorSettings>>().Value;
foreach (var vendor in settings.VendorDetails.DistinctBy(x => x.Name))
{
    builder.Services.AddHttpClient(vendor.Name, client =>
    {
        client.BaseAddress = new Uri(vendor.ApiUrl);
        client.Timeout = TimeSpan.FromSeconds(vendor.TimeoutSeconds);
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
