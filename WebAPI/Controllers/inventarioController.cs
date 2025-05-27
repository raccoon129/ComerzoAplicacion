using COMMON.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class inventarioController : GenericController<inventario>
    {
        public inventarioController() : base(Parametros.FabricaRepository.InventarioRepository())
        {
        }
    }
}
