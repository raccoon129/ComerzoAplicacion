using COMMON.Entidades;
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
    }
}
