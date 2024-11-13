using EcoGlam.DataBase;
using EcoGlam.Models;
using EcoGlam.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Linq;

using System.Diagnostics;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Previewer;

namespace EcoGlam.Controllers
{
    public class ReservasController : Controller
    {
        private readonly EcoGlamContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReservasController(EcoGlamContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Policy = "ListarReservasPermiso")]
        public async Task<IActionResult> Index()
        {
            var reservas = ObtenerTodasLasReservas();

            return View(reservas);
        }

        [Authorize(Policy = "CrearReservasPermiso")]
        [HttpGet]
        public IActionResult Crear()
        {
            ViewBag.PaquetesDisponibles = ObtenerPaquetesDisponibles();
            ViewBag.ServiciosDisponibles = _context.Servicios.Where(s => s.Estado == true && (s.IdServicio != 1 && s.IdServicio != 2 && s.IdServicio != 3))
                .ToList(); 

            return View(CargarDatosIniciales());

        }

        [Authorize(Policy = "CrearReservasPermiso")]
        [HttpPost]
        public IActionResult Crear(Reserva oReserva, string paqueteSeleccionado, string serviciosSeleccionados)
        {

            ViewBag.PaquetesDisponibles = _context.Paquetes.Where(s => s.Estado == true)
                    .ToList(); ;
            ViewBag.ServiciosDisponibles = _context.Servicios.Where(s => s.Estado == true && (s.IdServicio != 1 && s.IdServicio != 2 && s.IdServicio != 3))
                .ToList();
            ViewData["Error"] = "True";

            if (string.IsNullOrEmpty(paqueteSeleccionado))
            {
                ModelState.AddModelError("paqueteSeleccionados", "Seleccione un paquete");
                return View(CargarDatosIniciales());
            }

            if (string.IsNullOrEmpty(serviciosSeleccionados) || serviciosSeleccionados == "[]")
            {
                ViewData["ErrorServicio"] = "True";
                return View(CargarDatosIniciales());
            }

            if (!ModelState.IsValid )
            {
                return View(CargarDatosIniciales());
            }

            if (!Existe(oReserva.NroDocumentoCliente))
            {
                ModelState.AddModelError("oReserva.NroDocumentoCliente", "El cliente no existe");
                return View(CargarDatosIniciales());
            }

            var cliente = _context.Clientes.FirstOrDefault(c => c.NroDocumento == oReserva.NroDocumentoCliente);

            if (cliente.Estado == false)
            {
                ModelState.AddModelError("oReserva.NroDocumentoCliente", "El cliente esta inhabilitado");
                return View(CargarDatosIniciales());
            }

            if (cliente.Confirmado == false)
            {
                ModelState.AddModelError("oReserva.NroDocumentoCliente", "El cliente no ha confirmado su correo");
                return View(CargarDatosIniciales());
            }

            if (!ValidarFechas(oReserva))
            {
                return View(CargarDatosIniciales());
            }

            if (oReserva.Descuento == null)
            {
                oReserva.Descuento = 0;
            }

            _context.Reservas.Add(oReserva);
            _context.SaveChanges();

            var listaPaqueteSeleccionado = JsonConvert.DeserializeObject<List<dynamic>>(paqueteSeleccionado.ToString());

            if (listaPaqueteSeleccionado != null && listaPaqueteSeleccionado.Any())
            {
                var paquetes = listaPaqueteSeleccionado.Select(paquete => new Paquete
                {
                    IdPaquete = Convert.ToInt32(paquete.id),
                    Costo = Convert.ToDouble(paquete.costo)
                }).ToList();

                foreach (var paquete in paquetes)
                {
                    var DetalleReservaPaquete = new DetalleReservaPaquete
                    {
                        IdReserva = oReserva.IdReserva,
                        IdPaquete = paquete.IdPaquete,
                        Costo = paquete.Costo
                    };
                    _context.DetalleReservaPaquetes.Add(DetalleReservaPaquete);
                }
            }

            if (!string.IsNullOrEmpty(serviciosSeleccionados))
            {
                var listaServiciosSeleccionados = JsonConvert.DeserializeObject<List<dynamic>>(serviciosSeleccionados.ToString());

                if (listaServiciosSeleccionados != null && listaServiciosSeleccionados.Any())
                {
                    var servicios = listaServiciosSeleccionados.Select(servicio => new Servicio
                    {
                        IdServicio = Convert.ToInt32(servicio.id),
                        NomServicio = servicio.nombre.ToString(),
                        Costo = Convert.ToDouble(servicio.costo)
                    }).ToList();

                    for(int i = 0; i < listaServiciosSeleccionados.Count; i++)
                    {
                        if(listaServiciosSeleccionados[i].cantidad == null)
                        {
                            listaServiciosSeleccionados[i].cantidad = 1;
                        }
                            var DetalleReservaServicio = new DetalleReservaServicio
                        {
                            IdReserva = oReserva.IdReserva,
                            IdServicio = listaServiciosSeleccionados[i].id,
                            Costo = listaServiciosSeleccionados[i].costo,
                            Cantidad = listaServiciosSeleccionados[i].cantidad 
                        };
                        _context.DetalleReservaServicios.Add(DetalleReservaServicio);
                    }
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index", "Reservas");

        }

        [Authorize(Policy = "EditarReservasPermiso")]
        [HttpGet]
        public IActionResult Editar(int ReservaId)
        {
            ViewBag.PaqueteAsociado = ObtenerPaqueteAsociado(ReservaId);

            var paqueteAsociado = _context.DetalleReservaPaquetes
                .Where(p => p.IdReserva == ReservaId)
                .Select(p => p.IdPaquete)
                .FirstOrDefault();

            ViewBag.ServiciosAsociados = ObtenerServiciosAsociados(ReservaId);

            ViewBag.CantidadesServiciosAsociados = CantidadesServicios(ReservaId);

            ViewBag.PaquetesDisponibles = _context.Paquetes
                .Where(p => p.IdPaquete != paqueteAsociado && p.Estado == true)
                .ToList();

            ViewBag.ServiciosDisponibles = _context.Servicios.Where(s => s.Estado == true && (s.IdServicio != 1 && s.IdServicio != 2 && s.IdServicio != 3))
                .ToList();


            return View(CargarDatosReserva(ReservaId));

        }

        [Authorize(Policy = "EditarReservasPermiso")]
        [HttpPost]
        public IActionResult Editar(Reserva oReserva, string paqueteSeleccionado, string serviciosSeleccionados)
        {
            ViewBag.PaqueteAsociado = ObtenerPaqueteAsociado(oReserva.IdReserva);

            ViewBag.ServiciosAsociados = ObtenerServiciosAsociados(oReserva.IdReserva);

            ViewBag.CantidadesServiciosAsociados = CantidadesServicios(oReserva.IdReserva);

            var paqueteAsociado = ObtenerPaqueteAsociado(oReserva.IdReserva);

            ViewBag.PaquetesDisponibles = _context.Paquetes
                .Where(p => p.IdPaquete != paqueteAsociado.IdPaquete && p.Estado == true)
                .ToList();

            ViewBag.ServiciosDisponibles = _context.Servicios.Where(s => s.Estado == true && (s.IdServicio != 1 && s.IdServicio != 2 && s.IdServicio != 3))
                .ToList();

            if (!ModelState.IsValid)
            {
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (!Existe(oReserva.NroDocumentoCliente))
            {
                ModelState.AddModelError("oReserva.NroDocumentoCliente", "El cliente no existe");
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (string.IsNullOrEmpty(paqueteSeleccionado))
            {
                ModelState.AddModelError("paqueteSeleccionados", "Este campo es obligatorio");
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (string.IsNullOrEmpty(serviciosSeleccionados) || serviciosSeleccionados == "[]")
            {
                ViewData["ErrorServicio"] = "True";
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (!ValidarFechas(oReserva))
            {
                return View(CargarDatosReserva(oReserva.IdReserva));
            }

            if (oReserva.Descuento == null)
            {
                oReserva.Descuento = 0;
            }

            var listaAbonos = _context.Abonos
                .Where(a => a.IdReserva == oReserva.IdReserva)
                .ToList();

            var costoOriginalReserva = _context.Reservas
                .Where(r => r.IdReserva == oReserva.IdReserva)
                .Select(r => r.MontoTotal)
                .FirstOrDefault();

            if(oReserva.MontoTotal != costoOriginalReserva && listaAbonos.Count != 0)
            {
                foreach(var abono in listaAbonos)
                {
                    var valorDescuento = oReserva.SubTotal * (1 - (oReserva.Descuento / 100));
                    var nuevoPorcentaje = (abono.SubTotal/ valorDescuento) *100;
                    var porcentajeRedondeado = Math.Floor(nuevoPorcentaje.Value);

                    abono.ValorDeuda = valorDescuento;
                    abono.Porcentaje = porcentajeRedondeado;
                }
            }

            if (oReserva.IdEstadoReserva == 6)
            {
                var oAbono = new AbonoController(_context,_webHostEnvironment);
                bool validacion = oAbono.ObtenerPendiente(oReserva.IdReserva) == 0;

                if (!validacion)
                {
                    ViewBag.ErrorFinalizacion = "True";
                    return View(CargarDatosReserva(oReserva.IdReserva));
                }
               
            }

            guardarPaqueteSeleccionado(oReserva, paqueteSeleccionado);
            guardarServiciosSeleccionados(oReserva, serviciosSeleccionados);

            _context.Reservas.Update(oReserva);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "VerDetallesReservasPermiso")]
        public IActionResult Detalles(int ReservaId)
        {
            ViewBag.PaqueteAsociado = ObtenerPaqueteAsociado(ReservaId);

            var paqueteAsociado = _context.DetalleReservaPaquetes
                .Where(p => p.IdReserva == ReservaId)
                .Select(p => p.IdPaquete)
                .FirstOrDefault();

            ViewBag.ServiciosAsociadospaquete = ObtenerServiciosAsociadosPaquete(paqueteAsociado);

            ViewBag.ServiciosAsociados = ObtenerServiciosAsociados(ReservaId);

            ViewBag.CantidadesServiciosAsociados = CantidadesServicios(ReservaId);

            return View(CargarDatosReserva(ReservaId));
        }

        [Authorize(Policy = "AnularReservaPermiso")]
        public IActionResult AnularReserva(int idReserva)
        {

            var reserva = _context.Reservas
                .FirstOrDefault(r => r.IdReserva == idReserva);

            reserva.IdEstadoReserva = 5;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Metodos 

        public ReservaVM CargarDatosReserva(int? idReserva)
        {

            ReservaVM oReservaVM = new ReservaVM()
            {
                oReserva = _context.Reservas
                .Where(r => r.IdReserva == idReserva)
                .FirstOrDefault(),
                oListaEstados = _context.EstadosReservas.Select(reservas => new SelectListItem()
                {
                    Text = reservas.NombreEstadoReserva,
                    Value = reservas.IdEstadoReserva.ToString()
                }).ToList(),
                oListaMetodoPagos = _context.MetodoPagos.Select(reservas => new SelectListItem()
                {
                    Text = reservas.NomMetodoPago,
                    Value = reservas.IdMetodoPago.ToString()
                }).ToList()
            };

            return oReservaVM;
        }

        public ReservaVM CargarDatosIniciales()
        {

            ReservaVM oReservaVM = new ReservaVM()
            {
                oReserva = new Reserva(),
                oListaMetodoPagos = _context.MetodoPagos.Select(reservas => new SelectListItem()
                {
                    Text = reservas.NomMetodoPago,
                    Value = reservas.IdMetodoPago.ToString()
                }).ToList()
            };

            return oReservaVM;
        }

        public bool ValidarFechas(Reserva oReserva)
        {

            if (oReserva.FechaFinalizacion < oReserva.FechaInicio)
            {
                ModelState.AddModelError("oReserva.FechaFinalizacion", "La fecha de finalizacion debe ser posterior a la fecha de inicio");
                return false;
            }
            else if (oReserva.FechaReserva >= oReserva.FechaInicio)
            {
                ModelState.AddModelError("oReserva.FechaInicio", "La fecha de inicio debe ser mayor que la fecha de la reserva");
                return false;
            }

            return true;
        }

        public IActionResult BuscarCliente(string searchTerm)
        {
            var informacionCliente = _context.Clientes
                .Include(s => s.oTipoDocumento)
                .Where(s => s.NroDocumento == searchTerm)
                .FirstOrDefault();

            if (informacionCliente == null)
            {
                return Json(new
                {
                    nombre = "",
                    apellido = "",
                    tipoDocumento = "",
                    correo = "",
                    celular = "",
                });
            }else if (informacionCliente.Estado == false){
                return Json(new
                {
                    estado = "false",
                });
            }
            else
            {
                return Json(new
                {
                    nombre = informacionCliente.Nombres,
                    apellido = informacionCliente.Apellidos,
                    tipoDocumento = informacionCliente.oTipoDocumento.NomTipoDcumento,
                    correo = informacionCliente.Correo,
                    celular = informacionCliente.Celular
                });
            }
        }

        public bool Existe(string cliente)
        {
            return _context.Clientes.Any(c => c.NroDocumento == cliente);
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<Reserva> resultados;

            if (string.IsNullOrEmpty(searchTerm))
            {
                resultados = ObtenerTodasLasReservas();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        public List<int?> CantidadesServicios(int? reservaId)
        {
            var serviciosAsociados = ObtenerServiciosAsociados(reservaId);

            List<int?> listaCantidades = new List<int?>();

            foreach (var servicio in serviciosAsociados)
            {
                var cantidad = _context.DetalleReservaServicios
                    .Where(drs => drs.IdServicio == servicio.IdServicio && drs.IdReserva == reservaId)
                    .Select(drs => drs.Cantidad)
                    .FirstOrDefault();

                listaCantidades.Add(cantidad);
            }

            return listaCantidades;

        }

        public List<Reserva> ObtenerTodasLasReservas()
        {
            var todasLasReservas = _context.Reservas
                .Include(e => e.oEstadoReserva)
                .Include(e => e.oMetodoPago)
                .Include(e => e.oCliente)
                    .ThenInclude(e => e.oTipoDocumento)
                .Include(e => e.oUsuario)
                .Include(e => e.DetalleReservaPaquetes)
                    .ThenInclude(drp => drp.oPaquete)
                        .ThenInclude(p => p.oHabitacion)
                            .ThenInclude(h => h.oTipoHabitacion)
                .ToList();

            return todasLasReservas;
        }

        public List<Reserva> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            if (DateOnly.TryParse(searchTerm, out DateOnly fecha))
            {
                var resultados = _context.Reservas
                    .Include(e => e.oEstadoReserva)
                    .Include(e => e.oMetodoPago)
                    .Include(e => e.oCliente)
                    .Include(e => e.oUsuario)
                    .Where(e => e.FechaReserva == fecha)
                    .ToList();

                return resultados;
            }
            else
            {
                var resultados = _context.Reservas
                    .Include(e => e.oEstadoReserva)
                    .Include(e => e.oMetodoPago)
                    .Include(e => e.oCliente)
                    .Include(e => e.oUsuario)
                    .Where(e => e.NroDocumentoCliente.Contains(searchTerm))
                    .ToList();

                return resultados;
            }
        }

        public Paquete ObtenerPaqueteAsociado(int? reservaId)
        {
            return _context.DetalleReservaPaquetes
                .Where(p => p.IdReserva == reservaId)
                .Select(p => p.oPaquete)
                .FirstOrDefault();
        }

        public List<DetalleReservaServicio> ObtenerServiciosAsociados(int? ReservaId)
        {
            return _context.DetalleReservaServicios
                .Where(drs => drs.IdReserva == ReservaId)
                .Include(drs => drs.oServicio)
                .ToList();
        }

        public List<Servicio> ObtenerServiciosAsociadosPaquete(int? idPaquete)
        {
            return _context.PaqueteServicios
                .Where(p => p.IdPaquete == idPaquete)
                .Select(p => p.oServicio)
                .ToList();
        }

        public List<Paquete> ObtenerPaquetesDisponibles()
        {

            var habitacionesReservadas = _context.DetalleReservaPaquetes
                .Where(drp => drp.oReserva.IdEstadoReserva != 6 && drp.oReserva.IdEstadoReserva != 5) 
                .Select(drp => drp.oPaquete.IdHabitacion) 
                .Distinct()
                .ToList();

            var paquetesDisponibles = _context.Paquetes
                .Where(p => !habitacionesReservadas.Contains(p.IdHabitacion) && p.Estado == true)
                .ToList();

            return paquetesDisponibles;
        }

        public IActionResult ObtenerInfoBasicaPaquete(int reservaId)
        {
            var detalle = _context.DetalleReservaPaquetes
                .Where(p => p.IdReserva == reservaId)
                .FirstOrDefault();

            return Json(new
            {
                costo = detalle.Costo
            });
        }

        public IActionResult ObtenerCostoServicio(int servicioId)
        {
            var costoServicio = _context.Servicios
                                        .Where(s => s.IdServicio == servicioId)
                                        .Select(s => s.Costo)
                                        .FirstOrDefault();

            return Json(new { costo = costoServicio });
        }

        public IActionResult ObtenerInfoPaquete(int paqueteId)
        {
            var Informacionpaquete = _context.Paquetes
                                  .Include(s => s.oHabitacion)
                                    .ThenInclude(s => s.oTipoHabitacion)
                                  .Where(s => s.IdPaquete == paqueteId)
                                  .FirstOrDefault();

            return Json(new
            {
                nombre = Informacionpaquete.NomPaquete,
                costo = Informacionpaquete.Costo,
                habitacion = Informacionpaquete.oHabitacion.Nombre,
                nroPersonas = Informacionpaquete.oHabitacion.oTipoHabitacion.NumeroPersonas,
            });

        }

        public IActionResult ObtenerServiciosPaquete(int paqueteId)
        {
            var serviciosPaquete = _context.PaqueteServicios
                .Where(e => e.IdPaquete == paqueteId)
                .Include(e => e.oServicio)
                .ToList();

            var servicioData = serviciosPaquete.Select(e => new
            {
                nombre = e.oServicio.NomServicio,
                costo = e.Costo
            });

            return Json(servicioData);
        }

        public bool guardarPaqueteSeleccionado(Reserva oReserva, string paqueteSeleccionado)
        {
            var PaqueteSeleccionadoObj = JsonConvert.DeserializeObject<List<dynamic>>(paqueteSeleccionado.ToString());

            if (PaqueteSeleccionadoObj != null && PaqueteSeleccionadoObj.Count > 0)
            {

                var primerPaquete = PaqueteSeleccionadoObj[0];

                var nuevoPaquete = new Paquete()
                {
                    IdPaquete = primerPaquete.id,
                    Costo = primerPaquete.costo
                };

                var paqueteOriginal = _context.DetalleReservaPaquetes
                    .Where(drp => drp.IdReserva == oReserva.IdReserva)
                    .Select(drp => drp.oPaquete)
                    .FirstOrDefault();

                var detallePaqueteExistente = _context.DetalleReservaPaquetes
                   .FirstOrDefault(d => d.IdReserva == oReserva.IdReserva && d.IdPaquete == paqueteOriginal.IdPaquete);


                if (nuevoPaquete.IdPaquete != paqueteOriginal.IdPaquete)
                {

                    detallePaqueteExistente.IdPaquete = nuevoPaquete.IdPaquete;
                    detallePaqueteExistente.Costo = nuevoPaquete.Costo;

                    _context.DetalleReservaPaquetes.Update(detallePaqueteExistente);

                }

                return true;

            }

            return false;
        }

        public bool guardarServiciosSeleccionados(Reserva oReserva, string serviciosSeleccionados)
        {
            var ServiciosSeleccionadosObj = JsonConvert.DeserializeObject<List<dynamic>>(serviciosSeleccionados.ToString());

            if (ServiciosSeleccionadosObj != null && ServiciosSeleccionadosObj.Any())
            {

                var serviciosNuevos = new List<Servicio>();
                var cantidadeServiciosNuevos = new List<int>();

                for (var i = 0; i < ServiciosSeleccionadosObj.Count; i++)
                {
                    var servicio = new Servicio()
                    {
                        IdServicio = Convert.ToInt32(ServiciosSeleccionadosObj[i].id),
                        NomServicio = ServiciosSeleccionadosObj[i].nombre.ToString(),
                        Costo = Convert.ToDouble(ServiciosSeleccionadosObj[i].costo)
                    };

                    var cantidad = ServiciosSeleccionadosObj[i].cantidad;

                    serviciosNuevos.Add(servicio);

                    if (cantidad == null)
                    {
                        cantidad = 1;
                    }

                    cantidadeServiciosNuevos.Add((int)cantidad);
                }

                var serviciosOriginales = _context.DetalleReservaServicios
                    .Where(drs => drs.IdReserva == oReserva.IdReserva)
                    .Select(drs => drs.oServicio.IdServicio)
                    .ToList();

                var serviciosAEliminar = serviciosOriginales.Except(serviciosNuevos.Select(s => s.IdServicio)).ToList();
                var serviciosAAgregar = serviciosNuevos.Select(s => s.IdServicio).Except(serviciosOriginales).ToList();

                if (serviciosAEliminar.Count != 0)
                {

                    foreach (var servicioEliminar in serviciosAEliminar)
                    {

                        var detalle = _context.DetalleReservaServicios
                            .Where(drs => drs.IdReserva == oReserva.IdReserva && drs.IdServicio == servicioEliminar)
                            .FirstOrDefault();

                        _context.DetalleReservaServicios.Remove(detalle);

                    }

                }

                if (serviciosAAgregar.Count != 0)
                {

                    for (var i = 0; i < serviciosAAgregar.Count; i++)
                    {
                        var detalle = new DetalleReservaServicio()
                        {
                            IdReserva = oReserva.IdReserva,
                            IdServicio = serviciosAAgregar[i],
                            Costo = _context.Servicios
                                        .Where(s => s.IdServicio == serviciosAAgregar[i])
                                        .Select(s => s.Costo)
                                        .FirstOrDefault(),
                            Cantidad = cantidadeServiciosNuevos[i]
                        };

                        _context.DetalleReservaServicios.Add(detalle);
                    }

                }

                _context.SaveChanges();

                for (var i = 0; i < serviciosNuevos.Count; i++)
                {
                    var detalle = new DetalleReservaServicio()
                    {

                        IdReserva = oReserva.IdReserva,
                        IdServicio = serviciosNuevos[i].IdServicio,
                        Costo = serviciosNuevos[i].Costo,
                        Cantidad = cantidadeServiciosNuevos[i]

                    };

                    var detalleExistente = _context.DetalleReservaServicios
                        .FirstOrDefault(drs => drs.IdReserva == oReserva.IdReserva && drs.IdServicio == detalle.IdServicio);

                    if (detalleExistente != null)
                    {
                        detalleExistente.Cantidad = detalle.Cantidad;

                        _context.SaveChanges();
                    }
                }

                return true;

            }
            else
            {
                var serviciosAdicionales = _context.DetalleReservaServicios
                    .Where(d => d.IdReserva == oReserva.IdReserva)
                    .ToList();

                _context.DetalleReservaServicios.RemoveRange(serviciosAdicionales);

                return true;    
            }
        }

        //Exportar PDF

        public IActionResult DescargarPDF(int idReserva)
        {

            var reserva = _context.Reservas.Include(r => r.oCliente).FirstOrDefault(r => r.IdReserva == idReserva);
            var cliente = reserva.oCliente;
            var detallePaquete = _context.DetalleReservaPaquetes
                .Where(drp => drp.IdReserva == idReserva)
                .Include(drp => drp.oPaquete)
                    .ThenInclude(p => p.oHabitacion)
                .FirstOrDefault();
            var servicios = _context.DetalleReservaServicios
                .Where(drs => drs.IdReserva == idReserva)
                .Include(drs => drs.oServicio)
                    .ThenInclude(drs => drs.oTipoServicio)
                .ToList();


            var data = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);

                    //Header
                    page.Header().ShowOnce().Row(row =>
                    {
                        var rutaImagen = Path.Combine(_webHostEnvironment.WebRootPath, "Imagenes/Default/Logo.png");
                        byte[] imageData = System.IO.File.ReadAllBytes(rutaImagen);

                        row.ConstantItem(70).Image(imageData);


                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().Text("EcoGlam").Bold().FontSize(14);
                            col.Item().AlignCenter().Text("Crr 67D cll 83-23").FontSize(9);
                            col.Item().AlignCenter().Text("3113234546").FontSize(9);
                            col.Item().AlignCenter().Text("ecoglam7@gmail.com").FontSize(9);

                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Border(1).BorderColor("#257272")
                            .AlignCenter().Text("RUC 21312312312");

                            col.Item().Background("#257272").Border(1)
                            .BorderColor("#257272").AlignCenter()
                            .Text("Codigo de la reserva").FontColor("#fff");

                            col.Item().Border(1).BorderColor("#257272").
                            AlignCenter().Text(reserva.IdReserva);


                        });
                    });


                    //Contenido
                    page.Content().PaddingVertical(10).Column(col1 =>
                    {
                        //Datos Cliente
                        col1.Item().Column(col2 =>
                        {
                            col2.Item().Text("Datos del cliente").Underline().Bold();

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Nombre: ").SemiBold().FontSize(10);
                                txt.Span(cliente.Nombres + " " + cliente.Apellidos).FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Numero de Documento: ").SemiBold().FontSize(10);
                                txt.Span(cliente.NroDocumento).FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Correo: ").SemiBold().FontSize(10);
                                txt.Span(cliente.Correo).FontSize(10);
                            });
                        });

                        col1.Item().Text("");

                        //Datos Reserva
                        col1.Item().Column(col2 =>
                        {
                            col2.Item().Text("Datos de la Reserva").Underline().Bold();

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Fecha de la Reserva: ").SemiBold().FontSize(10);
                                txt.Span(reserva.FechaReserva.ToString()).FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Fecha de Incio: ").SemiBold().FontSize(10);
                                txt.Span(reserva.FechaInicio.ToString()).FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Fecha de Finalizacion: ").SemiBold().FontSize(10);
                                txt.Span(reserva.FechaFinalizacion.ToString()).FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Sub Total: ").SemiBold().FontSize(10);
                                txt.Span(reserva.SubTotal.Value.ToString("C")).FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("IVA 19%: ").SemiBold().FontSize(10);
                                txt.Span(reserva.Iva.Value.ToString("C")).FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Descuento: ").SemiBold().FontSize(10);
                                txt.Span(reserva.Descuento.ToString() + " %").FontSize(10);
                            });

                            col2.Item().Text(txt =>
                            {
                                txt.Span("Total: ").SemiBold().FontSize(10);
                                txt.Span(reserva.MontoTotal.Value.ToString("C")).FontSize(10);
                            });
                        });

                        col1.Item().Text("");

                        col1.Item().LineHorizontal(0.5f);

                        col1.Item().Text("");

                        //Paquete

                        col1.Item().Column(col2 =>
                        {
                            col2.Item().Text("Informacion del Paquete").Underline().Bold();
                        });

                        col1.Item().Text("");

                        col1.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background("#257272")
                                .Padding(2).Text("Nombre del Paquete").FontColor("#fff");

                                header.Cell().Background("#257272")
                               .Padding(2).Text("Habitacion").FontColor("#fff");

                                header.Cell().Background("#257272")
                               .Padding(2).Text("Costo").FontColor("#fff");

                            });

                            tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text(detallePaquete.oPaquete.NomPaquete).FontSize(10);

                            tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text(detallePaquete.oPaquete.oHabitacion.Nombre).FontSize(10);

                            tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text(detallePaquete.oPaquete.Costo.Value.ToString("C")).FontSize(10);

                        });

                        col1.Item().Text("");

                        col1.Item().LineHorizontal(0.5f);

                        col1.Item().Text("");

                        //Servicios

                        col1.Item().Column(col2 =>
                        {
                            col2.Item().Text("Informacion de los Servicios").Underline().Bold();
                        });

                        col1.Item().Text("");

                        col1.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background("#257272")
                                .Padding(2).Text("Nombre del Servicio").FontColor("#fff").FontSize(10);

                                header.Cell().Background("#257272")
                               .Padding(2).Text("Tipo de Servicio").FontColor("#fff").FontSize(10);

                                header.Cell().Background("#257272")
                               .Padding(2).Text("Cantidad").FontColor("#fff").FontSize(10);

                                header.Cell().Background("#257272")
                               .Padding(2).Text("Costo Indvidual").FontColor("#fff").FontSize(10);

                                header.Cell().Background("#257272")
                               .Padding(2).Text("Costo Total").FontColor("#fff").FontSize(10);

                            });

                            foreach(var servicio in servicios)
                            {
                                tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                .Padding(2).Text(servicio.oServicio.NomServicio).FontSize(10);

                                tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                    .Padding(2).Text(servicio.oServicio.oTipoServicio.NombreTipoServicio).FontSize(10);

                                tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                    .Padding(2).Text(servicio.Cantidad.ToString()).FontSize(10);

                                tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                    .Padding(2).Text(servicio.oServicio.Costo.Value.ToString("C")).FontSize(10);

                                var costoTotal = servicio.Cantidad * servicio.oServicio.Costo;

                                tabla.Cell().BorderBottom(0.5f).BorderColor("#D9D9D9")
                                    .Padding(2).Text(costoTotal.Value.ToString("C")).FontSize(10);
                            }


                        });

                    });

                    page.Footer()
                    .AlignRight()
                    .Text(txt =>
                    {
                        txt.Span("Pagina ").FontSize(10);
                        txt.CurrentPageNumber().FontSize(10);
                        txt.Span(" de ").FontSize(10);
                        txt.TotalPages().FontSize(10);
                    });
                });
            }).GeneratePdf();

            Stream stream = new MemoryStream(data);
            return File(stream, "application/pdf", "detalleReserva.pdf");

        }
    }
}
