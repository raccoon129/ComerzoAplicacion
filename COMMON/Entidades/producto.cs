using System;
using System.Collections.Generic;

namespace COMMON.Entidades
{
    public class producto : CamposControl
    {
        public int id_producto { get; set; }
        public string nombre_producto { get; set; } = string.Empty;
        public string descripcion_producto { get; set; } = string.Empty;
        public decimal precio_producto { get; set; }
        public string? notas_producto { get; set; }
        public string estado_producto { get; set; } = "activo";
        public int stock_producto { get; set; }
    }
}