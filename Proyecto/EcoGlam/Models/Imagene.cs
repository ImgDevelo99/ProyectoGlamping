using System;
using System.Collections.Generic;

namespace EcoGlam.Models;

public partial class Imagene
{
    public int? IdImagen { get; set; }

    public string? UrlImagen { get; set; }

    public virtual ICollection<ImagenAbono> ImagenAbonos { get; set; } = new List<ImagenAbono>();

    public virtual ICollection<ImagenHabitacion> ImagenHabitacions { get; set; } = new List<ImagenHabitacion>();

    public virtual ICollection<ImagenPaquete> ImagenPaquetes { get; set; } = new List<ImagenPaquete>();

    public virtual ICollection<ImagenServicio> ImagenServicios { get; set; } = new List<ImagenServicio>();
}
