using COMMON.Entidades;
using COMMON.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class usuarioController : GenericController<usuario>
    {
        public usuarioController() : base(Parametros.FabricaRepository.UsuarioRepository())
        {
        }

        [HttpPost("login")]
        public ActionResult<usuario> Login([FromBody] LoginModel credenciales)
        {
            try
            {
                var parametros = new Dictionary<string, string>
                {
                    { "p_nombre_usuario", credenciales.nombre_usuario_login },
                    { "p_clave", credenciales.clave_usuario }
                };

                var resultado = _repositorio.EjecutarProcedimiento<usuario>("sp_validar_usuario", parametros).FirstOrDefault();

                if (resultado != null)
                {
                    return Ok(resultado);
                }
                return Unauthorized("Credenciales inválidas");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error en login: {ex.Message}");
            }
        }
    }
}
