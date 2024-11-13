function prepararYEnviar() {

    var costo = $('#inputCosto').val();

    var costoFloat = convertirMonedaAFloat(costo)

    $('#inputCosto').val(costoFloat)

    $('#servicioForm').submit();
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