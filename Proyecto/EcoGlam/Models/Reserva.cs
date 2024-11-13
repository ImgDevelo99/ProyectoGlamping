using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoGlam.Models;

public partial class Reserva
{
    public int? IdReserva { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public string? NroDocumentoCliente { get; set; }

    public int? NroDocumentoUsuario { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public DateOnly? FechaReserva { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public DateOnly? FechaInicio { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public DateOnly? FechaFinalizacion { get; set; }

    public double? SubTotal { get; set; }

    public double? Descuento { get; set; }

    public double? Iva { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public double? MontoTotal { get; set; }

    [Required(ErrorMessage = "Campo requerido")]
    public int? MetodoPago { get; set; }

    public int? NroPersonas { get; set; }

    public int? IdEstadoReserva { get; set; }

    public virtual ICollection<Abono> Abonos { get; set; } = new List<Abono>();

    public virtual ICollection<DetalleReservaPaquete> DetalleReservaPaquetes { get; set; } = new List<DetalleReservaPaquete>();

    public virtual ICollection<DetalleReservaServicio> DetalleReservaServicios { get; set; } = new List<DetalleReservaServicio>();

    public virtual EstadosReserva? oEstadoReserva { get; set; }

    public virtual MetodoPago? oMetodoPago { get; set; }

    public virtual Cliente? oCliente { get; set; }

    public virtual Usuario? oUsuario { get; set; }
}
