using ERP.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ERP.API.Extensions
{
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Apply database migrations (Development only)
        /// </summary>
        public static void MigrateDatabase(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database");
            }
        }
    }
}
