using Microsoft.EntityFrameworkCore;

namespace AllegroSearchService.Data.Config
{
    public interface IDbInitializer<T>
        where T : DbContext
    {
        void SeedEverything(T dbContext);

        void Initialize(T dbContext);
    }
}
