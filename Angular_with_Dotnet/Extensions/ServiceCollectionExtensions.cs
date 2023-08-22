using System;
using Microsoft.Extensions.Options;
using WritableOptionsService;

namespace Angular_with_Dotnet.Extensions
{
	public static class ServiceCollectionExtensions
	{
#pragma warning disable CS8604 // Possible null reference argument.
		public static void ConfigureWritable<T>(this IServiceCollection services, IConfigurationSection section, string filename) where T : class, new()
		{
			services.Configure<T>(section);
			services.AddTransient<IWritableSvc<T>>(provider =>
			{
				var environment = provider.GetService<IWebHostEnvironment>();
				var options = provider.GetService<IOptionsMonitor<T>>();
                return new WritableSvc<T>(environment: environment, options: options, section.Key, filename);
            });
		}
#pragma warning restore CS8604 // Possible null reference argument.
	}
}

