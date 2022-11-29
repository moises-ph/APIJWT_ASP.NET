using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ApiProductosJWT.Models;
using System.Data;
using System.Data.SqlClient;
using BC = BCrypt.Net.BCrypt;

namespace ApiProductosJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticationController : ControllerBase
    {
        private readonly string cadenaSQL;
        private readonly string secretKey;

        public AutenticationController(IConfiguration config)
        {
            secretKey = config.GetSection("settings").GetSection("secreteKey").Value;
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }

        [HttpPost]
        [Route("auth")]
        public IActionResult Post(User user)
        {
            try
            {
                using (var conection = new SqlConnection(cadenaSQL))
                {
                    string password = null;
                    int Id = 0;
                    conection.Open();
                    var cmd = new SqlCommand("Auth_usr", conection);
                    cmd.Parameters.AddWithValue("email", user.email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            password = reader["password"].ToString();
                            Id = Convert.ToInt32(reader["Id"].ToString());
                            reader.Close();
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status400BadRequest, new { Message = "El Usuario no existe" });
                        }
                    }
                    conection.Close(); 
                    
                    if(BC.Verify(user.password, password))
                    {
                        var keyBytes = Encoding.ASCII.GetBytes(secretKey);
                        var claims = new ClaimsIdentity();
                        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.email));
                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = claims,
                            Expires = DateTime.UtcNow.AddMinutes(5),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
                        };
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);
                        string tokencreado = tokenHandler.WriteToken(tokenConfig);
                        return StatusCode(StatusCodes.Status200OK, new { token = tokencreado });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status401Unauthorized, new { token = "" });
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message, Error = ex });

            }
        }
    }
}
