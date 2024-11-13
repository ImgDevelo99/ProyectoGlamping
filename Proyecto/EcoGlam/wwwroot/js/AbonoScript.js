function formatoSubTotal(input) {

    let value = input.value.replace(/\D/g, '');

    if (value === '') {
        input.value = '';
    } else {

        valueNumero = parseFloat(value)
        numeroFormateado = formatearMoneda(valueNumero)

        input.value = numeroFormateado;

    }

};

function prepararYEnviar() {

    var deuda = $('#inputDeuda').val();
    var pendiente = $('#inputPendiente').val();
    var subTotal = $('#inputSubTotal').val();
    var iva = $('#inputIva').val();
    var totalAbonado = $('#inputTotalAbonado').val();

    var deudaFloat = convertirMonedaAFloat(deuda);
    var pendienteFloat = convertirMonedaAFloat(pendiente);
    var subTotalFloat = convertirMonedaAFloat(subTotal);
    var ivaFloat = convertirMonedaAFloat(iva);
    var totalAbonadoFloat = convertirMonedaAFloat(totalAbonado);

    $('#inputDeuda').val(deudaFloat);
    $('#inputPendiente').val(pendienteFloat);
    $('#inputSubTotal').val(subTotalFloat);
    $('#inputIva').val(ivaFloat);
    $('#inputTotalAbonado').val(totalAbonadoFloat);

    $('#abonoForm').submit();

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