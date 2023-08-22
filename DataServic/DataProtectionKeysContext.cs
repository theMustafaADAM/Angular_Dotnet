using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataService
{
#pragma warning disable CS8618 

    public class DataProtectionKeysContext : DbContext, IDataProtectionKeyContext
	{
        public DataProtectionKeysContext(DbContextOptions<DataProtectionKeysContext> options) : base(options) { }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }

#pragma warning restore CS8618 
}