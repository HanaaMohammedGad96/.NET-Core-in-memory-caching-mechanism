using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarService.WebAPI.Data;
using CarService.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CarService.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarsService _carsService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "AllCars";

        public CarsController(ICarsService carsService, IMemoryCache cache)
        {
            _carsService = carsService;
            _cache       = cache;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = (await _carsService.Get(new[] { id }, null)).FirstOrDefault();
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery] Filters filters)
        {
          if(!_cache.TryGetValue(CacheKey, out IEnumerable<Car> cars))
          {
            cars = await _carsService.Get(null, filters);

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
              SlidingExpiration = System.TimeSpan.FromMinutes(5)
            };
            _cache.Set(CacheKey, cars, cacheEntryOptions);
          }
            return Ok(cars);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Car car)
        {
            _cache.Remove(CacheKey);

            await _carsService.Add(car);

            return Ok(car);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = (await _carsService.Get(new[] { id }, null)).FirstOrDefault();
            
            if (user == null)
                return NotFound();
            
            _cache.Remove(CacheKey);
            
            await _carsService.Delete(user);
            
            return NoContent();
        }
    }
}
