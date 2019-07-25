using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityAPI.Infrastructure;
using IdentityAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private IdentityDBContext db;
        private IConfiguration configuration;
        public IdentityController(IdentityDBContext dbcontext, IConfiguration config)
        {
            db = dbcontext;
            configuration = config;
        }

        //POST  /api/identity/register
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("register")]
        public async Task<ActionResult<dynamic>> RegisterUser([FromBody]UserInfo user)
        {
            //TryValidateModel(user); //controller method used to validate the model forcefully. It not mandatory as validation is automatically done.
            if(ModelState.IsValid)
            {
                var result = await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
                //return Created("", result.Entity);
                return Created("",new {
                    Email = result.Entity.Email,
                    FirstName = result.Entity.FirstName,
                    LastName = result.Entity.LastName,
                    CreatedDate = DateTime.Now
                });
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        //POST  /api/identity/token
        [HttpPost("token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<dynamic>> GetToken([FromBody]LoginModel login)
        {
            TryValidateModel(login);
            if(ModelState.IsValid)
            {
                var token = GenerateToken(login);
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized();
                }
                else
                {
                    return Ok(new { status = true, token = token });
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        private string GenerateToken(LoginModel login)
        {
            var user = db.Users.SingleOrDefault(u => u.Email == login.Email && u.Password == login.Password);
            if (user == null)
                return null;
            else
            {
                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.FirstName),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("Jwt:Secret")));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: configuration.GetValue<string>("Jwt:Issuer"),
                    audience: configuration.GetValue<string>("Jwt:Audience"),
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: credentials
                    );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
    }
}