
$('#selectServicios').change(function () {
    var selectedServicioId = $(this).val();

    $.ajax({
        url: '/Paquetes/ObtenerCostoServicio',
        type: 'GET',
        data: { servicioId: selectedServicioId },
        dataType: 'json',
        success: function (data) {

            var costo = data.costo;
            var costoFormateado = formatoCostoInput(costo);
            $('#inputCosto').val(costoFormateado);
        },
        error: function () {
            console.error('Error al obtener el costo del servicio.');
        }
    });
});

$('#selectHabitacion').change(function () {
    var selectedHabitacionId = $(this).val();

    $.ajax({
        url: '/Paquetes/ObtenerCostoHabitacion',
        type: 'GET',
        data: { habitacionId: selectedHabitacionId },
        dataType: 'json',
        success: function (data) {

            var costo = data.costo;
            var costoFormateado = formatoCostoInput(costo);
            $('#inputCostoHabitacion').val(costoFormateado);
            actualizarCostoTotal();
        },
        error: function () {
            console.error('Error al obtener el costo de la habitacion.');
        }
    });
    
});

function calcularCostoServicios() {
    var total = 0;
    serviciosSeleccionados.forEach(servicio => {
        total += convertirMonedaAFloat(servicio.costo);
    });

    return total;
}

var serviciosSeleccionados = [];

$('#btnAgregarServicio').click(function () {
    var selectedServicioId = $('#selectServicios').val();
    var selectedServicioText = $('#selectServicios option:selected').text();
    var selectedServicioCosto = convertirMonedaAFloat($('#inputCosto').val());

    if (selectedServicioId) {
        if (!serviciosSeleccionados.some(servicio => servicio.id == selectedServicioId)) {

            serviciosSeleccionados.push({

                id: selectedServicioId,
                nombre: selectedServicioText,
                costo: selectedServicioCosto

            });

            var costoFormateado = parseFloat(selectedServicioCosto).toLocaleString('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 });

            var rowHtml = '<tr>' +
                '<td>' + selectedServicioText + '</td>' +
                '<td>' + costoFormateado + '</td>' +
                '<td><button type="button" class="btn btn-danger btn-sm" onclick="eliminarServicio(this)">Eliminar</button></td>' +
                '</tr>';
            $('#tablaServiciosSeleccionados tbody').append(rowHtml);

            actualizarInputServiciosSeleccionados();
        }
    }
    actualizarCostoTotal();
});

function eliminarServicio(btn) {

    var rowIndex = $(btn).closest('tr').index();
    serviciosSeleccionados.splice(rowIndex, 1);

    $(btn).closest('tr').remove();

    actualizarInputServiciosSeleccionados();
    actualizarCostoTotal();
}

function actualizarInputServiciosSeleccionados() {
    $('#serviciosSeleccionados').val(JSON.stringify(serviciosSeleccionados));
}

function actualizarCostoTotal() {
    var costoHabitacion = obtenerCostoHabitacion();
    var costoServicios = calcularCostoServicios();
    var costoTotal = costoHabitacion + costoServicios;

    $('#inputCosto1').val(formatoCostoInput(costoTotal));
}

function obtenerCostoHabitacion() {
    var habitacionId = $('#selectHabitacion').val();
    var costoHabitacion = 0;

    $.ajax({
        url: '/Paquetes/ObtenerCostoHabitacion',
        type: 'GET',
        data: { habitacionId: habitacionId },
        dataType: 'json',
        async: false,
        success: function (data) {
            costoHabitacion = data.costo;
        },
        error: function () {
            console.error('Error al obtener el costo de la habitación.');
        }
    });

    return costoHabitacion;
}

function prepararYEnviar() {

    var costo = convertirMonedaAFloat($('#inputCosto1').val());

    $('#inputCosto1').val(costo);

    $('#paqueteForm').submit();

}

$('#inputImagenes').on('change', function (event) {
    $('#imagenesSlider').empty();

    var files = event.target.files

    for (var i = 0; i < files.length; i++) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#imagenesSlider').append('<input type="radio" name="s" style="background-image: url(' + e.target.result + ');">')
            $('#imagenesSlider input').first().prop('checked', true);
        }
        reader.readAsDataURL(files[i]);
    }

});

