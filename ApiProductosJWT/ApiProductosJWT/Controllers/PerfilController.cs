using ApiProductosJWT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Files = System.IO.File;

namespace ApiProductosJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("ReglasCors")]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        private readonly string local_path = "D:\\System32\\CSharp\\PRODUCTOS\\ApiProductosJWT\\ApiProductosJWT\\";
        private readonly string cadenaSQL;
        public PerfilController(IConfiguration config)
        {
            cadenaSQL = config.GetConnectionString("CadenaSQL");
        }

        [HttpGet]
        [Route("image/{id:int}")]
        public FileStream GetImage(int id)
        {
            try
            {
                string path = Path.Combine(local_path, "IMAGENES\\");
                string image_name;
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("Select_perfil", conexion);
                    cmd.Parameters.AddWithValue("Id", id);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        rd.Read();
                        image_name = rd["perfil"].ToString();
                        rd.Close();
                    }
                    conexion.Close();
                }
                path += image_name;
                FileStream perfil = System.IO.File.OpenRead(path);
                return perfil;

            }
            catch (Exception err)
            {
                string defaultImg = "IMAGENES\\default.png";
                FileStream perfil = System.IO.File.OpenRead(defaultImg);
                return perfil;
            }
        }


        [HttpPost]
        [Route("upimage/{id}")]
        public async Task<IActionResult> PostImage(string id, [FromForm] Perfil perfil)
        {
            try
            {
                string path = Path.Combine(local_path, "IMAGENES\\");
                int error;
                string Respuesta;
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();
                    var cmd = new SqlCommand("Update_perfil", conexion);
                    cmd.Parameters.AddWithValue("Id", id);
                    cmd.Parameters.AddWithValue("perfil", perfil.Image.FileName);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        rd.Read();
                        error = Convert.ToInt32(rd["Error"].ToString());
                        Respuesta = rd["Respuesta"].ToString();
                        rd.Close();
                    }
                    conexion.Close();
                }
                if (error == 1)
                {
                    throw new Exception(Respuesta);
                }
                else
                {
                    using (FileStream stream = new FileStream(path + perfil.Image.FileName, FileMode.Create))
                    {
                        await perfil.Image.CopyToAsync(stream);
                        return StatusCode(StatusCodes.Status200OK, new { Mensaje = Respuesta });
                    }
                }
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = err.Message, Error = err });
            }
        }

        [HttpDelete]
        [Route("upimage/{id}")]
        public IActionResult DeleteImage(string id)
        {
            try
            {
                string path = Path.Combine(local_path, "IMAGENES\\");
                int error;
                string Image_name;
                string Respuesta;
                using (var conexion = new SqlConnection(cadenaSQL))
                {
                    conexion.Open();

                    var cmd2 = new SqlCommand("Select_perfil", conexion);
                    cmd2.Parameters.AddWithValue("Id", id);
                    cmd2.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader rd2 = cmd2.ExecuteReader())
                    {
                        rd2.Read();
                        Image_name = rd2["perfil"].ToString();
                        rd2.Close();
                    }

                    var cmd = new SqlCommand("Delete_perfil", conexion);
                    cmd.Parameters.AddWithValue("Id", id);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        rd.Read();
                        error = Convert.ToInt32(rd["Error"].ToString());
                        Respuesta = rd["Respuesta"].ToString();
                        rd.Close();
                    }
                    conexion.Close();
                }

                if (error == 1)
                {
                    throw new Exception(Respuesta);
                }
                else
                {
                    Files.Delete(path + Image_name);
                    return StatusCode(StatusCodes.Status200OK, new { Mensaje = Respuesta });
                }
            }
            catch (Exception err)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = err.Message, Error = err });
            }
        }
    }
}
