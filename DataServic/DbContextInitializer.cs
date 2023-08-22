using CountryService;
using FunctionalService;

namespace DataService
{
    public static class DbContextInitializer
	{
		public static async Task Initialize(
            DataProtectionKeysContext dataProtectionKeysContext,
            ApplicationDbContext applicationDbContext,
            IFunctionalSvc functionalSvc,
            ICountrySvc countrySvc )
		{
            // Check, If db DataProtectionKeysContext is created
            // Check, If db ApplicationDbContext is created

            await dataProtectionKeysContext.Database.EnsureCreatedAsync();
            await applicationDbContext.Database.EnsureCreatedAsync();

            // Check, If db contains any users. if not empty do nothing, it has been already seeded
            if (applicationDbContext.ApplicationUsers.Any()) return; 

            // but if empty, create admin user and App user
            await functionalSvc.CreateDefaultAdminUser();
            await functionalSvc.CreateDefaultUser();

            // Populate Country database
            var countries = await countrySvc.GetCountriesAsync();
            if (countries.Count > 0)
            {
                await applicationDbContext.Countries.AddRangeAsync(countries);
                await applicationDbContext.SaveChangesAsync();
            }

        }
    }
}

