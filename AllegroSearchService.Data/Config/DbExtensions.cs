using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace AllegroSearchService.Data.Config
{
    public static class DbExtensions
    {
        public static IServiceCollection UpdateDatabase<T, U>(
            this IServiceCollection services,
            IServiceProvider serviceProvider)
            where T : DbContext
            where U : IDbInitializer<T>, new()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<T>();
                var initializer = new U();


                if (!dbContext.AllMigrationsApplied())
                {
                    dbContext.Database.Migrate();
                }
                initializer.SeedEverything(dbContext);

                if (!dbContext.AllMigrationsApplied())
                {
                    throw new InvalidOperationException("Структура БД не соответствует последней версии.");
                }

                initializer.Initialize(dbContext);
            }

            return services;
        }

        public static bool AllMigrationsApplied(this DbContext context)
        {
            var applied = context.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var total = context.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }
    }
}
