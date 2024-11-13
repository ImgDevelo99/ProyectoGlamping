using EcoGlam.DataBase;
using EcoGlam.Models.ViewModels;
using EcoGlam.Models;
using EcoGlam.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace EcoGlam.Controllers
{
    public class ClientesController : Controller
    {
        private readonly EcoGlamContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ClientesController(EcoGlamContext context, IWebHostEnvironment hostingEnviroment)
        {
            _context = context;
            _hostingEnvironment = hostingEnviroment;
        }

        [Authorize(Policy = "ListarClientesPermiso")]
        public IActionResult Index()
        {
            return View(ObtenerTodosLosClientes());
        }

        [Authorize(Policy = "CrearClientesPermiso")]
        [HttpGet]
        public IActionResult Registrar()
        {
            return View(CargarDatosIniciales());
        }

        [Authorize(Policy = "CrearClientesPermiso")]
        [HttpPost]
        public async Task<IActionResult> Registrar(Cliente oCliente)
        {
            ViewBag.NumeroDocumento = oCliente.NroDocumento;
            ViewBag.TipoDocumento = oCliente.IdTipoDocumento.ToString();
            ViewBag.Nombres = oCliente.Nombres;
            ViewBag.Apellidos = oCliente.Apellidos;
            ViewBag.Celular = oCliente.Celular;
            ViewBag.Correo = oCliente.Correo;

            if (oCliente.Contrasena == null)
            {
                ViewBag.Mensaje = "Complete todos los campos";

                return View(CargarDatosIniciales());
            }

            if (oCliente.Contrasena != oCliente.ConfirmarContrasena)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden";

                return View(CargarDatosIniciales());
            }

            if (ObtenerDocumento(oCliente.NroDocumento) == null && ObtenerCorreo(oCliente.Correo) == null)
            {

                oCliente.Contrasena = UtilidadServicio.ConvertirSHA256(oCliente.Contrasena);
                oCliente.Token = UtilidadServicio.GenerarToken();
                oCliente.Restablecer = false;
                oCliente.Confirmado = false;
                oCliente.Estado = true;
                oCliente.IdRol = 2;

                bool respuesta = RegistrarDB(oCliente);

                if (respuesta)
                {

                    string rootPath = _hostingEnvironment.ContentRootPath;

                    string path = Path.Combine(rootPath, "Plantilla", "Confirmar.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Scheme, Request.Headers["host"], "/Clientes/Confirmar?token=" + oCliente.Token);

                    string htmlBody = string.Format(content, oCliente.Nombres, url);

                    Correo oCorreo = new Correo()
                    {
                        Para = oCliente.Correo,
                        Asunto = "Correo confirmacion",
                        Contenido = htmlBody
                    };

                    _ = Task.Run(() => CorreoServicio.Enviar(oCorreo));

                    TempData["Mensaje"] = $"Se creo la cuenta exitosamente. Se envio un correo de confirmacion al correo {oCliente.Correo}";
                }
                else
                {
                    ViewBag.Mensaje = $"No se pudo crear la cuenta. Revisa los campos e intentalo de nuevo";
                    return View(CargarDatosIniciales());
                }

            }
            else
            {
                if (ObtenerDocumento(oCliente.NroDocumento) != null)
                {
                    ViewBag.Mensaje = "Este numero de documento ya esta registrado";
                    return View(CargarDatosIniciales());
                }
                else if (ObtenerCorreo(oCliente.Correo) != null)
                {
                    ViewBag.Mensaje = "Este correo ya esta registrado";
                    return View(CargarDatosIniciales());
                }
            }

            return RedirectToAction("Index");

        }

        [Authorize(Policy = "EditarClientesPermiso")]
        [HttpGet]
        public IActionResult Editar(string NroDocumento)
        {
            return View(CargarDatosEditar(NroDocumento));
        }

        [Authorize(Policy = "EditarClientesPermiso")]
        [HttpPost]
        public IActionResult Editar(Cliente oCliente)
        {
            _context.Clientes.Update(oCliente);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "VerDetallesClientesPermiso")]
        public IActionResult Detalles(string NroDocumento)
        {
            return View(CargarDatosEditar(NroDocumento));
        }

        [Authorize(Policy = "CambiarEstadoClientesPermiso")]
        [HttpPost]
        public IActionResult cambiarEstado(string? nroDocumento, bool estado)
        {

            if (!nroDocumento.IsNullOrEmpty())
            {

                var cliente = _context.Clientes.Find(nroDocumento);

                if (cliente != null)
                {
                    cliente.Estado = estado;

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

        public Cliente ObtenerDocumento(string documento)
        {
            var cliente = _context.Clientes
                .Where(u => u.NroDocumento == documento)
                .FirstOrDefault();

            if (cliente != null)
            {
                Cliente oCliente = new Cliente()
                {
                    Nombres = cliente.Nombres.ToString(),
                    Apellidos = cliente.Apellidos.ToString(),
                    Contrasena = cliente.Contrasena.ToString(),
                    Restablecer = cliente.Restablecer,
                    Confirmado = cliente.Confirmado,
                    Token = cliente.Token,
                };

                return oCliente;
            }

            return null;

        }

        public Cliente ObtenerCorreo(string correo)
        {
            var cliente = _context.Clientes
                .Where(u => u.Correo == correo)
                .FirstOrDefault();

            if (cliente != null)
            {
                Cliente oCliente = new Cliente()
                {
                    Nombres = cliente.Nombres.ToString(),
                    Apellidos = cliente.Apellidos.ToString(),
                    Contrasena = cliente.Contrasena.ToString(),
                    Restablecer = cliente.Restablecer,
                    Confirmado = cliente.Confirmado,
                    Token = cliente.Token,
                };

                return oCliente;
            }

            return null;
        }

        public bool RegistrarDB(Cliente oCiente)
        {
            bool respuesta = false;

            try
            {
                _context.Clientes.Add(oCiente);
                _context.SaveChanges();
                respuesta = true;

            }
            catch (Exception ex)
            {
                throw ex;
            }


            return respuesta;

        }

        public List<Cliente> ObtenerTodosLosClientes()
        {
            var clientes = _context.Clientes
                .Include(c => c.oRol)
                .ToList();

            return clientes;
        }

        public IActionResult Buscar(string searchTerm)
        {
            List<Cliente> resultados;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                resultados = ObtenerTodosLosClientes();
            }
            else
            {
                resultados = ObtenerResultadosDeLaBaseDeDatos(searchTerm);
            }

            return PartialView("_ResultadoBusquedaParcial", resultados);
        }

        private List<Cliente> ObtenerResultadosDeLaBaseDeDatos(string searchTerm)
        {
            var resultados = _context.Clientes
                                  .Include(c => c.oRol)
                                  .Where(c => c.Nombres.Contains(searchTerm) || c.Apellidos.Contains(searchTerm) || c.NroDocumento.Contains(searchTerm) || c.Correo.Contains(searchTerm))
                                  .ToList();

            return resultados;
        }

        public ClienteVM CargarDatosIniciales()
        {
            var clienteVM = new ClienteVM()
            {
                oCliente = new Cliente(),
                oListaTipoDocumento = _context.TipoDocumentos.Select(tDocumento => new SelectListItem()
                {
                    Text = tDocumento.NomTipoDcumento,
                    Value = tDocumento.IdTipoDocumento.ToString()
                }).ToList()
            };

            return clienteVM;
        }

        public ClienteVM CargarDatosEditar(string NroDocumento)
        {

            var cliente = _context.Clientes
                .FirstOrDefault(c => c.NroDocumento == NroDocumento);

            var clienteVM = new ClienteVM()
            {
                oCliente = cliente,
                oListaTipoDocumento = _context.TipoDocumentos.Select(tDocumento => new SelectListItem()
                {
                    Text = tDocumento.NomTipoDcumento,
                    Value = tDocumento.IdTipoDocumento.ToString()
                }).ToList()
            };

            return clienteVM;
        }

        public bool ConfirmarDB(string token)
        {
            bool respuesta = false;

            try
            {
                var cliente = _context.Clientes
                .Where(u => u.Token == token)
                .FirstOrDefault();

                cliente.Confirmado = true;

                _context.Clientes.Update(cliente);
                _context.SaveChanges();
                respuesta = true;
                return respuesta;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult validarCambioEstado(string nroDocumento)
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.NroDocumento == nroDocumento);
            var oRol = _context.Roles.Where(r => r.IdRol == cliente.IdRol).FirstOrDefault();

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
