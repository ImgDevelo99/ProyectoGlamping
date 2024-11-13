using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoGlam.Models;

public partial class Cliente
{
    public string NroDocumento { get; set; } = null!;

    public int? IdTipoDocumento { get; set; }

    public string? Nombres { get; set; }

    public string? Apellidos { get; set; }

    public string? Celular { get; set; }

    public string? Correo { get; set; }

    public string? Contrasena { get; set; }

    [NotMapped]
    public string? ConfirmarContrasena { get; set; }

    public bool Restablecer { get; set; }

    public bool Confirmado { get; set; }

    public string? Token { get; set; }

    public bool? Estado { get; set; }

    public int? IdRol { get; set; }

    public virtual Role? oRol { get; set; }

    public virtual TipoDocumento? oTipoDocumento { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
