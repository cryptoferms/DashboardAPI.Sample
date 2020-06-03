using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DashboardAPI.Data;
using DashboardAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DashboardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private DashboardDbContext _dashboardDbContext;

        public VehiclesController(DashboardDbContext dashboardDbContext)
        {
            _dashboardDbContext = dashboardDbContext;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post(Vehicle vehicle)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _dashboardDbContext.User.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }
            var vehicleObj = new Vehicle()
            {
                Title = vehicle.Title,
                Description = vehicle.Description,
                Color = vehicle.Color,
                Company = vehicle.Company,
                Condition = vehicle.Condition,
                DatePosted = vehicle.DatePosted,
                Engine = vehicle.Engine,
                Price = vehicle.Price,
                Model = vehicle.Model,
                Location = vehicle.Location,
                CategoryId= vehicle.CategoryId,
                IsFeatured = false,
                IsHotAndNew = false,
                UserId = user.Id
            };
            _dashboardDbContext.Vehicles.Add(vehicleObj);
            _dashboardDbContext.SaveChanges();

            return Ok(new { vehicleId = vehicleObj.Id, message = "Veiculo adicionado com sucesso" });
        }

        //[HttpGet("[action]")]
        //public IActionResult HotAndNewAds()
        //{
        //    from v in _dashboardDbContext.Vehicles
        //    where v.IsHotAndNew == true
        //    select new
        //    {
        //        Id = v.Id,
        //        Title = v.Title,
        //        ImageUrl = v.Images.FirstOrDefault()
        //    };
        //}
    }
}
