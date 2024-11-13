using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoGlam.Models;

public partial class Servicio
{
    public int? IdServicio { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public int? IdTipoServicio { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    [MaxLength(20, ErrorMessage = "El campo no puede tener más de 20 caracteres.")]
    [MinLength(5, ErrorMessage = "El campo debe tener al menos 5 caracteres.")]
    public string? NomServicio { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public double? Costo { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    [MaxLength(100, ErrorMessage = "El campo no puede tener más de 100 caracteres.")]
    [MinLength(5, ErrorMessage = "El campo debe tener al menos 5 caracteres.")]
    public string? Descripcion { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<DetalleReservaServicio> DetalleReservaServicios { get; set; } = new List<DetalleReservaServicio>();

    public virtual TipoServicio? oTipoServicio { get; set; }

    public virtual ICollection<ImagenServicio> ImagenServicios { get; set; } = new List<ImagenServicio>();

    public virtual ICollection<PaqueteServicio> PaqueteServicios { get; set; } = new List<PaqueteServicio>();
}
