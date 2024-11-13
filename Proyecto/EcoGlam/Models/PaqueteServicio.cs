using System;
using System.Collections.Generic;

namespace EcoGlam.Models;

public partial class PaqueteServicio
{
    public int? IdPaqueteServicio { get; set; }

    public int? IdPaquete { get; set; }

    public int? IdServicio { get; set; }

    public double? Costo { get; set; }

    public virtual Paquete? oPaquete { get; set; }

    public virtual Servicio? oServicio { get; set; }
}
