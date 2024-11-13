using System;
using System.Collections.Generic;

namespace EcoGlam.Models;

public partial class PermisosRole
{
    public int? IdPermisosRoles { get; set; }

    public int? IdRol { get; set; }

    public int? IdPermiso { get; set; }

    public virtual Permiso? oPermiso { get; set; }

    public virtual Role? oRol { get; set; }
}
