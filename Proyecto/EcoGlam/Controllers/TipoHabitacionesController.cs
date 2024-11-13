using EcoGlam.DataBase;
using EcoGlam.Models.ViewModels;
using EcoGlam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EcoGlam.Controllers
{
    public class TipoHabitacionesController : Controller
    {

        private readonly EcoGlamContext _context;

        public TipoHabitacionesController(EcoGlamContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "ListarTipoHabitacionesPermiso")]
        public IActionResult Index()
        {
            var tipoHabitaciones = ObtenerTodosLosTipoHabitaciones();
            return View(tipoHabitaciones);
        }

        [Authorize(Policy = "CrearTipoHabitacionesPermiso")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View(CargarDatosIniciales());
        }

        [Authorize(Policy = "CrearTipoHabitacionesPermiso")]
        [HttpPost]
        public IActionResult Crear(TipoHabitacione oTipoHabitacion)
        {

            if (Existe(oTipoHabitacion.NomTipoHabitacion))
            {
                ModelState.AddModelError("oTipoHabitacion.NomTipoHabitacion", "El tipo de habitacion ya existe");
                return View(CargarDatosIniciales());
            }

            if (!ModelState.IsValid)
            {
                return View(CargarDatosIniciales());
            }

            _context.TipoHabitaciones.Add(oTipoHabitacion);

            _context.SaveChanges();

            return RedirectToAction("Index", "TipoHabitaciones");
        }

        [Authorize(Policy = "EditarTipoHabitacionesPermiso")]
        [HttpGet]
        public IActionResult Editar(int idTipoHabitacion)
        {
            return View(CargarDatosEditar(idTipoHabitacion));
        }

        [Authorize(Policy = "EditarTipoHabitacionesPermiso")]
        [HttpPost]
        public IActionResult Editar(TipoHabitacione oTipoHabitacion)
        {

            if (!ModelState.IsValid)
            {
                return View(CargarDatosEditar(oTipoHabitacion.IdTipoHabitacion));
            }

            if (Existe(oTipoHabitacion))
            {
                ModelState.AddModelError("oTipoHabitacion.NomTipoHabitacion", "El servicio ya existe");
                return View(CargarDatosEditar(oTipoHabitacion.IdTipoHabitacion));
            }

            _context.TipoHabitaciones.Update(oTipoHabitacion);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "CambiarEstadoTipoHabitacionesPermiso")]
        [HttpPost]
        public IActionResult cambiarEstado(int? id, bool estado)
        {

            if (id.HasValue)
            {

                var tipoHabitacion = _context.TipoHabitaciones.Find(id.Value);

                if(tipoHabitacion != null)
                {
                    tipoHabitacion.Estado = estado;

                    if(estado == false)
                    {

                        var habitacionConTipoHabi = _context.Habitaciones
                            .Where(h => h.IdTipoHabitacion == tipoHabitacion.IdTipoHabitacion && h.Estado == true)
                            .ToList();

                        if(habitacionConTipoHabi != null)
                        {

                            foreach(var habitacion in habitacionConTipoHabi)
                            {
                                habitacion.Estado = false;

                                var paqueteConHabitacion = _context.Paquetes
                                    .Where(p => p.IdHabitacion == habitacion.IdHabitacion && p.Estado == true)
                                    .ToList();

                                if(paqueteConHabitacion != null)
                                {
                                    foreach(var paquete in paqueteConHabitacion)
                                    {
                                        paquete.Estado = false;
                                    }
                                }
                            }

                        }

                    }

                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }

            }

            return NotFound();

        }

        //Metodos

        public TipoHabitacione CargarDatosIniciales()
        {
            TipoHabitacione oTipoHabitacion = new TipoHabitacione();


            return oTipoHabitacion;
        }

        public TipoHabitacione CargarDatosEditar(int? idTipoHabitacion)
        {
            TipoHabitacione oTipoHabitacion = _context.TipoHabitaciones.Find(idTipoHabitacion);

            return oTipoHabitacion;
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<TipoHabitacione> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = ObtenerTodosLosTipoHabitaciones();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        private List<TipoHabitacione> ObtenerTodosLosTipoHabitaciones()
        {
            var todosLosTiposHabitaciones = _context.TipoHabitaciones
                .ToList();

            return todosLosTiposHabitaciones;
        }

        private List<TipoHabitacione> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            var resultados = _context.TipoHabitaciones
                                  .Where(e => e.NomTipoHabitacion.Contains(searchTerm))
                                  .ToList();

            return resultados;
        }

        public bool Existe(string nombre)
        {
            return _context.TipoHabitaciones.Any(ts => ts.NomTipoHabitacion == nombre);
        }

        public bool Existe(TipoHabitacione oTipoHabitacion)
        {
            return _context.TipoHabitaciones.Any(th => th.NomTipoHabitacion == oTipoHabitacion.NomTipoHabitacion && th.IdTipoHabitacion != oTipoHabitacion.IdTipoHabitacion);
        }

        public IActionResult verificarHabitacionAsociada(int idTipoHabitacion)
        {

            bool respuesta = false;

            var habitacionTipoHabi = _context.Habitaciones
                .Where(ht => ht.IdTipoHabitacion == idTipoHabitacion && ht.Estado == true)
                .ToList();
            
            if(habitacionTipoHabi.Count != 0)
            {
                respuesta = true;
            }

            return Json(respuesta);

        }

    }
}
