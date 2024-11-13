using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcoGlam.Models.ViewModels
{
    public class UsuarioVM
    {
        public Usuario oUsuario { get; set; }
        public List<SelectListItem> oListaTipoDocumento { get; set; }
    }
}
