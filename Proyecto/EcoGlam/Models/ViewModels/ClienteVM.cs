using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcoGlam.Models.ViewModels
{
    public class ClienteVM
    {
        public Cliente oCliente {  get; set; }
        public List<SelectListItem> oListaTipoDocumento { get; set; }
        
    }
}
