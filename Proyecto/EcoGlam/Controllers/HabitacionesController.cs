using EcoGlam.DataBase;
using EcoGlam.Models.ViewModels;
using EcoGlam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace EcoGlam.Controllers
{
    
    public class HabitacionesController : Controller
    {

        private readonly EcoGlamContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public HabitacionesController(EcoGlamContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Policy = "ListarHabitacionesPermiso")]
        public async Task<IActionResult> Index()
        {
            var Habitaciones = _context.Habitaciones
                .Include(m => m.oTipoHabitacion)
                .ToListAsync();

            return View(await Habitaciones);
        }

        [Authorize(Policy = "CrearHabitacionesPermiso")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View(CargarDatosIniciales());
        }

        [Authorize(Policy = "CrearHabitacionesPermiso")]
        [HttpPost]
        public IActionResult Crear(Habitacione oHabitacion, List<IFormFile> Imagenes)
        {
            if (Existe(oHabitacion.Nombre))
            {
                ModelState.AddModelError("oHabitacion.Nombre", "La habitacion ya existe");
                return View(CargarDatosIniciales());
            }

            if (Imagenes.IsNullOrEmpty())
            {
                ModelState.AddModelError("imagenHabitacion", "Agregue al menos una imagen de la habitacion");
                return View(CargarDatosIniciales());
            }

            if (oHabitacion.Costo == 0)
            {
                ModelState.AddModelError("oHabitacion.Costo", "El costo no puede estar vacio");
                return View(CargarDatosIniciales());
            }

            if (!ModelState.IsValid)
            {
                return View(CargarDatosIniciales());
            }


            _context.Habitaciones.Add(oHabitacion);

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

                        var imagenHabitacion = new ImagenHabitacion
                        {
                            IdImagen = imagen.IdImagen,
                            IdHabitacion = oHabitacion.IdHabitacion
                        };

                        _context.ImagenHabitacions.Add(imagenHabitacion);
                        _context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index", "Habitaciones");
        }

        [Authorize(Policy = "EditarHabitacionesPermiso")]
        [HttpGet]
        public IActionResult Editar(int idHabitacion)
        {
            ViewBag.Imagenes = ObtenerImagenesAsociadas(idHabitacion);

            return View(CargarDatosEditar(idHabitacion));
        }

        [Authorize(Policy = "EditarHabitacionesPermiso")]
        [HttpPost]
        public IActionResult Editar(Habitacione oHabitacion, List<IFormFile> Imagenes)
        {
            ViewBag.Imagenes = ObtenerImagenesAsociadas(oHabitacion.IdHabitacion);

            if (!ModelState.IsValid)
                return View(CargarDatosEditar(oHabitacion.IdHabitacion));

            if (Existe(oHabitacion))
            {
                ModelState.AddModelError("oHabitacion.Nombre", "La habitacion ya existe");
                return View(CargarDatosEditar(oHabitacion.IdHabitacion));
            }

            if (oHabitacion.Costo == 0)
            {
                ModelState.AddModelError("oHabitacion.Costo", "El costo no puede estar vacio");
                return View(CargarDatosEditar(oHabitacion.IdHabitacion));
            }

            if (Imagenes.Count != 0)
            {
                var idsImagenes = _context.ImagenHabitacions
                    .Where(imh => imh.IdHabitacion == oHabitacion.IdHabitacion)
                    .Select(imh => imh.IdImagen)
                    .ToList();

                var imagenesHabitaciones = _context.ImagenHabitacions
                    .Where(imh => imh.IdHabitacion == oHabitacion.IdHabitacion);

                _context.ImagenHabitacions.RemoveRange(imagenesHabitaciones);

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

                            var imagenHabitaciones = new ImagenHabitacion
                            {
                                IdImagen = imagen.IdImagen,
                                IdHabitacion = oHabitacion.IdHabitacion
                            };

                            _context.ImagenHabitacions.Add(imagenHabitaciones);
                            _context.SaveChanges();
                        }
                    }
                }
            }

            _context.Habitaciones.Update(oHabitacion);
            _context.SaveChanges();

            var PaqueteLista = _context.Paquetes
                .Include(p => p.oHabitacion)
                .Where(p => p.IdHabitacion == oHabitacion.IdHabitacion)
                .ToList();

            if (PaqueteLista.Count != 0)
            {
                foreach(var paquete in PaqueteLista)
                {
                    var costoHabitacion = oHabitacion.Costo;

                    var serviciosPaquete = _context.PaqueteServicios.AsNoTracking()
                        .Where(ps => ps.IdPaquete == paquete.IdPaquete)
                        .Select(ps => ps.oServicio)
                        .ToList();

                    double? serviciosCosto = 0;

                    foreach (var servicio in serviciosPaquete)
                    {
                        serviciosCosto += servicio.Costo;
                    }

                    var nuevoCosto = costoHabitacion + serviciosCosto;

                    paquete.Costo = nuevoCosto;

                    _context.Paquetes.Update(paquete);
                }
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "VerDetallesHabitacionesPermiso")]
        [HttpGet]
        public IActionResult Detalles(int idHabitacion)
        {

            ViewBag.ImagenesAsociadas = _context.ImagenHabitacions
                .Where(imh => imh.IdHabitacion == idHabitacion)
                .Select(imh => imh.oImagen.UrlImagen)
                .ToList();

            return View(CargarDatosEditar(idHabitacion));
        }

        [Authorize(Policy = "CambiarEstadoHabitacionesPermiso")]
        [HttpPost]
        public IActionResult cambiarEstado(int? id, bool estado)
        {

            if (id.HasValue)
            {

                var Habitacion = _context.Habitaciones.Find(id.Value);

                if (Habitacion != null)
                {
                    Habitacion.Estado = estado;

                    if(estado == false)
                    {

                        var paqueteConHabitacion = _context.Paquetes
                        .Where(p => p.IdHabitacion == Habitacion.IdHabitacion && p.Estado == true)
                        .ToList();

                        if (paqueteConHabitacion != null)
                        {

                            foreach (var paquete in paqueteConHabitacion)
                            {
                                paquete.Estado = false;
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

        public List<string?> ObtenerImagenesAsociadas(int? idHabitacion)
        {
            return _context.ImagenHabitacions
                .Where(ih => ih.IdHabitacion == idHabitacion)
                .Select(ih => ih.oImagen.UrlImagen)
                .ToList();
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<Habitacione> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = ObtenerTodasLasHabitaciones();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        private List<Habitacione> ObtenerTodasLasHabitaciones()
        {
            var todasLasHabitaciones = _context.Habitaciones
                .Include(m => m.oTipoHabitacion)
                .ToList();

            return todasLasHabitaciones;
        }

        private List<Habitacione> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            var resultados = _context.Habitaciones
                                  .Include(e => e.oTipoHabitacion)
                                  .Where(e => e.Nombre.Contains(searchTerm) || e.oTipoHabitacion.NomTipoHabitacion.Contains(searchTerm))
                                  .ToList();

            return resultados;
        }

        public bool Existe(string nombre)
        {
            return _context.Habitaciones.Any(ts => ts.Nombre == nombre);
        }

        public bool Existe(Habitacione oHabitacion)
        {
            return _context.Habitaciones.Any(h => h.Nombre == oHabitacion.Nombre && h.IdHabitacion != oHabitacion.IdHabitacion);
        }

        public HabitacionVM CargarDatosEditar(int? idHabitacion)
        {
            HabitacionVM oHabitacionVM = new HabitacionVM()
            {
                oHabitacion = _context.Habitaciones
                                .Where(h => h.IdHabitacion == idHabitacion)
                                .FirstOrDefault(),
                oListaTipoHabitaciones = _context.TipoHabitaciones
                .Select(thabitacion => new SelectListItem()
                {
                    Text = thabitacion.NomTipoHabitacion,
                    Value = thabitacion.IdTipoHabitacion.ToString()
                })
                .ToList()
            };


            return oHabitacionVM;
        }

        public HabitacionVM CargarDatosIniciales()
        {
            var habitacionVM = new HabitacionVM();

            habitacionVM.oListaTipoHabitaciones = _context.TipoHabitaciones
                .Where(h => h.Estado == true)
                .Select(thabitaciones => new SelectListItem()
                {
                    Text = thabitaciones.NomTipoHabitacion,
                    Value = thabitaciones.IdTipoHabitacion.ToString()
                })
                .ToList();

            return habitacionVM;
        }

        public IActionResult verificarPaquetesAsociados(int idHabitacion)
        {

            bool respuesta = false;

            var paqueteHabitacion = _context.Paquetes
                .Where(p => p.IdHabitacion == idHabitacion && p.Estado == true)
                .ToList();

            if(paqueteHabitacion.Count > 0)
            {
                respuesta = true;
            }

            return Json(respuesta);

        }

        public IActionResult validarCambioEstado(int idHabitacion, bool estado)
        {
            if(estado)
            {
                bool respuestaTipoHabitacion = false;

                var tipoHabitacion = _context.Habitaciones
                    .FirstOrDefault(h => h.IdHabitacion == idHabitacion && h.oTipoHabitacion.Estado == false);

                if(tipoHabitacion != null)
                {

                    respuestaTipoHabitacion = true;

                }

                return Json(new
                {
                    tipoHabitacion = respuestaTipoHabitacion
                });
            }

            return NotFound();
        }
    }
}
