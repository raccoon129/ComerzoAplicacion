using COMMON.Validadores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public static class FabricManager
    {
        public static ProductoManager ProductoManager => new ProductoManager(new productoValidator());
        public static ClienteManager ClienteManager => new ClienteManager(new clienteValidator());
        public static UsuarioManager UsuarioManager => new UsuarioManager(new usuarioValidator());
        public static InventarioManager InventarioManager => new InventarioManager(new inventarioValidator());
    }
}
