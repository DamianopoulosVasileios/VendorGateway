using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(config.GetConnectionString("SQLiteConnectionString")));
            return services;
        }
    }

    public static class DbPathResolver
    {
        public static string GetPath(string envirnoment)
        {
            var projectFolder = "Infrastructure";
            var dbFolderName = "DbFile";

            var path = Path.Combine(projectFolder, dbFolderName);

            Directory.CreateDirectory(path);

            return Path.Combine(path, "VendorGateway.db");
        }
    }
}
