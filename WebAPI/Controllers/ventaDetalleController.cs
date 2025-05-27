using COMMON.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ventaDetalleController : GenericController<venta_detalle>
    {
        public ventaDetalleController() : base(Parametros.FabricaRepository.VentaDetalleRepository())
        {

        }
    }
}
