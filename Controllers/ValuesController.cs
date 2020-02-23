using System.Collections.Generic;
using System.Threading.Tasks;
using FullStackRebelsTestCSharp.Models;
using FullStackRebelsTestCSharp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FullStackRebelsTestCSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        DatabaseService _databaseService;

        public ValuesController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        // GET api/values
        [HttpGet]
        public async Task<ActionResult<List<Show>>> Get([FromQuery]int page = 0, [FromQuery]int limit = 50)
        {
            return await _databaseService.GetResults(limit, page);
        }
    }
}
