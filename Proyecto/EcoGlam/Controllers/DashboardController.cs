using EcoGlam.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using EcoGlam.DataBase;
using EcoGlam.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Data;

namespace EcoGlam.Controllers
{
    [Authorize(Policy = "DashboardPermiso")]
    public class DashboardController : Controller
    {
        private readonly EcoGlamContext _context;

        public DashboardController(EcoGlamContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //Metodos

        public IActionResult resumenReservas(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            List<DashReservaVM> Lista = new List<DashReservaVM>();

            if (formatoFecha == 3 || formatoFecha == 4)
            {
                CultureInfo culture = new CultureInfo("es-ES");

                for (DateOnly fecha = new DateOnly(fechaInicio.Year, fechaInicio.Month, 1); fecha <= FechaFin; fecha = fecha.AddMonths(1))
                {
                    int cantidadReservas = _context.Reservas
                        .Count(r => r.FechaReserva.Value.Year == fecha.Year && r.FechaReserva.Value.Month == fecha.Month);

                    string nombreMes = culture.DateTimeFormat.GetMonthName(fecha.Month);
                    Lista.Add(new DashReservaVM
                    {
                        Fecha = $"{nombreMes} {fecha.Year}",
                        Cantidad = cantidadReservas.ToString()
                    });
                }
            }
            else
            {
                for (DateOnly fecha = fechaInicio; fecha <= FechaFin; fecha = fecha.AddDays(1))
                {
                    int cantidadReservas = _context.Reservas
                        .Count(r => r.FechaReserva == fecha);

                    Lista.Add(new DashReservaVM
                    {
                        Fecha = fecha.ToString("dd-MM-yyyy"),
                        Cantidad = cantidadReservas.ToString()
                    });
                }
            }

            return Json(Lista);
        }

        public IActionResult resumenPaquetes(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            var resultados = _context.DetalleReservaPaquetes
                .Where(d => d.oReserva.FechaReserva >= fechaInicio && d.oReserva.FechaReserva <= FechaFin)
                .GroupBy(d => d.IdPaquete)
                .Select(g => new
                {
                    NombrePaquete = g.FirstOrDefault().oPaquete.NomPaquete,
                    CantidadReservas = g.Count()
                })
                .ToList();

            List<DashPaqueteVM> lista = resultados.Select(r => new DashPaqueteVM
            {
                NombrePaquete = r.NombrePaquete,
                Cantidad = r.CantidadReservas.ToString()
            }).ToList();

            return Json(lista);
        }

        public IActionResult resumenServicios(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            var resultados = _context.DetalleReservaServicios
                .Where(d => d.oReserva.FechaReserva >= fechaInicio && d.oReserva.FechaReserva <= FechaFin)
                .GroupBy(d => d.IdServicio)
                .Select(g => new
                {
                    NombreServicio = g.FirstOrDefault().oServicio.NomServicio,
                    CantidadReservas = g.Sum(d => d.Cantidad)
                })
                .ToList();

            List<DashServiciosVM> lista = resultados.Select(r => new DashServiciosVM
            {
                NombreServicio = r.NombreServicio,
                Cantidad = r.CantidadReservas.ToString()
            }).ToList();

            return Json(lista);
        }

        public IActionResult resumenTipoHabi(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            var resultados = _context.DetalleReservaPaquetes
                .Include(d => d.oPaquete)
                    .ThenInclude(p => p.oHabitacion)
                        .ThenInclude(h => h.oTipoHabitacion)
                .Where(d => d.oReserva.FechaReserva >= fechaInicio && d.oReserva.FechaReserva <= FechaFin)
                .GroupBy(d => d.oPaquete.oHabitacion.oTipoHabitacion.IdTipoHabitacion)
                .Select(g => new
                {
                    NombreTipoHabitacion = g.FirstOrDefault().oPaquete.oHabitacion.oTipoHabitacion.NomTipoHabitacion,
                    CantidadReservas = g.Count()
                })
                .ToList();

            List<DashTipoHabitacionVM> lista = resultados.Select(r => new DashTipoHabitacionVM
            {
                NombreTipoHabitacion = r.NombreTipoHabitacion,
                Cantidad = r.CantidadReservas.ToString()
            }).ToList();

            return Json(lista);
        }

        public IActionResult resumenEstadosReserva(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            var resultados = _context.Reservas
                .Include(r => r.oEstadoReserva)
                .Where(r => r.FechaReserva >= fechaInicio && r.FechaReserva <= FechaFin)
                .GroupBy(r => r.IdEstadoReserva)
                .Select(g => new
                {
                    NombreEstado = g.FirstOrDefault().oEstadoReserva.NombreEstadoReserva,
                    CantidadReservas = g.Count()
                });

            List<DashEstadoReservaVM> lista = resultados.Select(r => new DashEstadoReservaVM
            {
                NombreEstado = r.NombreEstado,
                Cantidad = r.CantidadReservas.ToString()
            }).ToList();

            return Json(lista);
        }

        public IActionResult infoBasicaDash(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            var reservasActivas = _context.Reservas
                .Count(r => r.FechaReserva >= fechaInicio && r.FechaReserva <= FechaFin &&
                    r.IdEstadoReserva != 5 && r.IdEstadoReserva != 6);
            var reservasRealizadas = _context.Reservas
                .Count(r => r.FechaReserva >= fechaInicio && r.FechaReserva <= FechaFin);
            var reservasFinalizadas = _context.Reservas
                .Count(r => r.FechaReserva >= fechaInicio && r.FechaReserva <= FechaFin &&
                    r.IdEstadoReserva == 6);
            var Ingresos = _context.Abonos
                .Where(a => a.IdAbono != 0 && a.oReserva.FechaReserva >= fechaInicio && a.oReserva.FechaReserva <= FechaFin &&
                    (a.oReserva.IdEstadoReserva == 5 || a.oReserva.IdEstadoReserva == 6))
                .Sum(a => a.CantAbono);

            var info = new
            {
                ReservasActivas = reservasActivas,
                ReservasRealizadas = reservasRealizadas,
                ReservasFinalizadas = reservasFinalizadas,
                Ingresos = Ingresos
            };

            return Json(info);
        }

        //Obtencion de Fechas

        private DateOnly ObtenerFechaInicioUltimosDias(DateTime fechaDateTime, int cantidadDias)
        {
            return DateOnly.FromDateTime(fechaDateTime).AddDays(-cantidadDias);
        }

        private DateOnly ObtenerFechaInicioUltimoMes(DateTime fechaDateTime)
        {
            return DateOnly.FromDateTime(fechaDateTime.AddMonths(-1));
        }

        private DateOnly ObtenerFechaInicioUltimosMeses(DateTime fechaDateTime, int cantidadMeses)
        {
            return DateOnly.FromDateTime(fechaDateTime.AddMonths(-cantidadMeses + 1));
        }

        private DateOnly ObtenerFechaInicioUltimoAnio(DateTime fechaDateTime)
        {
            return DateOnly.FromDateTime(fechaDateTime.AddYears(-1));
        }

        private DateOnly ObtenerPrimerDiaMesActual(DateTime fechaDateTime)
        {
            return new DateOnly(fechaDateTime.Year, fechaDateTime.Month, 1);
        }

        private (DateOnly, DateOnly) ObtenerRangoSemanaActual(DateTime fechaDateTime)
        {
            DayOfWeek primerDiaSemana = DayOfWeek.Monday;
            int diferenciaDias = (fechaDateTime.DayOfWeek - primerDiaSemana + 7) % 7;
            DateTime primerDiaSemanaActual = fechaDateTime.AddDays(-diferenciaDias);
            DateTime ultimoDiaSemanaActual = primerDiaSemanaActual.AddDays(6);
            return (DateOnly.FromDateTime(primerDiaSemanaActual), DateOnly.FromDateTime(ultimoDiaSemanaActual));
        }

    }
}
