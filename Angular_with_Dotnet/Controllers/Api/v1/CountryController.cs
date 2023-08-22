using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using DataService;
using Microsoft.EntityFrameworkCore;
using ModelService;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Angular_with_Dotnet.Controllers.Api.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CountryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CountryController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCountries()
        {
            List<CountryModel> countries = await _db.Countries.OrderBy(x => x.Name).ToListAsync();
            return Ok(countries);
        }
    }
}

