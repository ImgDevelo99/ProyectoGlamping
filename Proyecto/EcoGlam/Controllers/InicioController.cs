using EcoGlam.DataBase;
using EcoGlam.Models;
using EcoGlam.Models.ViewModels;
using EcoGlam.Servicios;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace EcoGlam.Controllers
{
    public class InicioController : Controller
    {
        private readonly EcoGlamContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public InicioController(EcoGlamContext context, IWebHostEnvironment hostingEnviroment)
        {
            _context = context;
            _hostingEnvironment = hostingEnviroment;
        }

        public IActionResult Login()
        {
            var usuario = CargarDatosIniciales();

            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string contrasena)
        {

            if(correo != null && contrasena != null)
            {
                Usuario usuario = Validar(correo, UtilidadServicio.ConvertirSHA256(contrasena));

                if (usuario != null)
                {

                    if (!usuario.Estado.Value)
                    {
                        ViewBag.Mensaje = "Usuario Inhabilitado";
                    }
                    else
                    {
                        if (!usuario.Confirmado)
                        {
                            ViewBag.Mensaje = $"Falta confirmar su cuenta. Se le envio un correo a {correo}";
                        }
                        else if (usuario.Restablecer)
                        {
                            ViewBag.Mensaje = $"Se ha solicitado restablecer su cuenta. Por favor revise su correo";
                        }
                        else
                        {

                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, usuario.Nombres),
                                new Claim("Correo", usuario.Correo),
                            };

                            var permisosAsociados = _context.PermisosRoles
                                .Where(pr => pr.IdRol == usuario.IdRol)
                                .Select(pr => pr.oPermiso.NomPermiso)
                                .ToList();

                            foreach (var permiso in permisosAsociados)
                            {
                                claims.Add(new Claim("Permiso", permiso));
                            }

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                            return RedirectToAction("Index", "Dashboard");
                        }
                    }

                }
                else
                {
                    ViewBag.Mensaje = "No se encontraron coincidencias";
                }
            }
            else
            {
                ViewBag.Mensaje = "Rellene los campos";
            }

            

            return View(CargarDatosIniciales());
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(Usuario oUsuario) 
        {

            if(oUsuario.Contrasena != oUsuario.ConfirmarContrasena)
            {
                TempData["NroDocumento"] = oUsuario.Nombres;
                TempData["TipoDocumento"] = oUsuario.IdTipoDocumento;
                TempData["Nombre"] = oUsuario.Nombres;
                TempData["Apellido"] = oUsuario.Apellidos;
                TempData["Celular"] = oUsuario.Celular;
                TempData["Correo"] = oUsuario.Correo;
                TempData["RegistroActivo"] = true;
                TempData["Mensaje"] = "Las contraseñas no coinciden";

                return RedirectToAction("Login","Inicio");
            }

            if(ObtenerDocumento(oUsuario.NroDocumento) == null && ObtenerCorreo(oUsuario.Correo) == null)
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
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Headers["host"], "/Inicio/Confirmar?token=" + oUsuario.Token);

                    string htmlBody = string.Format(content, oUsuario.Nombres, url);

                    Correo oCorreo = new Correo()
                    {
                        Para = oUsuario.Correo,
                        Asunto = "Correo confirmacion",
                        Contenido = htmlBody
                    };

                    _ = Task.Run(() => CorreoServicio.Enviar(oCorreo));
                    TempData["Creado"] = true;
                    TempData["Mensaje"] = $"Su cuenta ha sido creada. Hemos enviado un mensaje al correo {oUsuario.Correo} para confirmar su cuenta";
                }
                else
                {
                    TempData["Mensaje"] = $"No se pudo crear su cuenta";
                }

            }
            else
            {
                if(ObtenerDocumento(oUsuario.NroDocumento) != null)
                {
                    TempData["Mensaje"] = "Este numero de documento ya esta regisrado";
                }else if (ObtenerCorreo(oUsuario.Correo) != null)
                {
                    TempData["Mensaje"] = "Este correo ya esta regisrado";
                }
            }

            TempData["RegistroActivo"] = true;
            return RedirectToAction("Login");

        }

        public IActionResult Confirmar(string token)
        {
            ViewBag.Respuesta = ConfirmarDB(token);
            return View();
        }

        public IActionResult Restablecer()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Restablecer(string correo)
        {
            Usuario oUsuario = ObtenerCorreo(correo);
            ViewBag.Correo = correo;

            if (oUsuario != null)
            {
                bool respuesta = RestablecerActualizar(true, oUsuario.Contrasena, oUsuario.Token);

                if (respuesta)
                {
                    string rootPath = _hostingEnvironment.ContentRootPath;

                    string path = Path.Combine(rootPath, "Plantilla", "Restablecer.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Headers["host"], "/Inicio/Actualizar?token=" + oUsuario.Token);

                    string htmlBody = string.Format(content, oUsuario.Nombres, url);

                    Correo oCorreo = new Correo()
                    {
                        Para = correo,
                        Asunto = "Restablecer Cuenta",
                        Contenido = htmlBody
                    };

                    bool enviado = CorreoServicio.Enviar(oCorreo);
                    ViewBag.Restablecido = true;
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo restablecer la cuenta";
                }
            }
            else
            {
                ViewBag.Mensaje = "No se encontraron coincidencias con el correo";
            }

            return View();
        }

        public IActionResult Actualizar(string token)
        {
            ViewBag.Token = token;  
            return View();
        }

        [HttpPost]
        public IActionResult Actualizar(string token, string contrasena, string confirmarContrasena)
        {
            ViewBag.Token = token;

            if(contrasena != confirmarContrasena)
            {
                ViewBag.Mensaje = "Las contrasenas no coinciden";
                return View();
            }

            bool respuesta = RestablecerActualizar(false, UtilidadServicio.ConvertirSHA256(contrasena), token);

            if (respuesta)
            {
                ViewBag.Restablecido = true;
            }
            else
            {
                ViewBag.Mensaje = "No se pudo actualizar";
            }

            return View();
        }

        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        public IActionResult Error()
        {
            return View();
        }

        //Metodos

        public bool RegistrarDB(Usuario oUsuario)
        {
            bool respuesta = false;

            try
            {
                _context.Usuarios.Add(oUsuario);
                _context.SaveChanges();
                respuesta = true;

            }catch (Exception ex)
            {
                throw ex;
            }
            

            return respuesta;

        }

        public Usuario Validar(string correo, string contrasena)
        {
            var usuario = _context.Usuarios
                .Where(u => u.Correo == correo && u.Contrasena == contrasena)
                .Include(u => u.oRol)
                .FirstOrDefault();

            if(usuario != null)
            {
                Usuario oUsuario = new Usuario()
                {
                    Nombres = usuario.Nombres.ToString(),
                    Apellidos = usuario.Apellidos.ToString(),
                    Correo = usuario.Correo,
                    Restablecer = usuario.Restablecer,
                    Confirmado = usuario.Confirmado,
                    Estado = usuario.Estado,
                    IdRol = usuario.IdRol
                };

                return oUsuario;
            }

            return null;
            
        }

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

            if(usuario != null)
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
            
            return null ;
        }

        public bool RestablecerActualizar(bool restablecer, string contrasena, string token)
        {
            bool respuesta = false;

            try
            {
                var usuario = _context.Usuarios
                .Where(u => u.Token == token)
                .FirstOrDefault();

                usuario.Restablecer = restablecer;
                usuario.Contrasena = contrasena;

                _context.Usuarios.Update(usuario);
                _context.SaveChanges();
                respuesta = true;

                return respuesta;
            }
            catch (Exception ex)
            {
                throw ex;
            }

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
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public UsuarioVM CargarDatosIniciales()
        {
            var usuario = new UsuarioVM()
            {
                oUsuario = new Usuario(),
                oListaTipoDocumento = _context.TipoDocumentos.Select(tDocumento => new SelectListItem()
                {
                    Text = tDocumento.NomTipoDcumento,
                    Value = tDocumento.IdTipoDocumento.ToString()
                }).ToList()
            };

            return usuario;
        }
    }
}
