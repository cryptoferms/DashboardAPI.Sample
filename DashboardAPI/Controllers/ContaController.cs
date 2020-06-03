using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using AuthenticationPlugin;
using DashboardAPI.Data;
using DashboardAPI.Helpers;
using DashboardAPI.Models;
using ImageUploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DashboardAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ContaController : ControllerBase
    {
        private DashboardDbContext _dashboardDbContext;
        private IConfiguration _configuration;
        private readonly AuthService _auth;

        public ContaController(IConfiguration configuration, DashboardDbContext dashboardDbContext)
        {
            _dashboardDbContext = dashboardDbContext;
            _configuration = configuration;
            _auth = new AuthService(_configuration);
        }

        //POST api/conta/registro
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Registro([FromBody] Usuario usuario)
        {
            var userWithSameEmail = _dashboardDbContext.Usuarios.Where(u => u.Email == usuario.Email).SingleOrDefault();
            if (userWithSameEmail != null)
            {
                return BadRequest("Um usuário com o mesmo email já existe");
            }
            if (usuario.Senha != usuario.ConfirmarSenha)
            {
                return BadRequest("Senhas não são iguais.");
            }
            var usuarioObj = new Usuario()
            {
                Nome = usuario.Nome,
                Sobrenome = usuario.Sobrenome,
                Telefone = usuario.Telefone,
                Endereco = usuario.Endereco,
                Email = usuario.Email,
                DataNascimento = usuario.DataNascimento,
                DataInclusao = DateTime.Now,
                Senha = SecurePasswordHasherHelper.Hash(usuario.Senha),
                ConfirmarSenha = SecurePasswordHasherHelper.Hash(usuario.ConfirmarSenha),
                Status = '1'
            };
            _dashboardDbContext.Usuarios.Add(usuarioObj);
            _dashboardDbContext.SaveChanges();

            return StatusCode(StatusCodes.Status201Created);
        }

        //POST /api/conta/Login
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] Usuario usuario)
        {
            var usuarioEmail = _dashboardDbContext.Usuarios.FirstOrDefault(u => u.Email == usuario.Email);
            if (usuarioEmail == null)
            {
                return NotFound();
            }
            if (!SecurePasswordHasherHelper.Verify(usuario.Senha, usuarioEmail.Senha))
            {
                return Unauthorized();
            }
            var claims = new[]
            {
               new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
               new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
               new Claim(ClaimTypes.Name, usuario.Email),
            };
            var token = _auth.GenerateAccessToken(claims);
            return new ObjectResult(new
            {
                access_token = token.AccessToken,
                expires_in = token.ExpiresIn,
                token_type = token.TokenType,
                creation_Time = token.ValidFrom,
                expiration_Time = token.ValidTo,
                user_id = usuarioEmail.Id,
                user_name = usuarioEmail.Nome,
                user_email = usuario.Email,
            });
        }

        //POST api/conta/TrocarSenha
        [HttpPost]
        public IActionResult TrocarSenha([FromBody] ChangePasswordModel changepasswordModel)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _dashboardDbContext.Usuarios.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }

            if (!SecurePasswordHasherHelper.Verify(changepasswordModel.OldPassword, user.Senha))
            {
                return Unauthorized("Me desculpe você não pode alterar sua senha");
            }
            user.Senha = SecurePasswordHasherHelper.Hash(changepasswordModel.NewPassword);
            _dashboardDbContext.SaveChanges();
            return Ok("Sua senha foi alterada com sucesso!");
        }

        //POST api/conta/EditarTelefone
        [HttpPost]
        public IActionResult EditarTelefone([FromBody] ChangePhoneModel changePhoneModel)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _dashboardDbContext.Usuarios.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }
            user.Telefone = changePhoneModel.PhoneNumber;
            _dashboardDbContext.SaveChanges();
            return Ok("Seu número de telefone foi atualizado com sucesso!");
        }

        //POST api/conta/Editarperfil
        [HttpPost]
        public IActionResult EditarPerfil([FromBody] byte[] ImageArray)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
            var user = _dashboardDbContext.Usuarios.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)  return BadRequest();          
            var stream = new MemoryStream(ImageArray);
            var guid = Guid.NewGuid().ToString();
            var file = $"{guid}.jpg";
            var folder = "wwwroot/ProfileImages";
            var fullPath = $"{folder}/{file}";
            var imageFullPath = fullPath.Remove(0, 7);
            var response = ArquivosHelper.UploadPhoto(stream, folder, file);
            if (!response) return BadRequest();
            user.ImageUrl = imageFullPath;
            _dashboardDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }
        // GET api/conta/FotodePerfil
        [HttpGet]
        public IActionResult FotodePerfil()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var user = _dashboardDbContext.Usuarios.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return NotFound();
            var responseResult = _dashboardDbContext.Usuarios
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
