using COMMON.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using WebAPI;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class clienteController : GenericController<cliente>
    {
        public clienteController() : base(Parametros.FabricaRepository.ClienteRepository())
        {
        }
    }
}
