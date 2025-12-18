using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DMA.Entities;
using DMA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DMA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService service;

        public AuthController(IAuthService service )
        {
            this.service = service;     
        }

        [HttpPost("register")]

        public async Task<ActionResult <User?>> Register(RegisterUserDto request)
        {
          var user = await service.RegisterAsync(request);
            if (user == null)
                return BadRequest("Username Already Exixts");

            return Ok(user);
        }

        [HttpPost("Login")]

        public async Task<ActionResult<string>> Login(LoginUserDto request)
        {
            var token = await service.LoginAsync(request);
            if (token is null)
                return BadRequest("username OR password is wrong ");

            return Ok(token);
        }
        [HttpGet]
        [Authorize]
      public IActionResult AuthenticatedOnlyEndPoint()
        {
            return Ok("You are authenticated");
        }
      
        [Authorize (Roles = "Admin")]
        [HttpGet ("admin-only")]
        public IActionResult AdminOnlyEndPoint()
        {
            return Ok("You are Admin");
        }
        [Authorize(Roles = "User")]
        [HttpGet("user-only")]
        public IActionResult userOnlyEndPoint()
        {
            return Ok("You are user");
        }
    }
}
