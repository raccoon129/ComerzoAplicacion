using COMMON.Entidades;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class InformacionComercioManager : GenericManager<informacion_comercio>
    {
        public InformacionComercioManager(AbstractValidator<informacion_comercio> validador) : base(validador)
        {
        }

        /// <summary>
        /// Obtiene la información del comercio, asumiendo que solo hay un registro
        /// </summary>
        /// <returns>Información del comercio o null si no existe</returns>
        public async Task<informacion_comercio> ObtenerInformacionComercio()
        {
            try
            {
                var todos = await ObtenerTodos();
                return todos?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Error = $"Error al obtener información del comercio: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Guarda la información del comercio (crea o actualiza según corresponda)
        /// </summary>
        /// <param name="info">Datos del comercio a guardar</param>
        /// <returns>La información actualizada o null si ocurrió un error</returns>
        public async Task<informacion_comercio> GuardarInformacionComercio(informacion_comercio info)
        {
            try
            {
                var infoExistente = await ObtenerInformacionComercio();

                if (infoExistente == null)
                {
                    // Si no existe información, se crea
                    return await base.Agregar(info);
                }
                else
                {
                    // Si ya existe, se actualiza preservando el ID asignado por la base de datos
                    // Asumiendo que hay un campo id_comercio que es clave primaria
                    // Si hay otra propiedad que funciona como ID, ajusta el código según corresponda

                    // Copiamos los datos al objeto existente
                    infoExistente.nombre_comercio = info.nombre_comercio;
                    infoExistente.descripcion = info.descripcion;
                    infoExistente.fecha_creacion = info.fecha_creacion;
                    infoExistente.razon_social = info.razon_social;
                    infoExistente.encargado = info.encargado;

                    return await base.Modificar(infoExistente);
                }
            }
            catch (Exception ex)
            {
                Error = $"Error al guardar información del comercio: {ex.Message}";
                return null;
            }
        }
    }
}