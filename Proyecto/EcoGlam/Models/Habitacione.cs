using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoGlam.Models;

public partial class Habitacione
{
    public int? IdHabitacion { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public int? IdTipoHabitacion { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public string? Nombre { get; set; }

    public bool? Estado { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public double? Costo { get; set; }

    public virtual TipoHabitacione? oTipoHabitacion { get; set; }

    public virtual ICollection<ImagenHabitacion> ImagenHabitacions { get; set; } = new List<ImagenHabitacion>();

    public virtual ICollection<Paquete> Paquetes { get; set; } = new List<Paquete>();
}
