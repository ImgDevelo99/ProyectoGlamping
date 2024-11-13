using System;
using System.Collections.Generic;

namespace EcoGlam.Models;

public partial class ImagenServicio
{
    public int? IdImagenServi { get; set; }

    public int? IdImagen { get; set; }

    public int? IdServicio { get; set; }

    public virtual Imagene? oImagen { get; set; }

    public virtual Servicio? oServicio { get; set; }
}
