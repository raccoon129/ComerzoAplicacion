using COMMON.Entidades;
using COMMON.Modelos;
using FluentValidation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class UsuarioManager : GenericManager<usuario>
    {
        public UsuarioManager(AbstractValidator<usuario> validador) : base(validador)
        {
        }


        public async Task<usuario> Login(LoginModel credenciales)
        {
            try
            {
                if (string.IsNullOrEmpty(credenciales.nombre_usuario_login) || string.IsNullOrEmpty(credenciales.clave_usuario))
                {
                    Error = "El nombre de usuario y la contraseña son obligatorios";
                    return null;
                }

                var json = JsonConvert.SerializeObject(credenciales);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/usuario/login", content).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    Error = "";
                    usuario usuarioAutenticado = JsonConvert.DeserializeObject<usuario>(responseContent);
                    return usuarioAutenticado;
                }
                else
                {
                    Error = responseContent;
                    return null;
                }
            }
            catch (Exception ex)
            {
                Error = $"Error durante el inicio de sesión: {ex.Message}";
                return null;
            }
        }

        public override Task<usuario> Agregar(usuario entidad)
        {
            try
            {
                // Validaciones y configuraciones iniciales
                if (string.IsNullOrEmpty(entidad.clave_usuario))
                {
                    Error = "La contraseña no puede estar vacía";
                    return null;
                }

                if (string.IsNullOrEmpty(entidad.nombre_usuario_login))
                {
                    Error = "El nombre de usuario no puede estar vacío";
                    return null;
                }

                return base.Agregar(entidad);
            }
            catch (Exception ex)
            {
                Error = $"Error al agregar usuario: {ex.Message}";
                return null;
            }
        }

    }
}