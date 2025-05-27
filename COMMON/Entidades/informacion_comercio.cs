using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class informacion_comercio : CamposControl
    {
        public string nombre_comercio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime fecha_creacion { get; set; }
        public string? razon_social { get; set; }
        public int encargado { get; set; }

    }
}
