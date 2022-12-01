using ApiProductosJWT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using BC = BCrypt.Net.BCrypt;

namespace ApiProductosJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string cadenaSQL;
        public UserController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }

        [HttpPost]
        public IActionResult Post([FromBody] User user)
        {
            try
            {
                string password = BC.HashPassword(user.password, 10);
                using (var conection = new SqlConnection(cadenaSQL))
                {
                    conection.Open();
                    string? message = null;
                    int err = 0;
                    var cmd = new SqlCommand("Create_usr", conection);
                    cmd.Parameters.AddWithValue("Name", user.Name);
                    cmd.Parameters.AddWithValue("Lastname", user.Lastname);
                    cmd.Parameters.AddWithValue("email", user.email);
                    cmd.Parameters.AddWithValue("password", password);
                    cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.ExecuteNonQuery();
                    //return StatusCode(StatusCodes.Status200OK, new { Message = "Usuario creado correctamente" });
                    using (var reader = cmd.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            err = Convert.ToInt32(reader["Error"].ToString());
                            if (err == 1)
                            {
                                message = reader["Respuesta"].ToString();
                                reader.Close();
                                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Hubo un error al crear el usuario", Error = err });
                            }
                            else
                            {
                                reader.Close();
                                return StatusCode(StatusCodes.Status200OK, new { Message = "Usuario creado correctamente" });
                            }
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Hubo un error al crear el usuario" });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex, Message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize]
        [Route("update/{id:int}")]
        public IActionResult Put(int id, [FromBody] User user)
        {
            try
            {
                string? password = user.password != null ? BC.HashPassword(user.password, 10) : null;
                using (var conection = new SqlConnection(cadenaSQL))
                {
                    conection.Open();
                    string? message = null;
                    int err = 0;
                    var cmd = new SqlCommand("Update_usr", conection);
                    cmd.Parameters.AddWithValue("Id", id);
                    cmd.Parameters.AddWithValue("Name", user.Name is null ? DBNull.Value : user.Name);
                    cmd.Parameters.AddWithValue("Lastname", user.Lastname is null ? DBNull.Value : user.Lastname);
                    cmd.Parameters.AddWithValue("email", user.email is null ? DBNull.Value : user.email);
                    cmd.Parameters.AddWithValue("password", password is null ? DBNull.Value : password);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            err = Convert.ToInt32(reader["Error"].ToString());
                            if (err == 1)
                            {
                                message = reader["Respuesta"].ToString();
                                reader.Close();
                                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Hubo un error al actualizar el usuario", Error = err });
                            }
                            else
                            {
                                reader.Close();
                                return StatusCode(StatusCodes.Status200OK, new { Message = "Usuario actualizado correctamente" });
                            }
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Hubo un error al actualizar el usuario" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex, Message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("delete/{id:int}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            try
            {
                using (var conection = new SqlConnection(cadenaSQL))
                {
                    conection.Open();
                    string? message = null;
                    int err = 0;
                    var cmd = new SqlCommand("Delete_usr", conection);
                    cmd.Parameters.AddWithValue("Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            err = Convert.ToInt32(reader["Error"].ToString());
                            if (err == 1)
                            {
                                message = reader["Respuesta"].ToString();
                                reader.Close();
                                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Hubo un error al eliminar el usuario", Error = err });
                            }
                            else
                            {
                                reader.Close();
                                return StatusCode(StatusCodes.Status200OK, new { Message = "Usuario eliminado correctamente" });
                            }
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Hubo un error al eliminar el usuario" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex, Message = ex.Message });
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        [Authorize]
        public IActionResult Get(int id)
        {
            List<User> user = new List<User>();
            try
            {
                using (var conection = new SqlConnection(cadenaSQL))
                {
                    conection.Open();
                    var cmd = new SqlCommand("Select_usr", conection);
                    cmd.Parameters.AddWithValue("Id", id);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user.Add(new User() {
                                Name = reader["Name"].ToString(),
                                Lastname = reader["Lastname"].ToString(),
                                email = reader["email"].ToString()
                            });
                            return Ok(user);
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Hubo un error al eliminar el usuario" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex, Message = ex.Message });
            }
        }
    }
}
