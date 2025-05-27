using COMMON.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ventaController : GenericController<venta>
    {
        public ventaController() : base(Parametros.FabricaRepository.VentaRepository())
        {
        }
    }

}
