using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoGlam.Models;

public partial class TipoHabitacione
{
    public int? IdTipoHabitacion { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    [MaxLength(20, ErrorMessage = "El campo no puede tener más de 20 caracteres.")]
    [MinLength(5, ErrorMessage = "El campo debe tener al menos 5 caracteres.")]
    public string? NomTipoHabitacion { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    [RegularExpression(@"^\d+$", ErrorMessage = "El campo solo puede contener números.")]
    public int? NumeroPersonas { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<Habitacione> Habitaciones { get; set; } = new List<Habitacione>();
}
