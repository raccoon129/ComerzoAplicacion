using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class usuario : CamposControl
    {
        public int id_usuario { get; set; }
        public string nombre_usuario_login { get; set; } = string.Empty;
        public string clave_usuario { get; set; } = string.Empty;

    }
}
