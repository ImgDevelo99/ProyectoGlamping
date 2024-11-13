$('#fechaInicio').add($('#fechaFin')).on('change', function () {

    validarFechas();
    actualizarInfoCosto();

});

$('#selectServicios').change(function () {
    var selectedServicioId = $(this).val();

    $.ajax({
        url: '/Reservas/ObtenerCostoServicio',
        type: 'GET',
        data: { servicioId: selectedServicioId },
        dataType: 'json',
        success: function (data){
            $('#inputCostoServicio').val(formatoCostoInput(data.costo));
        },
        error: function () {
            console.error('Error al obtener el costo del servicio.');
        }
    })
});

var paqueteSeleccionado = [];
var serviciosSeleccionados = [];

$('#selectPaquetes').change(actualizarInfoPaquete);

function actualizarInfoPaquete() {

    var selectedPaqueteId = $('#selectPaquetes').val();

    $.ajax({
        url: '/Reservas/ObtenerInfoPaquete',
        type: 'GET',
        data: { paqueteId: selectedPaqueteId },
        dataType: 'json',
        success: function (data) {
            $('#inputCostoPaquete').val(formatoCostoInput(data.costo));
            $('#inputHabitacion').val(data.habitacion);
            $('#nroPersonas').val(data.nroPersonas);

            var selectedPaqueteCosto = convertirMonedaAFloat($('#inputCostoPaquete').val());

            paqueteSeleccionado = [];

            paqueteSeleccionado.push({
                id: selectedPaqueteId,
                costo: selectedPaqueteCosto
            });

            actualizarInputPaqueteSeleccionado();
            actualizarInfoCosto();
        },
        error: function () {
            console.error('Error al obtener la informacion del paquete.');
        }
    })

    $.ajax({
        url: '/Reservas/ObtenerServiciosPaquete',
        type: 'GET',
        data: { paqueteId: selectedPaqueteId },
        dataType: 'json',
        success: function (data) {

            $('#tablaServiciosPaquete tbody').empty();

            $.each(data, function (index, servicio) {

                var costoFormateado = parseFloat(servicio.costo).toLocaleString('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 });
                var rowHtml = '<tr>' +
                    '<td>' + servicio.nombre + '</td>' +
                    '<td>' + costoFormateado + '</td>' +
                    '</tr>';
                $('#tablaServiciosPaquete tbody').append(rowHtml);
            });
        },
        error: function () {
            console.error('Error al obtener los servicios del paquete.');
        }
    });

    $.ajax({
        url: '/Reservas/ObtenerServiciosPorPaquete',
        type: 'GET',
        data: { paqueteId: selectedPaqueteId },
        dataType: 'json',
        success: function (data) {

            var servicios = $.parseJSON(data);

            selectedServiciosAdicionales.append('<option value="" disabled selected>' + '-- Seleccionar --' + '</option>');

            $.each(servicios, function (index, servicio) {
                selectedServiciosAdicionales.append('<option value="' + servicio.IdServicio + '">' + servicio.NomServicio + '</option>');
            });

            $('#inputCostoServicio').val('');
            serviciosSeleccionados = [];
            actualizarInputPaqueteSeleccionado();

            var tbody = $('#tablaServiciosSeleccionados tbody');
            tbody.empty();

            actualizarInfoCosto();

        },
        error: function () {
            console.error('Error al obtener la lista de servicios.');
        }
    });

};

function actualizarInputPaqueteSeleccionado() {
    $('#paqueteSeleccionado').val(JSON.stringify(paqueteSeleccionado));
}

function buscarCliente(searchTerm) {
    $.ajax({
        url: '/Reservas/BuscarCliente',
        type: 'GET',
        data: { searchTerm: searchTerm },
        dataType: 'json',
        success: function (data) {
            $('#inputNombre').val(data.nombre);
            $('#inputApellido').val(data.apellido);
            $('#inputDireccion').val(data.direccion);
            $('#inputTDocumento').val(data.tipoDocumento);
            $('#inputCorreo').val(data.correo);
            $('#inputCelular').val(data.celular);
            $('#spanDocumento').val(data.errorMessage);

            if (data.nombre === "" && searchTerm.trim() !== "") {
                $('#spanDocumento').text("El cliente no existe");
            } else if (data.estado == "false") {
                $('#spanDocumento').text("El cliente esta inhabilitado");
            }
            else {
                $('#spanDocumento').text("");
            }

        },
        error: function () {
            console.error('Error al realizar la búsqueda.');
        }
    });
}


$('#btnAgregarServicio').click(function () {
    var selectedServicioId = $('#selectServicios').val();
    var selectedServicioText = $('#selectServicios option:selected').text();
    var selectedServicioCosto = convertirMonedaAFloat($('#inputCostoServicio').val());
    var cantidadServicio = 1;

    if (selectedServicioId) {

        if (!serviciosSeleccionados.some(servicio => servicio.id == selectedServicioId)) {

            serviciosSeleccionados.push({

                id: selectedServicioId,
                nombre: selectedServicioText,
                costo: selectedServicioCosto,
                cantidad: cantidadServicio

            });

            var costoFormateado = parseFloat(selectedServicioCosto).toLocaleString('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 });

            var rowHtml = '<tr>' +
                '<td>' + selectedServicioText + '</td>' +
                '<td>' + costoFormateado + '</td>' +
                '<td> <input style="max-width:80px;text-align: center" value="1" onchange="cambiarCantidad(this)" onblur="reemplazarEspacioVacioPorUno(this)" oninput="formatoNumero(this)"> </td>' +
                '<td><button type="button" class="btn btn-danger btn-sm" onclick="eliminarServicio(this)">Eliminar</button></td>' +
                '</tr>';
            $('#tablaServiciosSeleccionados tbody').append(rowHtml);

        }

        actualizarInputServiciosSeleccionados();
        actualizarInfoCosto();
        
    }
});

function cambiarCantidad(input) {

    var Index = $(input).closest('tr').index();
    var nuevaCantidad = parseInt($(input).val());
    serviciosSeleccionados[Index].cantidad = nuevaCantidad;

    actualizarInputServiciosSeleccionados();
    actualizarInfoCosto();

}

function actualizarInfoCosto() {

    var dias = CalcularDiferenciaDias() || 1;
    var costoPaquete = (convertirMonedaAFloat($('#inputCostoPaquete').val()) || 0) * dias;
    var costoServicios = calcularCostosServicio();
    var subTotal = costoPaquete + costoServicios;
    var descuento = parseFloat($('#descuento').val()) || 0;
    var valorDescuento = subTotal * (1 - (descuento/100))
    var iva = Math.round((valorDescuento) * 0.19);
    var total = Math.round(valorDescuento + iva);

    $('#inputSubTotal').val(formatoCostoInput(subTotal));
    $('#inputIva').val(formatoCostoInput(iva));
    $('#inputCostoTotal').val(formatoCostoInput(total));
};

function calcularCostosServicio() {
    var total = 0;
    serviciosSeleccionados.forEach(servicio => {
        total += (parseFloat(servicio.costo) * ((servicio.cantidad) || 1));
    })
    return total;
};

function eliminarServicio(btn) {

    var rowIndex = $(btn).closest('tr').index();
    serviciosSeleccionados.splice(rowIndex, 1);

    $(btn).closest('tr').remove();

    actualizarInputServiciosSeleccionados();
    actualizarInfoCosto();

};

function actualizarInputServiciosSeleccionados() {
    $('#serviciosSeleccionados').val(JSON.stringify(serviciosSeleccionados));
};

function formatoDescuento(input) {

    let value = input.value.replace(/\D/g, '');

    if (value === '') {
        input.value = '';
    } else {
        value = Math.min(Math.max(parseInt(value), 0), 100);
        input.value = value;
    }

};

function prepararYEnviar() {

    var subtotal = $('#inputSubTotal').val();
    var iva = $('#inputIva').val();
    var total = $('#inputCostoTotal').val();

    $('#inputSubTotal').val(convertirMonedaAFloat(subtotal));
    $('#inputIva').val(convertirMonedaAFloat(iva));
    $('#inputCostoTotal').val(convertirMonedaAFloat(total));

    $('#reservaForm').submit();
}

function CalcularDiferenciaDias() {

    var fechaInicio = $('#fechaInicio').val();
    var fechaFin = $('#fechaFin').val()

    var fechaInicioObj = new Date(fechaInicio);
    var fechaFinObj = new Date(fechaFin);

    var diferenciaMilisegundos = fechaFinObj - fechaInicioObj;

    var diferenciaDias = Math.floor(diferenciaMilisegundos / (1000 * 60 * 60 * 24));

    return diferenciaDias;

}

function validarFechas() {

    var fechaInicio = $('#fechaInicio').val();
    var fechaFin = $('#fechaFin').val();

    if (fechaInicio && fechaFin) {

        var fechaInicioObj = new Date(fechaInicio);
        var fechaFinObj = new Date(fechaFin);

        if (fechaInicioObj > fechaFinObj) {

            $('#fechaFinSpan').html("La fecha de finalizacion debe ser posterior a la fecha de inicio");
            $('#fechaFin').val("");
            return;
        } else {
            $('#fechaFinSpan').html("");
        }


    }

}

