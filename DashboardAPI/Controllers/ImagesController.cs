using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DashboardAPI.Data;
using DashboardAPI.Models;
using ImageUploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DashboardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private DashboardDbContext _dashboardDbContext;

        public ImagesController(DashboardDbContext dashboardDbContext)
        {
            _dashboardDbContext = dashboardDbContext;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] Image imageModel)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _dashboardDbContext.User.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }
            var stream = new MemoryStream(imageModel.ImageArray);
            var guid = Guid.NewGuid().ToString();
            var file = $"{guid}.jpg";
            var folder = "wwwroot";
            var response = FilesHelper.UploadImage(stream, folder, file);
            if (!response)
            {
                return BadRequest();
            }
            else
            {
                var image = new Image()
                {
                    ImageUrl = file,
                    VehicleId = imageModel.VehicleId
                };
                _dashboardDbContext.Images.Add(image);
                _dashboardDbContext.SaveChanges();
                return StatusCode(StatusCodes.Status201Created);
            }
        }
    }
}
