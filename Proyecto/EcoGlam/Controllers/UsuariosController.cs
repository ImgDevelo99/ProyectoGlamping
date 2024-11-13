using EcoGlam.DataBase;
using EcoGlam.Models;
using EcoGlam.Models.ViewModels;
using EcoGlam.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace EcoGlam.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly EcoGlamContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public UsuariosController(EcoGlamContext context, IWebHostEnvironment hostingEnviroment)
        {
            _context = context;
            _hostingEnvironment = hostingEnviroment;
        }

        [Authorize(Policy = "ListarUsuariosPermiso")]
        public IActionResult Index()
        {
            return View(ObtenerTodosLosUsuarios());
        }

        [Authorize(Policy = "CrearUsuariosPermiso")]
        [HttpGet]
        public IActionResult Registrar()
        {
            return View(CargarDatosIniciales());
        }

        [Authorize(Policy = "CrearUsuariosPermiso")]
        [HttpPost]
        public async Task<IActionResult> Registrar(Usuario oUsuario)
        {
            ViewBag.NumeroDocumento = oUsuario.NroDocumento;
            ViewBag.TipoDocumento = oUsuario.IdTipoDocumento.ToString();
            ViewBag.Nombres = oUsuario.Nombres;
            ViewBag.Apellidos = oUsuario.Apellidos;
            ViewBag.Celular = oUsuario.Celular;
            ViewBag.Correo = oUsuario.Correo;

            if (oUsuario.Contrasena == null)
            {
                ViewBag.Mensaje = "Complete todos los campos";

                return View(CargarDatosIniciales());
            }

            if (oUsuario.Contrasena != oUsuario.ConfirmarContrasena)
            {

                ViewBag.Mensaje = "Las contraseñas no coinciden";

                return View(CargarDatosIniciales());
            }

            if (ObtenerDocumento(oUsuario.NroDocumento) == null && ObtenerCorreo(oUsuario.Correo) == null)
            {

                oUsuario.Contrasena = UtilidadServicio.ConvertirSHA256(oUsuario.Contrasena);
                oUsuario.Token = UtilidadServicio.GenerarToken();
                oUsuario.Restablecer = false;
                oUsuario.Confirmado = false;
                oUsuario.Estado = true;
                oUsuario.IdRol = 1;

                bool respuesta = RegistrarDB(oUsuario);

                if (respuesta)
                {

                    string rootPath = _hostingEnvironment.ContentRootPath;

                    string path = Path.Combine(rootPath, "Plantilla", "Confirmar.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Headers["host"], "/Usuarios/Confirmar?token=" + oUsuario.Token);

                    string htmlBody = string.Format(content, oUsuario.Nombres, url);

                    Correo oCorreo = new Correo()
                    {
                        Para = oUsuario.Correo,
                        Asunto = "Correo confirmacion",
                        Contenido = htmlBody
                    };

                    _ = Task.Run(() => CorreoServicio.Enviar(oCorreo));

                    TempData["Mensaje"] = $"Se creo la cuenta exitosamente. Se envio un correo de confirmacion al correo {oUsuario.Correo}";
                }
                else
                {
                    ViewBag.Mensaje = $"No se pudo crear la cuenta. Revisa los campos e intentalo de nuevo";
                    return View(CargarDatosIniciales());
                }

            }
            else
            {
                if (ObtenerDocumento(oUsuario.NroDocumento) != null)
                {
                    ViewBag.Mensaje = "Este numero de documento ya esta registrado";
                    return View(CargarDatosIniciales());
                }
                else if (ObtenerCorreo(oUsuario.Correo) != null)
                {
                    ViewBag.Mensaje = "Este correo ya esta registrado";
                    return View(CargarDatosIniciales());
                }
            }

            return RedirectToAction("Index");

        }

        [Authorize(Policy = "EditarUsuariosPermiso")]
        [HttpGet]
        public IActionResult Editar(int NroDocumento)
        {
            return View(CargarDatosEditar(NroDocumento));
        }

        [Authorize(Policy = "EditarUsuariosPermiso")]
        [HttpPost]
        public IActionResult Editar(Usuario oUsuario)
        {
            _context.Usuarios.Update(oUsuario);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "VerDetallesUsuariosPermiso")]
        public IActionResult Detalles(int NroDocumento)
        {
            return View(CargarDatosEditar(NroDocumento));
        }

        [Authorize(Policy = "CambiarEstadoUsuariosPermiso")]
        [HttpPost]
        public IActionResult cambiarEstado(int? nroDocumento, bool estado)
        {

            if (nroDocumento.HasValue)
            {

                var usuarios = _context.Usuarios.Find(nroDocumento.Value);

                if (usuarios != null)
                {
                    usuarios.Estado = estado;

                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }

            }

            return NotFound();

        }

        public IActionResult Confirmar(string token)
        {
            ViewBag.Respuesta = ConfirmarDB(token);
            return View();
        }

        //Metodos

        public Usuario ObtenerDocumento(int documento)
        {
            var usuario = _context.Usuarios
                .Where(u => u.NroDocumento == documento)
                .FirstOrDefault();

            if (usuario != null)
            {
                Usuario oUsuario = new Usuario()
                {
                    Nombres = usuario.Nombres.ToString(),
                    Apellidos = usuario.Apellidos.ToString(),
                    Contrasena = usuario.Contrasena.ToString(),
                    Restablecer = usuario.Restablecer,
                    Confirmado = usuario.Confirmado,
                    Token = usuario.Token,
                };

                return oUsuario;
            }

            return null;

        }

        public Usuario ObtenerCorreo(string correo)
        {
            var usuario = _context.Usuarios
                .Where(u => u.Correo == correo)
                .FirstOrDefault();

            if (usuario != null)
            {
                Usuario oUsuario = new Usuario()
                {
                    Nombres = usuario.Nombres.ToString(),
                    Apellidos = usuario.Apellidos.ToString(),
                    Contrasena = usuario.Contrasena.ToString(),
                    Restablecer = usuario.Restablecer,
                    Confirmado = usuario.Confirmado,
                    Token = usuario.Token,
                };

                return oUsuario;
            }

            return null;
        }

        public bool RegistrarDB(Usuario oUsuario)
        {
            bool respuesta = false;

            try
            {
                _context.Usuarios.Add(oUsuario);
                _context.SaveChanges();
                respuesta = true;

            }
            catch (Exception ex)
            {
                throw ex;
            }


            return respuesta;

        }

        public List<Usuario> ObtenerTodosLosUsuarios()
        {
            var usuarios = _context.Usuarios
                .Include(u => u.oRol)
                .ToList();

            return usuarios;
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<Usuario> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = ObtenerTodosLosUsuarios();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        private List<Usuario> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            var resultados = _context.Usuarios
                                  .Include(u => u.oRol)
                                  .Where(u => u.Nombres.Contains(searchTerm) || u.Apellidos.Contains(searchTerm) || u.NroDocumento.ToString().Contains(searchTerm) || u.Correo.Contains(searchTerm))
                                  .ToList();

            return resultados;
        }

        public UsuarioVM CargarDatosIniciales()
        {
            var usuarioVM = new UsuarioVM()
            {
                oUsuario = new Usuario(),
                oListaTipoDocumento = _context.TipoDocumentos.Select(tDocumento => new SelectListItem()
                {
                    Text = tDocumento.NomTipoDcumento,
                    Value = tDocumento.IdTipoDocumento.ToString()
                }).ToList()
            };

            return usuarioVM;
        }

        public UsuarioVM CargarDatosEditar(int NroDocumento)
        {

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.NroDocumento == NroDocumento);

            var usuarioVM = new UsuarioVM()
            {
                oUsuario = usuario,
                oListaTipoDocumento = _context.TipoDocumentos.Select(tDocumento => new SelectListItem()
                {
                    Text = tDocumento.NomTipoDcumento,
                    Value = tDocumento.IdTipoDocumento.ToString()
                }).ToList()
            };

            return usuarioVM;
        }

        public bool ConfirmarDB(string token)
        {
            bool respuesta = false;

            try
            {
                var usuario = _context.Usuarios
                .Where(u => u.Token == token)
                .FirstOrDefault();

                usuario.Confirmado = true;

                _context.Usuarios.Update(usuario);
                _context.SaveChanges();
                respuesta = true;
                return respuesta;
            }
            catch (Exception ex)
            {
                RedirectToAction("Error", "Inicio");
                return false;
            }
        }

        public IActionResult validarCambioEstado(int nroDocumento)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.NroDocumento == nroDocumento);
            var oRol = _context.Roles.Where(r => r.IdRol == usuario.IdRol).FirstOrDefault();

            if (oRol.Estado.Value)
            {
                return Json(new { respuesta = true });
            }
            else
            {
                return Json(new { respuesta = false });
            }
        }

    }


}
