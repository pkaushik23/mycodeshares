using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FbAuthWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] string username)
        {
            if(username == "Prerak")
            {
                var token = await GenerateJwtToken(username);
                return Ok(new { user = username, token = token});
            }
            else
            {
                return BadRequest("Invalid User");
            }
        }

        private async Task<string> GenerateJwtToken(string username)
        {
            var someSecret = "a very random string which should come form appsettings";
            List<Claim> claims = new List<Claim>() {
                new Claim(ClaimTypes.Name,username),
                new Claim(ClaimTypes.Role,"User"),
               
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(someSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken SecurityToken = new JwtSecurityToken(issuer:"myapi.com",
              audience: "myapi.com",
              claims: claims,
              expires: DateTime.Now.AddDays(7),
              signingCredentials: credentials);
            return await Task.Run<string>(() => {
                return jwtTokenHandler.WriteToken(SecurityToken);
            });
        }
    }
}
