using COMMON.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI;

namespace WebApi.Controllers
{
    [Route("api/informacion_comercio")]
    [ApiController]
    public class informacionComercioController : GenericController<informacion_comercio>
    {
        public informacionComercioController() : base(Parametros.FabricaRepository.InformacionComercioRepository())
        {
        }
    }
}