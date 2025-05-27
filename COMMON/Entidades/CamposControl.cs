using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public abstract class CamposControl
    {
        public string? usuario_alta { get; set; }
        public string? usuario_mod { get; set; }
        public DateTime fecha_alta { get; set; }
        public DateTime? fecha_mod { get; set; }
    }
}
