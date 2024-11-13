function formatearMoneda(valor) {

    var opcionesFormato = {
        style: 'currency',
        currency: 'COP',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }

    return valor.toLocaleString('es-CO', opcionesFormato);

};

function convertirMonedaAFloat(monedaFormateada) {

    const monedaTexto = monedaFormateada.toString();
    var valorFiltrado = monedaTexto.replace('$', '');

    while (valorFiltrado.includes('.')) {
        valorFiltrado = valorFiltrado.replace('.', '')
    };

    const valorFloat = parseFloat(valorFiltrado);

    return isNaN(valorFloat) ? 0 : valorFloat;
};

function formatoCosto(input) {

    let value = input.value.replace(/\D/g, '');

    if (value === '') {
        input.value = '';
    } else {

        valueNumero = parseFloat(value)
        numeroFormateado = formatearMoneda(valueNumero)

        input.value = numeroFormateado;

    }

};

function formatoNumero(input) {

    let value = input.value;

    if (isNaN(value) || value < 1) {
        input.value = ""
    }

};

function reemplazarEspacioVacioPorUno(input) {
    if (input.value.trim() === '') {
        input.value = '1';
    }
}

function formatoCostoInput(costo) {

    let costoFormateado = formatearMoneda(costo);

    return costoFormateado;
}

