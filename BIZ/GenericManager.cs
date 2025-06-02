using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using COMMON;
using COMMON.Entidades;
using FluentValidation;
using Newtonsoft.Json;

namespace BIZ
{
    public abstract class GenericManager<T> where T : CamposControl
    {
        protected HttpClient _httpClient;
        public string Error { get; protected set; }
        AbstractValidator<T> _validator;
        protected GenericManager(AbstractValidator<T> validador)
        {
            _validator = validador;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Params.UrlAPI);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("aplication/json"));
        }

        public async Task<List<T>> ObtenerTodos()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"api/{typeof(T).Name}").ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (response.IsSuccessStatusCode) //Si regreso un 200
                {
                    Error = "";
                    var respuesta = JsonConvert.DeserializeObject<List<T>>(content);
                    return respuesta;
                }
                else
                {
                    Error = content;
                    return null;
                }
            }
            catch (Exception ex)
            {

                Error = ex.Message;
                return null;
            }
        }

        public async Task<T> ObtenerPorId(string id)
        {
            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync($"api/{typeof(T).Name}/{id}").ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (response.IsSuccessStatusCode) //Si regreso un 200
                {
                    Error = "";
                    var respuesta = JsonConvert.DeserializeObject<T>(content);
                    return respuesta;
                }
                else
                {
                    Error = content;
                    return null;
                }
            }
            catch (Exception ex)
            {

                Error = ex.Message;
                return null;
            }
        }

        public async Task<T> ObtenerPorId(int id)
        {
            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync($"api/{typeof(T).Name}/{id}").ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (response.IsSuccessStatusCode) //Si regreso un 200
                {
                    Error = "";
                    var respuesta = JsonConvert.DeserializeObject<T>(content);
                    return respuesta;
                }
                else
                {
                    Error = content;
                    return null;
                }
            }
            catch (Exception ex)
            {

                Error = ex.Message;
                return null;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"api/{typeof(T).Name}/{id}").ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (response.IsSuccessStatusCode) //Si regreso un 200
                {
                    Error = "";
                    return true;
                }
                else
                {
                    Error = content;
                    return false;
                }
            }
            catch (Exception ex)
            {

                Error = ex.Message;
                return false;
            }
        }

        public virtual async Task<T> Agregar(T entidad)
        {
            try
            {
                entidad.usuario_alta = ParametrosSesion.UsuarioConectado;
                entidad.usuario_mod = ParametrosSesion.UsuarioConectado;
                entidad.fecha_alta = DateTime.Now;
                entidad.fecha_mod = DateTime.Now;

                var resultadoValidacion = _validator.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {

                    var c = JsonConvert.SerializeObject(entidad);
                    var body = new StringContent(c, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PostAsync($"api/{typeof(T).Name}", body).ConfigureAwait(false);//
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (response.IsSuccessStatusCode) //Si regreso un 200
                    {
                        Error = "";
                        var respuesta = JsonConvert.DeserializeObject<T>(content);
                        return respuesta;
                    }
                    else
                    {
                        Error = content;
                        return null;
                    }
                }
                else
                {
                    Error = string.Join(",", resultadoValidacion.Errors.Select(e => e.ErrorMessage));
                    return null;
                }
            }
            catch (Exception ex)
            {

                Error = ex.Message;
                return null;
            }
        }

        public async Task<T> Modificar(T entidad)
        {
            try
            {
                entidad.usuario_mod = ParametrosSesion.UsuarioConectado;
                entidad.fecha_mod = DateTime.Now;

                if (entidad.fecha_alta == default)
                    entidad.fecha_alta = DateTime.Now;

                if (entidad.usuario_alta == default)
                    entidad.usuario_alta = ParametrosSesion.UsuarioConectado;


                var resultadoValidacion = _validator.Validate(entidad);
                if (resultadoValidacion.IsValid)
                {
                    var c = JsonConvert.SerializeObject(entidad);
                    var body = new StringContent(c, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PutAsync($"api/{typeof(T).Name}", body).ConfigureAwait(false);
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (response.IsSuccessStatusCode) //Si regreso un 200
                    {
                        Error = "";
                        var respuesta = JsonConvert.DeserializeObject<T>(content);
                        return respuesta;
                    }
                    else
                    {
                        Error = content;
                        return null;
                    }
                }
                else
                {
                    Error = string.Join(",", resultadoValidacion.Errors.Select(e => e.ErrorMessage));
                    return null;
                }
            }
            catch (Exception ex)
            {

                Error = ex.Message;
                return null;
            }
        }
    }
}