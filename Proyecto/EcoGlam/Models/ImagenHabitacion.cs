using System;
using System.Collections.Generic;

namespace EcoGlam.Models;

public partial class ImagenHabitacion
{
    public int? IdImagenHabi { get; set; }

    public int? IdImagen { get; set; }

    public int? IdHabitacion { get; set; }

    public virtual Habitacione? oHabitacion { get; set; }

    public virtual Imagene? oImagen { get; set; }
}
