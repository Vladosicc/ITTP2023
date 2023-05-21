using ITTP_2023.DbContexts;
using ITTP_2023.Services;
using Microsoft.EntityFrameworkCore;

namespace ITTP_2023.Helpers
{
    public static class MigrationHelper
    {
        public static bool DatabaseExist(DbContextOptions options)
        {
            using UserContext userContext = new UserContext(options);
            return userContext.Database.CanConnect();
        }

        public static bool MigrationsExist(DbContextOptions options)
        {
            using UserContext userContext = new UserContext(options);
            return userContext.Database.GetMigrations().Count() > 0;
        }

        public static void CreateDatabase(DbContextOptions options)
        {
            using UserContext userContext = new UserContext(options);
            if(userContext.Database.EnsureCreated())
            {
                UserService userService = new UserService(userContext, null);
                userService.createAdmin().RunSynchronously();
            }

        }
    }
}
