using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class cliente : CamposControl
    {
        public int id_cliente { get; set; }
        public string nombre_cliente { get; set; } = string.Empty;
        public string? telefono_cliente { get; set; }
        public string? direccion_cliente { get; set; }


    }
}
