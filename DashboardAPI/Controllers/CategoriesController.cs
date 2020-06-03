using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DashboardAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private DashboardDbContext _dashboardDbContext;

        public CategoriesController(DashboardDbContext dashboardDbContext)
        {
            _dashboardDbContext = dashboardDbContext;
        }


        [HttpGet]
        public IActionResult Get()
        {
            var categories = _dashboardDbContext.Categories;
            return Ok(categories);
        }


    }
}
