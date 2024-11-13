using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoGlam.Models;

public partial class Abono
{
    public int? IdAbono { get; set; }

    public int? IdReserva { get; set; }

    public DateOnly? FechaAbono { get; set; }

    public double? ValorDeuda { get; set; }

    public double? Porcentaje { get; set; }

    public double? Pendiente { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public double? SubTotal { get; set; }

    public double? Iva { get; set; }

    public double? CantAbono { get; set; }

    public bool? Estado { get; set; }

    public virtual Reserva? oReserva { get; set; }

    public virtual ICollection<ImagenAbono> ImagenAbonos { get; set; } = new List<ImagenAbono>();
}
