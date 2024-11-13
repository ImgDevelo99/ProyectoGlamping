using EcoGlam.DataBase;
using EcoGlam.Models;
using EcoGlam.Models.ViewModels;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace EcoGlam.Controllers
{
    public class RolesController : Controller
    {
        private readonly EcoGlamContext _context;

        public RolesController(EcoGlamContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "ListarRolesPermiso")]
        public IActionResult Index()
        {
            return View(ObtenerRoles());
        }

        [Authorize(Policy = "CrearRolesPermiso")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View(CargarDatosIniciales());
        }

        [Authorize(Policy = "CrearRolesPermiso")]
        [HttpPost]
        public IActionResult Crear(Role oRol, string permisosSeleccionados)
        {
            if (!ModelState.IsValid)
            {
                return View(CargarDatosIniciales());
            }

            if (Existe(oRol.NomRol))
            {
                ModelState.AddModelError("oRol.NomRol", "Este rol ya existe");
                return View(CargarDatosIniciales());
            }

            if (permisosSeleccionados.IsNullOrEmpty() || permisosSeleccionados == "[]")
            {
                ViewBag.Mensaje = "Selecciona al menos un permiso";
                return View(CargarDatosIniciales());
            }

            _context.Roles.Add(oRol);
            _context.SaveChanges();

            List<int>? Idspermisos = JsonConvert.DeserializeObject<List<int>>(permisosSeleccionados);

            if(Idspermisos != null || Idspermisos.Count != 0)
            {
                foreach(var idPermiso in Idspermisos)
                {
                    var oRolPermiso = new PermisosRole()
                    {
                        IdRol = oRol.IdRol,
                        IdPermiso = idPermiso
                    };

                    _context.PermisosRoles.Add(oRolPermiso);
                }
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "EditarRolesPermiso")]
        [HttpGet]
        public IActionResult Editar(int idRol)
        {
            ViewBag.ObjPermisosAsociados = ObtenerDiccionarioPermisos(idRol);
            return View(CargarDatosEditar(idRol));
        }

        [Authorize(Policy = "EditarRolesPermiso")]
        [HttpPost]
        public IActionResult Editar(Role oRol, string permisosSeleccionados)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ObjPermisosAsociados = ObtenerDiccionarioPermisos(oRol.IdRol.Value);
                return View(CargarDatosIniciales());
            }

            if (Existe(oRol))
            {
                ViewBag.ObjPermisosAsociados = ObtenerDiccionarioPermisos(oRol.IdRol.Value);
                ModelState.AddModelError("oRol.NomRol", "Este rol ya existe");
                return View(CargarDatosIniciales());
            }

            if (permisosSeleccionados.IsNullOrEmpty() || permisosSeleccionados == "[]")
            {
                ViewBag.Mensaje = "Selecciona al menos un permiso";
                return View(CargarDatosIniciales());
            }

            var idsPermisosOriginales = _context.PermisosRoles
                .Where(pr => pr.IdRol == oRol.IdRol)
                .Select(pr => pr.IdPermiso.Value).ToList();

            List<int>? idspermisosNuevos = JsonConvert.DeserializeObject<List<int>>(permisosSeleccionados);

            if((idsPermisosOriginales != null && idsPermisosOriginales.Count != 0) && (idspermisosNuevos != null && idspermisosNuevos.Count != 0))
            {
                if(!idsPermisosOriginales.OrderBy(x => x).SequenceEqual(idspermisosNuevos.OrderBy(x => x)))
                {
                    var idsPermisosAAgregar = idspermisosNuevos.Except(idsPermisosOriginales).ToList();
                    var idsPermisosAEliminar = idsPermisosOriginales.Except(idspermisosNuevos).ToList();

                    if(idsPermisosAEliminar.Count != 0)
                    {
                       foreach (var permisoEliminar in idsPermisosAEliminar)
                        {
                            var oPermisoRol = _context.PermisosRoles.FirstOrDefault(pr => pr.IdPermiso == permisoEliminar && pr.IdRol == oRol.IdRol);

                            if(oPermisoRol != null)
                            {
                                _context.PermisosRoles.Remove(oPermisoRol);
                            }
                        }
                    }

                    if(idsPermisosAAgregar.Count != 0)
                    {
                        foreach(var permiso in idsPermisosAAgregar)
                        {
                            var oPermisoRol = new PermisosRole()
                            {
                                IdPermiso = permiso,
                                IdRol = oRol.IdRol,
                            };

                            _context.PermisosRoles.Add(oPermisoRol);
                        }
                    }

                    _context.SaveChanges();
                }
            }

            _context.Roles.Update(oRol);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "VerDetallesRolesPermiso")]
        public IActionResult Detalles(int idRol)
        {
            return View(CargarDatosDetalles(idRol));
        }

        [Authorize(Policy = "CambiarEstadoRolesPermiso")]
        public IActionResult cambiarEstado(int? id, bool estado)
        {

            if (id.HasValue)
            {

                var rol = _context.Roles.Find(id.Value);
                var usuariosAsociadosRol = ObtenerUsuariosRol(id.Value);
                var clientesAsociadosRol = ObtenerClientesRol(id.Value);

                if (rol != null)
                {
                    if (!estado)
                    {
                        if (usuariosAsociadosRol.Count != 0 && usuariosAsociadosRol != null)
                        {
                            foreach (var usuario in usuariosAsociadosRol)
                            {
                                usuario.Estado = estado;
                            }
                        }

                        if (clientesAsociadosRol.Count != 0 && clientesAsociadosRol != null)
                        {
                            foreach (var cliente in clientesAsociadosRol)
                            {
                                cliente.Estado = estado;
                            }
                        }
                    }
                    
                    rol.Estado = estado;
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }

            }

            return NotFound();

        }

        //Metodos 

        public RolVM CargarDatosIniciales()
        {
            var oRolVm = new RolVM()
            {
                oRol = new Role(),
                oListaPermisos = _context.Permisos.ToList()
            };

            return oRolVm;
        }

        public RolVM CargarDatosEditar(int idRol)
        {
            var oRolVm = new RolVM()
            {
                oRol = _context.Roles.FirstOrDefault(r => r.IdRol == idRol)  ?? new Role(),
                oListaPermisos = _context.Permisos.ToList()
            };

            return oRolVm;
        }

        public RolVM CargarDatosDetalles(int idRol)
        {
            var oRolVm = new RolVM()
            {
                oRol = _context.Roles.FirstOrDefault(r => r.IdRol == idRol) ?? new Role(),
                oListaPermisos = _context.PermisosRoles.Where(pr => pr.IdRol == idRol).Select(pr => pr.oPermiso).ToList()
            };

            return oRolVm;
        }

        public List<Role> ObtenerRoles()
        {
            return _context.Roles.ToList();
        }

        private List<Role> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            var resultados = _context.Roles
                                  .Where(e => e.NomRol.Contains(searchTerm))
                                  .ToList();

            return resultados;
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<Role> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = ObtenerRoles();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        public IActionResult BuscarPermiso(string searchTerm)
        {
            List<Permiso> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = _context.Permisos.ToList();
            }
            else
            {
                resultados = _context.Permisos.Where(p => p.NomPermiso.Contains(searchTerm)).ToList();
            }

            return PartialView("_ResultadoBusquedaParcialPermisos", resultados);
        }

        public bool Existe(string nombre)
        {
            return _context.Roles.Any(r => r.NomRol == nombre);
        }

        public bool Existe(Role oRol)
        {
            return _context.Roles.Any(r => r.NomRol == oRol.NomRol && r.IdRol != oRol.IdRol);
        }

        public Dictionary<int, bool> ObtenerDiccionarioPermisos(int idRol)
        {
            var permisosAsociados = _context.PermisosRoles.Where(pr => pr.IdRol == idRol).Select(pr => pr.oPermiso).ToList();
            var ObjPermisosAsociados = new Dictionary<int, bool>();

            foreach (var permiso in permisosAsociados)
            {
                ObjPermisosAsociados[permiso.IdPermiso.Value] = true;
            }
            return ObjPermisosAsociados;
        }

        public List<Usuario> ObtenerUsuariosRol(int idRol)
        {
            return _context.Usuarios.Where(u => u.IdRol == idRol).ToList();
        }

        public List<Cliente> ObtenerClientesRol(int idRol)
        {
            return _context.Clientes.Where(c => c.IdRol == idRol).ToList();
        }

        public IActionResult validarCambioEstado(int idRol)
        {
            var clientes = _context.Clientes.Where(c => c.IdRol == idRol && c.Estado == true).ToList();
            var usuarios = _context.Usuarios.Where(u => u.IdRol == idRol && u.Estado == true).ToList();

            if (clientes.Count != 0 && usuarios.Count != 0)
            {
                return Json(new { resultado = 1, mensaje = "Hay clientes y usuarios activos con este rol, si lo inhabilitas ellos tambien seran inhabilitados" });
            }
            else if (clientes.Count != 0)
            {
                return Json(new { resultado = 2, mensaje = "Hay clientes activos con este rol, si lo inhabilitas ellos tambien seran inhabilitados" });
            }
            else if (usuarios.Count != 0)
            {
                return Json(new { resultado = 3, mensaje = "Hay usuarios activos con este rol, si lo inhabilitas ellos tambien seran inhabilitados" });
            }
            else
            {
                return Json(new { resultado = 0, mensaje = "" });
            }
        }
    }
}
