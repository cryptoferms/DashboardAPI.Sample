using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AuthenticationPlugin;
using DashboardAPI.Data;
using DashboardAPI.Models;
using ImageUploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DashboardAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private DashboardDbContext _dashboardDbContext;
        private IConfiguration _configuration;
        private readonly AuthService _auth;

        public AccountController(IConfiguration configuration, DashboardDbContext dashboardDbContext)
        {
            _dashboardDbContext = dashboardDbContext;
            _configuration = configuration;
            _auth = new AuthService(_configuration);
        }


        [HttpPost]
        public IActionResult Register([FromBody] User user)
        {
            var userWithSameEmail = _dashboardDbContext.User.Where(u => u.Email == user.Email).SingleOrDefault();
            if (userWithSameEmail != null)
            {
                return BadRequest("Um usuário com o mesmo email já existe");
            }
            var userObj = new User()
            {
                Name = user.Name,
                Email = user.Email,
                Password = SecurePasswordHasherHelper.Hash(user.Password),
            };

            _dashboardDbContext.User.Add(userObj);
            _dashboardDbContext.SaveChanges();

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            var userEmail = _dashboardDbContext.User.FirstOrDefault(u => u.Email == user.Email);
            if (userEmail == null)
            {
                return NotFound();
            }
            if (!SecurePasswordHasherHelper.Verify(user.Password, userEmail.Password))
            {
                return Unauthorized();
            }
            var claims = new[]
            {
               new Claim(JwtRegisteredClaimNames.Email, user.Email),
               new Claim(ClaimTypes.Email, user.Email),
            };
            var token = _auth.GenerateAccessToken(claims);
            return new ObjectResult(new
            {
                access_token = token.AccessToken,
                expires_in = token.ExpiresIn,
                token_type = token.TokenType,
                creation_Time = token.ValidFrom,
                expiration_Time = token.ValidTo,
                user_id = userEmail.Id
            });

        }

        [HttpPost]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordModel changepasswordModel)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _dashboardDbContext.User.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }

            if (!SecurePasswordHasherHelper.Verify(changepasswordModel.OldPassword, user.Password))
            {
                return Unauthorized("Me desculpe você não pode alterar sua senha");
            }
            user.Password = SecurePasswordHasherHelper.Hash(changepasswordModel.NewPassword);
            _dashboardDbContext.SaveChanges();
            return Ok("Sua senha foi alterada com sucesso!");
        }
        [HttpPost]
        [Authorize]
        public IActionResult EditPhoneNumber([FromBody] ChangePhoneModel changePhoneModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _dashboardDbContext.User.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }
            user.Phone = changePhoneModel.PhoneNumber;
            _dashboardDbContext.SaveChanges();
            return Ok("Seu número de telefone foi atualizado com sucesso!");
        }

        [HttpPost]
        [Authorize] 
        public IActionResult EditUserProfile([FromBody] byte[] ImageArray)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _dashboardDbContext.User.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }
            var stream = new MemoryStream(ImageArray);
            var guid = Guid.NewGuid().ToString();
            var file = $"{guid}.jpg";
            var folder = "wwwroot/ProfileImages";
            var fullPath = $"{folder}/{file}";
            var response = FilesHelper.UploadImage(stream, folder, file);
            if (!response)
            {
                return BadRequest();
            }
            else
            {
                user.ImageUrl = file;
                _dashboardDbContext.SaveChanges();
                return StatusCode(StatusCodes.Status201Created);
            }
        }
        [HttpGet]
        public IActionResult UserProfileImage()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var user = _dashboardDbContext.User.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return NotFound();
            var responseResult = _dashboardDbContext.User
                .Where(x => x.Email == userEmail)
                .Select(x => new
                {
                    x.ImageUrl,
                })
                .SingleOrDefault();
            return Ok(responseResult);
        }
    }
}
