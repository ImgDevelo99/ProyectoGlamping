using EcoGlam.DataBase;
using EcoGlam.Models.ViewModels;
using EcoGlam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace EcoGlam.Controllers
{
    public class PaquetesController : Controller
    {
        private readonly EcoGlamContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public PaquetesController(EcoGlamContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Policy = "ListarPaquetesPermiso")]
        public IActionResult Index()
        {
            var paquetes = ObtenerTodosLosPaquetes();
            return View(paquetes);
        }

        [Authorize(Policy = "CrearPaquetesPermiso")]
        [HttpGet]
        public IActionResult Crear()
        {
            ViewBag.ServiciosDisponibles = ObtenerServiciosDisponibles();

            return View(CargarDatosIniciales());
        }

        [Authorize(Policy = "CrearPaquetesPermiso")]
        [HttpPost]
        public IActionResult Crear(Paquete oPaquete, string serviciosSeleccionados, List<IFormFile> Imagenes)
        {

            if (serviciosSeleccionados.IsNullOrEmpty())
            {
                ViewData["Error"] = "True";
                ViewData["ErrorServicio"] = "True";
                ViewBag.ServiciosDisponibles = ObtenerServiciosDisponibles();

                ModelState.AddModelError("serviciosSeleccionados", "Selecciona al menos un servicio");
                return View(CargarDatosIniciales());
            }

            if (Imagenes.IsNullOrEmpty())
            {
                ViewData["Error"] = "True";
                ViewBag.ServiciosDisponibles = ObtenerServiciosDisponibles();

                ModelState.AddModelError("imagenPaquete", "Agregue al menos una imagen del paquete");
                return View(CargarDatosIniciales());
            }

            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "True";
                ViewBag.ServiciosDisponibles = ObtenerServiciosDisponibles();

                return View(CargarDatosIniciales());
            }

            if (Existe(oPaquete.NomPaquete))
            {
                ViewData["Error"] = "True";
                ViewBag.ServiciosDisponibles = ObtenerServiciosDisponibles();

                ModelState.AddModelError("oPaquete.NomPaquete", "El paquete ya existe");
                return View(CargarDatosIniciales());
            }

            if (oPaquete.Costo == 0)
            {
                ViewData["Error"] = "True";
                ViewBag.ServiciosDisponibles = ObtenerServiciosDisponibles();

                ModelState.AddModelError("oPaquete.Costo", "El costo no puede estar vacio");
                return View(CargarDatosIniciales());
            }

            _context.Paquetes.Add(oPaquete);
            _context.SaveChanges();

            
            var listaServiciosSeleccionados = JsonConvert.DeserializeObject<List<dynamic>>(serviciosSeleccionados.ToString());

            if (listaServiciosSeleccionados != null && listaServiciosSeleccionados.Any())
            {
                var servicios = listaServiciosSeleccionados.Select(servicio => new Servicio
                {
                    IdServicio = Convert.ToInt32(servicio.id),
                    NomServicio = servicio.nombre.ToString(),
                    Costo = Convert.ToDouble(servicio.costo)
                }).ToList();

                foreach (var servicio in servicios)
                {
                    var paqueteServicio = new PaqueteServicio
                    {
                        IdPaquete = oPaquete.IdPaquete,
                        IdServicio = servicio.IdServicio,
                        Costo = servicio.Costo
                    };
                    _context.PaqueteServicios.Add(paqueteServicio);
                }
            }

            _context.SaveChanges();

            foreach (var imagenes in Imagenes)
            {
                if (imagenes != null && imagenes.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        imagenes.CopyTo(stream);

                        var webRootPath = _webHostEnvironment.WebRootPath;
                        var nuevoNombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(imagenes.FileName)}";

                        var imagePath = Path.Combine(webRootPath, "imagenes", nuevoNombreArchivo);
                        System.IO.File.WriteAllBytes(imagePath, stream.ToArray());

                        var imagen = new Imagene
                        {
                            UrlImagen = $"/imagenes/{nuevoNombreArchivo}"
                        };

                        _context.Imagenes.Add(imagen);
                        _context.SaveChanges();

                        var imagenPaquete = new ImagenPaquete
                        {
                            IdImagen = imagen.IdImagen,
                            IdPaquete = oPaquete.IdPaquete
                        };

                        _context.ImagenPaquetes.Add(imagenPaquete);
                        _context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index", "Paquetes");

        }

        [Authorize(Policy = "EditarPaquetesPermiso")]
        [HttpGet]
        public IActionResult Editar(int idPaquete)
        {
            ViewBag.ServiciosAsociados = ObtenerServiciosAsociados(idPaquete);
            ViewBag.ServiciosDisponibles = ObtenerServiciosDisponibles();
            ViewBag.ImagenesAsociadas = ObtenerImagenesAsociadas(idPaquete);

            return View(CargarDatosEditar(idPaquete));
        }

        [Authorize(Policy = "EditarPaquetesPermiso")]
        [HttpPost]
        public IActionResult Editar(Paquete oPaquete, string serviciosSeleccionados, List<IFormFile> Imagenes)
        {
            ViewBag.ServiciosAsociados = ObtenerServiciosAsociados(oPaquete.IdPaquete);
            ViewBag.ServiciosDisponibles = ObtenerServiciosDisponibles();
            ViewBag.ImagenesAsociadas = ObtenerImagenesAsociadas(oPaquete.IdPaquete);

            if (!ModelState.IsValid)
            {
                return View(CargarDatosEditar(oPaquete.IdPaquete));
            }

            if (Existe(oPaquete))
            {
                ModelState.AddModelError("oPaquete.NomPaquete", "El paquete ya existe");
                return View(CargarDatosEditar(oPaquete.IdPaquete));
            }

            if (oPaquete.Costo == 0)
            {
                ModelState.AddModelError("oPaquete.Costo", "El costo no puede estar vacio");
                return View(CargarDatosEditar(oPaquete.IdPaquete));
            }

            if(Imagenes.Count != 0)
            {
                var idsImagenes = _context.ImagenPaquetes
                    .Where(imp => imp.IdPaquete == oPaquete.IdPaquete)
                    .Select(imp => imp.IdImagen)
                    .ToList();

                var imagenesPaquetes = _context.ImagenPaquetes
                    .Where(imp => imp.IdPaquete == oPaquete.IdPaquete);

                _context.ImagenPaquetes.RemoveRange(imagenesPaquetes);

                foreach (var idImagen in idsImagenes)
                {
                    var imagen = _context.Imagenes.Find(idImagen);
                    if (imagen != null)
                    {
                        _context.Imagenes.Remove(imagen);

                        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imagen.UrlImagen.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                foreach (var imagenes in Imagenes)
                {
                    if (imagenes != null && imagenes.Length > 0)
                    {
                        using (var stream = new MemoryStream())
                        {
                            imagenes.CopyTo(stream);

                            var webRootPath = _webHostEnvironment.WebRootPath;
                            var nuevoNombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(imagenes.FileName)}";

                            var imagePath = Path.Combine(webRootPath, "imagenes", nuevoNombreArchivo);
                            System.IO.File.WriteAllBytes(imagePath, stream.ToArray());

                            var imagen = new Imagene
                            {
                                UrlImagen = $"/imagenes/{nuevoNombreArchivo}"
                            };

                            _context.Imagenes.Add(imagen);
                            _context.SaveChanges();

                            var imagenPaquetes = new ImagenPaquete
                            {
                                IdImagen = imagen.IdImagen,
                                IdPaquete = oPaquete.IdPaquete
                            };

                            _context.ImagenPaquetes.Add(imagenPaquetes);
                            _context.SaveChanges();
                        }
                    }
                }
            }

            var listaServiciosSeleccionados = JsonConvert.DeserializeObject<List<dynamic>>(serviciosSeleccionados.ToString());

            if (listaServiciosSeleccionados != null && listaServiciosSeleccionados.Any())
            {
                var serviciosNuevos = listaServiciosSeleccionados.Select(servicio => new Servicio
                {
                    IdServicio = Convert.ToInt32(servicio.id),
                    NomServicio = servicio.nombre.ToString(),
                    Costo = Convert.ToDouble(servicio.costo)
                }).ToList();

                var serviciosOriginales = _context.PaqueteServicios
                    .Where(ps => ps.IdPaquete == oPaquete.IdPaquete)
                    .Select(ps => ps.IdServicio)
                    .ToList();

                var serviciosAEliminar = serviciosOriginales.Except(serviciosNuevos.Select(s => s.IdServicio)).ToList();

                foreach(var servicioEliminar in serviciosAEliminar)
                {
                    var paqueteServicioEliminar = _context.PaqueteServicios
                        .FirstOrDefault(ps => ps.IdPaquete == oPaquete.IdPaquete && ps.IdServicio == servicioEliminar);

                    if(paqueteServicioEliminar != null)
                    {
                        _context.PaqueteServicios.Remove(paqueteServicioEliminar);
                    }
                }

                var serviciosAAgregar = serviciosNuevos.Select(s => s.IdServicio).Except(serviciosOriginales).ToList();
                foreach(var servicioAgregar in serviciosAAgregar)
                {
                    var paqueteServicio = new PaqueteServicio
                    {
                        IdPaquete = oPaquete.IdPaquete,
                        IdServicio = servicioAgregar,
                        Costo = _context.Servicios
                                    .Where(s => s.IdServicio == servicioAgregar)
                                    .Select(s => s.Costo)
                                    .FirstOrDefault()
                    };

                    _context.PaqueteServicios.Add(paqueteServicio);
                }
            }

            _context.Paquetes.Update(oPaquete);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "VerDetallesPaquetesPermiso")]
        [HttpGet]
        public IActionResult Detalles(int idPaquete)
        {
            ViewBag.ServiciosAsociados = ObtenerServiciosAsociados(idPaquete);
            ViewBag.ImagenesAsociadas = ObtenerImagenesAsociadas(idPaquete);

            return View(CargarDatosEditar(idPaquete));
        }

        [Authorize(Policy = "CambiarEstadoPaquetesPermiso")]
        [HttpPost]
        public IActionResult cambiarEstado(int? id, bool estado)
        {

            if (id.HasValue)
            {

                var paquete = _context.Paquetes.Find(id.Value);

                if (paquete != null)
                {
                    paquete.Estado = estado;
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }

            }

            return NotFound();

        }

        //Metodos

        public PaqueteVM CargarDatosIniciales()
        {
            var paqueteVM = new PaqueteVM();

            paqueteVM.oListaHabitaciones = _context.Habitaciones.Where(h => h.Estado == true).Select(habitaciones => new SelectListItem()
            {
                Text = habitaciones.Nombre,
                Value = habitaciones.IdHabitacion.ToString()
            }).ToList();

            paqueteVM.oListaServiciosSeleccionados = new List<Servicio>();

            return paqueteVM;
        }

        public PaqueteVM CargarDatosEditar(int? idPaquete)
        {
            var oPaqueteVM = new PaqueteVM()
            {
                oPaquete = _context.Paquetes
                            .Where(p => p.IdPaquete == idPaquete)
                            .FirstOrDefault(),
                oListaHabitaciones = _context.Habitaciones.Select(habitaciones => new SelectListItem()
                {
                    Text = habitaciones.Nombre,
                    Value = habitaciones.IdHabitacion.ToString()
                }).ToList(),
                oListaServiciosSeleccionados = new List<Servicio>()
            };

            return oPaqueteVM;
        }

        public List<Servicio> ObtenerServiciosDisponibles()
        {
            return _context.Servicios.Where(s => s.Estado == true && (s.IdServicio == 1 || s.IdServicio == 2 || s.IdServicio == 3))
                .ToList();
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<Paquete> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = ObtenerTodosLosPaquetes();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        private List<Paquete> ObtenerTodosLosPaquetes()
        {
            var todosLosPaquetes = _context.Paquetes
                .Include(m => m.oHabitacion)
                .ToList();

            return todosLosPaquetes;
        }

        private List<Paquete> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            var resultados = _context.Paquetes
                                  .Include(e => e.oHabitacion)
                                  .Where(e => e.NomPaquete.Contains(searchTerm) || e.oHabitacion.Nombre.Contains(searchTerm))
                                  .ToList();

            return resultados;
        }

        public IActionResult ObtenerCostoServicio(int servicioId)
        {
            var costoServicio = _context.Servicios
                                        .Where(s => s.IdServicio == servicioId)
                                        .Select(s => s.Costo)
                                        .FirstOrDefault();

            return Json(new { costo = costoServicio });
        }

        public IActionResult ObtenerCostoHabitacion(int habitacionId)
        {
            var costoHabitacion = _context.Habitaciones
                                        .Where(s => s.IdHabitacion == habitacionId)
                                        .Select(s => s.Costo)
                                        .FirstOrDefault();

            return Json(new { costo = costoHabitacion });
        }

        public bool Existe(string nombre)
        {
            return _context.Paquetes.Any(ts => ts.NomPaquete == nombre);
        }

        public bool Existe(Paquete oPaquete)
        {
            return _context.Paquetes.Any(p => p.NomPaquete == oPaquete.NomPaquete && p.IdPaquete != oPaquete.IdPaquete);
        }

        public List<string?> ObtenerImagenesAsociadas(int? idPaquete)
        {
            return _context.ImagenPaquetes
                .Where(ip => ip.IdPaquete == idPaquete)
                .Select(ip => ip.oImagen.UrlImagen)
                .ToList();  
        }

        public List<Servicio> ObtenerServiciosAsociados(int? idPaquete)
        {
            return _context.PaqueteServicios
                                        .Where(ps => ps.IdPaquete == idPaquete)
                                        .Select(ps => ps.oServicio)
                                        .ToList();
        }

        public IActionResult validarCambioEstado(int idPaquete, bool estado)
        {

            if (estado)
            {
                bool respuestaServicio = false;
                bool respuestaHabitacion = false;

                var ServiciosPaquete = _context.PaqueteServicios
                    .Where(ps => ps.IdPaquete == idPaquete && ps.oServicio.Estado == false)
                    .Select(ps => ps.oServicio)
                    .ToList();

                var HabitacionPaquete = _context.Paquetes
                    .Where(p => p.IdPaquete == idPaquete && p.oHabitacion.Estado == false)
                    .FirstOrDefault();

                if(ServiciosPaquete.Count != 0)
                {
                    respuestaServicio = true;
                }

                if(HabitacionPaquete != null)
                {
                    respuestaHabitacion = true;
                }

                return Json(new
                {
                    servicios = respuestaServicio,
                    habitacion = respuestaHabitacion
                });
                    
            }

            return Json(new
            {
                servicios = false,
                habitacion = false
            }); 

        }

    }
}
