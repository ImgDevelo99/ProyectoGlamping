using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcoGlam.Models.ViewModels
{
    public class HabitacionVM
    {
        public Habitacione oHabitacion { get; set; }
        public List<SelectListItem> oListaTipoHabitaciones { get; set; }

    }
}
