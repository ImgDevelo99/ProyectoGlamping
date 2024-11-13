using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoGlam.Models;

public partial class TipoServicio
{
    public int? IdTipoServicio { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    [MaxLength(20, ErrorMessage = "El campo no puede tener más de 20 caracteres.")]
    [MinLength(5, ErrorMessage = "El campo debe tener al menos 5 caracteres.")]
    public string? NombreTipoServicio { get; set; }

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
