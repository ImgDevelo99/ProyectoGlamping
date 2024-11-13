using EcoGlam.DataBase;
using EcoGlam.Models.ViewModels;
using EcoGlam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EcoGlam.Controllers
{
    public class TipoServiciosController : Controller
    {
        private readonly EcoGlamContext _context;

        public TipoServiciosController(EcoGlamContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "ListarTipoServicioPermiso")]
        public IActionResult Index()
        {
            var tipoServicios = ObtenerTodosLosTiposServicio();

            return View(tipoServicios);
        }

        [Authorize(Policy = "CrearTipoServiciosPermiso")]
        [HttpGet]
        public IActionResult Crear()
        {
            var oTipoServicio = new TipoServicio();
            return View(oTipoServicio);
        }

        [Authorize(Policy = "CrearTipoServiciosPermiso")]
        [HttpPost]
        public async Task<IActionResult> Crear(TipoServicio oTipoServicio)
        {
            if (Existe(oTipoServicio.NombreTipoServicio))
            {
                ModelState.AddModelError("NombreTipoServicio", "El tipo de servicio ya existe");
                return View(oTipoServicio);
            }

            _context.TipoServicios.Add(oTipoServicio);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "TipoServicios");
        }

        [Authorize(Policy = "EditarTipoServiciosPermiso")]
        [HttpGet]
        public IActionResult Editar(int idTipoServicio)
        {

            var oTipoServicio = _context.TipoServicios
                                        .Where(s => s.IdTipoServicio == idTipoServicio)
                                        .FirstOrDefault();

            return View(oTipoServicio);

        }

        [Authorize(Policy = "EditarTipoServiciosPermiso")]
        [HttpPost]
        public IActionResult Editar (TipoServicio oTipoServicio)
        {
            if (Existe(oTipoServicio))
            {
                ModelState.AddModelError("NombreTipoServicio", "El servicio ya existe");
                return View(CargarDatosEditar(oTipoServicio.IdTipoServicio));
            }

            _context.TipoServicios.Update(oTipoServicio);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Metodos

        public TipoServicio CargarDatosEditar(int? idServicio)
        {
            TipoServicio oTipoServicio = _context.TipoServicios
                                            .Where(tp => tp.IdTipoServicio == idServicio)
                                            .FirstOrDefault();

            return oTipoServicio;
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<TipoServicio> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = ObtenerTodosLosTiposServicio();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        private List<TipoServicio> ObtenerTodosLosTiposServicio()
        {
            var todosLosTipoServicios = _context.TipoServicios
                .ToList();

            return todosLosTipoServicios;
        }

        private List<TipoServicio> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            var resultados = _context.TipoServicios
                                  .Where(e => e.NombreTipoServicio.Contains(searchTerm))
                                  .ToList();

            return resultados;
        }

        public bool Existe(string nombre)
        {
            return _context.TipoServicios.Any(ts => ts.NombreTipoServicio == nombre);
        }

        public bool Existe(TipoServicio oTipoServicio)
        {
            return _context.TipoServicios.Any(ts => ts.NombreTipoServicio == oTipoServicio.NombreTipoServicio && ts.IdTipoServicio != oTipoServicio.IdTipoServicio);
        }
    }
}