using ModelService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DataService
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(

                new
                {
                    Id              = "1",
                    NormalizedName  = "ADMINISTRATOR",
                    Name            = "Administrator",
                    RoleName        = "Administrator",
                    Handle          = "administrator",
                    RoleIcon        = "/uploads/roles/icons/default/role.png",
                    IsActive        = true
                },

                new
                {
                    Id              = "2",
                    NormalizedName  = "CUSTOMER",
                    Name            = "Customer",
                    RoleName        = "Customer",
                    Handle          = "customer",
                    RoleIcon        = "/uploads/roles/icons/default/role.png",
                    IsActive        = true
                }
            );
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<AddressModel> addresses { get; set; }
        public DbSet<TokenModel> Tokens { get; set; }
        public DbSet<ActivityModel> Activities { get; set; }
        public DbSet<CountryModel> Countries { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<PermissionType> PermissionsType { get; set; }
        public DbSet<TwoFactorCodeModel> TwoFactorCodes { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

