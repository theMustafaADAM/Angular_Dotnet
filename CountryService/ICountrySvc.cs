using System;
using ModelService;

namespace CountryService
{
    public interface ICountrySvc
	{
        Task<List<CountryModel>> GetCountriesAsync();
    }
}

