var permisosSeleccionados = {};
const formulario = document.getElementById('formularioRol');

function guardarEstado(checkbox) {
    var permisoId = checkbox.dataset.permisoId;

    if (checkbox.checked) {
        permisosSeleccionados[permisoId] = true;
    } else {
        delete permisosSeleccionados[permisoId]
    }
}

function buscarAhoraPermiso(searchTerm) {
    $.ajax({
        url: '/Roles/BuscarPermiso',
        type: 'GET',
        data: { searchTerm: searchTerm },
        dataType: 'html',
        success: function (data) {
            $('#searchResults').html(data);

            $('.checkbox-permiso').each(function () {
                var permisoId = $(this).data('permiso-id');
                $(this).prop('checked', permisosSeleccionados.hasOwnProperty(permisoId) && permisosSeleccionados[permisoId]);
            });
        },
        error: function () {
            console.error('Error al realizar la búsqueda.');
        }
    });
};

function buscarAhoraPermisoDetalle(searchTerm) {
    $.ajax({
        url: '/Roles/BuscarPermiso',
        type: 'GET',
        data: { searchTerm: searchTerm },
        dataType: 'html',
        success: function (data) {
            $('#searchResults').html(data);

            $('.checkbox-permiso').each(function () {
                $(this).prop('checked', true);
                $(this).prop('disabled', true);
            });
        },
        error: function () {
            console.error('Error al realizar la búsqueda.');
        }
    });
};

formulario.addEventListener('submit', (e) => {
    e.preventDefault();
    var idsPermisosSeleccionados = Object.keys(permisosSeleccionados);
    $('#permisosSeleccionados').val(JSON.stringify(idsPermisosSeleccionados));

    formulario.submit();
});