using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public static class ParametrosSesion
    {
        // Inicializamos con un valor predeterminado, pero se actualizará al iniciar sesión
        public static string UsuarioConectado = sesionUsuario.usuarioConectado?.nombre_usuario_login ?? "UsuarioAPI";

        // ID del usuario conectado (0 si no hay usuario conectado)
        public static int IdUsuarioConectado = sesionUsuario.usuarioConectado?.id_usuario ?? 0;

        // Método para actualizar los datos del usuario cuando inicia sesión
        public static void ActualizarUsuarioConectado()
        {
            if (sesionUsuario.usuarioConectado != null)
            {
                UsuarioConectado = sesionUsuario.usuarioConectado.nombre_usuario_login;
                IdUsuarioConectado = sesionUsuario.usuarioConectado.id_usuario;
            }
            else
            {
                UsuarioConectado = "UsuarioAPI";
                IdUsuarioConectado = 0;
            }
        }
    }
}
