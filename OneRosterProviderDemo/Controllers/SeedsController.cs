using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;

namespace OneRosterProviderDemo.Controllers
{
    [Route("seeds")]
    public class SeedsController : BaseController
    {
        public SeedsController(ApiContext _db) : base(_db)
        {

        }

        [HttpGet]
        public IActionResult Reset()
        {
            SeedData.Initialize(db);
            return Ok("Seeded");
        }
    }
}