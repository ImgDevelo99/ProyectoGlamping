using EcoGlam.DataBase;
using EcoGlam.Models;
using EcoGlam.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EcoGlam.Controllers
{
    public class ServiciosController : Controller
    {
        private readonly EcoGlamContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ServiciosController(EcoGlamContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Policy = "ListarServiciosPermiso")]
        public IActionResult Index()
        {
            var servicio = ObtenerTodosLosServicios();

            return View(servicio);
        }

        [Authorize(Policy = "CrearServiciosPermiso")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View(CargarDatosIniciales());
        }

        [Authorize(Policy = "CrearServiciosPermiso")]
        [HttpPost]
        public IActionResult Crear(Servicio oServicio, List<IFormFile> Imagenes)
        {

            if (!ModelState.IsValid)
            {
                return View(CargarDatosIniciales());
            }

            if (Existe(oServicio.NomServicio))
            {
                ModelState.AddModelError("oServicio.NomServicio", "El servicio ya existe");
                return View(CargarDatosIniciales());
            }

            if (Imagenes.IsNullOrEmpty())
            {
                ModelState.AddModelError("imagenServicio", "Agregue al menos una imagen del servicio");
                return View(CargarDatosIniciales());
            }

            if (oServicio.Costo == 0)
            {
                ModelState.AddModelError("oServicio.Costo", "El costo no puede estar vacio");
                return View(CargarDatosIniciales());
            }


            _context.Servicios.Add(oServicio);

            _context.SaveChanges();

            foreach (var imagenes in Imagenes)
            {
                if(imagenes != null && imagenes.Length > 0)
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

                        var imagenServicio = new ImagenServicio
                        {
                            IdImagen = imagen.IdImagen,
                            IdServicio = oServicio.IdServicio
                        };

                        _context.ImagenServicios.Add(imagenServicio);
                        _context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index","Servicios");
        }

        [Authorize(Policy = "EditarServiciosPermiso")]
        [HttpGet]
        public IActionResult Editar(int idServicio)
        {
            ViewBag.Imagenes = ObtenerImagenesAsociadas(idServicio);

            return View(CargarDatosEditar(idServicio));
        }

        [Authorize(Policy = "EditarServiciosPermiso")]
        [HttpPost]
        public IActionResult Editar(Servicio oServicio, List<IFormFile> Imagenes)
        {

            ViewBag.Imagenes = ObtenerImagenesAsociadas(oServicio.IdServicio);

            if (!ModelState.IsValid)
            {
                return View(CargarDatosEditar(oServicio.IdServicio));
            }

            if (Existe(oServicio))
            {
                ModelState.AddModelError("oServicio.NomServicio", "El servicio ya existe");
                return View(CargarDatosEditar(oServicio.IdServicio));
            }

            if (oServicio.Costo == 0)
            {
                ModelState.AddModelError("oServicio.Costo", "El costo no puede estar vacio");
                return View(CargarDatosEditar(oServicio.IdServicio));
            }


            if (Imagenes.Count != 0)
            {
                var idsImagenes = _context.ImagenServicios
                    .Where(ims => ims.IdServicio == oServicio.IdServicio)
                    .Select(ims => ims.IdImagen)
                    .ToList();

                var imagenesServicio = _context.ImagenServicios
                    .Where(ims => ims.IdServicio == oServicio.IdServicio);

                _context.ImagenServicios.RemoveRange(imagenesServicio);

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

                            var imagenServicio = new ImagenServicio
                            {
                                IdImagen = imagen.IdImagen,
                                IdServicio = oServicio.IdServicio
                            };

                            _context.ImagenServicios.Add(imagenServicio);
                            _context.SaveChanges();
                        }
                    }
                }
            }

            _context.Servicios.Update(oServicio);
            _context.SaveChanges();

            var paqueteServicioLista = _context.PaqueteServicios
                .Where(ps => ps.IdServicio == oServicio.IdServicio)
                .ToList();

            if(paqueteServicioLista.Count != 0)
            {
                foreach(var paqueteServicio in paqueteServicioLista)
                {
                    if(paqueteServicio.Costo != oServicio.Costo)
                    {
                        paqueteServicio.Costo = oServicio.Costo;

                        _context.PaqueteServicios.Update(paqueteServicio);
                    }
                }
            }

            var paqueteLista = _context.PaqueteServicios
                .Where(ps => ps.IdServicio == oServicio.IdServicio)
                .Select(ps => ps.oPaquete)
                .ToList();

            if (paqueteLista.Count != 0)
            {
                foreach (var paquete in paqueteLista)
                {
                    var costoHabitacion = _context.Paquetes
                        .Where(p => p.IdPaquete == paquete.IdPaquete)
                        .Select(p => p.oHabitacion.Costo)
                        .FirstOrDefault();

                    var serviciosPaquete = _context.PaqueteServicios.AsNoTracking()
                        .Where(ps => ps.IdPaquete == paquete.IdPaquete)
                        .Select(ps => ps.oServicio)
                        .ToList();

                    double? serviciosCosto = 0;

                    foreach(var servicio in serviciosPaquete)
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

        [Authorize(Policy = "VerDetallesServiciosPermiso")]
        [HttpGet]
        public IActionResult Detalles(int idServicio)
        {

            ViewBag.ImagenesAsociadas = _context.ImagenServicios
                .Where(ims => ims.IdServicio == idServicio)
                .Select(imh => imh.oImagen.UrlImagen)
                .ToList();

            return View(CargarDatosEditar(idServicio));

        }

        [Authorize(Policy = "CambiarEstadoServiciosPermiso")]
        [HttpPost]
        public IActionResult cambiarEstado(int? id, bool estado)
        {

            if (id.HasValue)
            {

                var servicio = _context.Servicios.Find(id.Value);

                if (servicio != null)
                {
                    servicio.Estado = estado;

                    if(estado == false)
                    {

                        var paqueteConServicio = _context.PaqueteServicios
                        .Where(ps => ps.IdServicio == servicio.IdServicio && ps.oPaquete.Estado == true)
                        .Select(ps => ps.oPaquete)
                        .ToList();

                        if (paqueteConServicio != null)
                        {

                            foreach (var paquete in paqueteConServicio)
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

        public IActionResult Buscar(string searchTerm)
        {
            List<Servicio> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = ObtenerTodosLosServicios();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        private List<Servicio> ObtenerTodosLosServicios()
        {
            var todosLosServicios = _context.Servicios
                .Include(m => m.oTipoServicio)
                .ToList();

            return todosLosServicios;
        }

        private List<Servicio> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            var resultados = _context.Servicios
                                  .Include(e => e.oTipoServicio)
                                  .Where(e => e.NomServicio.Contains(searchTerm) || e.oTipoServicio.NombreTipoServicio.Contains(searchTerm))
                                  .ToList();

            return resultados;
        }

        public bool Existe(string nombre)
        {
            return _context.Servicios.Any(ts => ts.NomServicio == nombre);
        }

        public bool Existe(Servicio oServicio) 
        {
            return _context.Servicios.Any(s => s.NomServicio == oServicio.NomServicio && s.IdServicio != oServicio.IdServicio);
        }

        public ServicioVM CargarDatosIniciales()
        {
            var servicioVM = new ServicioVM();

            servicioVM.oListaTipoServicio = _context.TipoServicios
                .Select(tservicio => new SelectListItem()
                {
                    Text = tservicio.NombreTipoServicio,
                    Value = tservicio.IdTipoServicio.ToString()
                })
                .ToList();

            return servicioVM;
        }

        public ServicioVM CargarDatosEditar(int? idServicio)
        {
            ServicioVM oServicioVM = new ServicioVM()
            {
                oServicio = _context.Servicios
                                .Where(s => s.IdServicio == idServicio)
                                .FirstOrDefault(),
                oListaTipoServicio = _context.TipoServicios
                .Select(tservicio => new SelectListItem()
                {
                    Text = tservicio.NombreTipoServicio,
                    Value = tservicio.IdTipoServicio.ToString()
                })
                .ToList()
            };


            return oServicioVM;
        }

        public List<string?> ObtenerImagenesAsociadas(int? idServicio)
        {
            return _context.ImagenServicios
                .Where(im => im.IdServicio == idServicio)
                .Select(im => im.oImagen.UrlImagen)
                .ToList();
        }

        public IActionResult verificarPaquetesAsociados(int idServicio)
        {

            bool respuesta = false;

            var paqueteServicio = _context.PaqueteServicios
                .Where(ps => ps.IdServicio == idServicio && ps.oPaquete.Estado == true)
                .ToList();

            if(paqueteServicio.Count != 0)
            {
                respuesta = true;
            }

            return Json(respuesta);

        }
    }
}