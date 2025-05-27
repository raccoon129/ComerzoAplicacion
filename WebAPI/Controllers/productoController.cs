using COMMON.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class productoController : GenericController<producto>
    {
        public productoController() : base(Parametros.FabricaRepository.ProductoRepository())
        {
        }
    }
}
