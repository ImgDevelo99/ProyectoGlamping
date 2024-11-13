using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoGlam.Models;

public partial class Paquete
{
    public int? IdPaquete { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public string? NomPaquete { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public double? Costo { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public int? IdHabitacion { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public bool? Estado { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public string? Descripcion { get; set; }

    public virtual ICollection<DetalleReservaPaquete> DetalleReservaPaquetes { get; set; } = new List<DetalleReservaPaquete>();

    public virtual Habitacione? oHabitacion { get; set; }

    public virtual ICollection<ImagenPaquete>? ImagenPaquetes { get; set; } = new List<ImagenPaquete>();

    public virtual ICollection<PaqueteServicio>? PaqueteServicios { get; set; } = new List<PaqueteServicio>();
}
