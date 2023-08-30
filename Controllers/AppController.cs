using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodexAPI.Context;
using CodexAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppController : ControllerBase
    {

        private readonly AppDbContext _context;

        public AppController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Get(string TextoBase)
         {
            



         }

        


        
    }
}