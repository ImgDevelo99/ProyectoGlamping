function resumenReservas(formatoFecha) {
    $.ajax({
        url: "/Dashboard/resumenReservas",
        method: "GET",
        data: { formatoFecha: formatoFecha },
        dataType: "json",
        success: function (dataJson) {

            const labels = dataJson.map((item) => { return item.fecha });
            const datos = dataJson.map((item) => { return item.cantidad });

            const data = {
                labels: labels,
                datasets: [{
                    label: 'Reservas Realizadas',
                    data: datos,
                    backgroundColor: [
                        'rgba(255, 99, 132, 0.2)',
                        'rgba(255, 159, 64, 0.2)',
                        'rgba(255, 205, 86, 0.2)',
                        'rgba(75, 192, 192, 0.2)',
                        'rgba(54, 162, 235, 0.2)',
                        'rgba(153, 102, 255, 0.2)',
                        'rgba(201, 203, 207, 0.2)'
                    ],
                    borderColor: [
                        'rgb(255, 99, 132)',
                        'rgb(255, 159, 64)',
                        'rgb(255, 205, 86)',
                        'rgb(75, 192, 192)',
                        'rgb(54, 162, 235)',
                        'rgb(153, 102, 255)',
                        'rgb(201, 203, 207)'
                    ],
                    borderWidth: 1
                }]
            };

            const config = {
                type: 'bar',
                data: data,
                options: {
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                },
            };

            const canva = document.getElementById('chart1');
            const grafico = new Chart(canva, config)
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function resumenPaquetes(formatoFecha) {
    $.ajax({
        url: "/Dashboard/resumenPaquetes",
        method: "GET",
        data: { formatoFecha: formatoFecha },
        dataType: "json",
        success: function (dataJson) {
            const labels = dataJson.map((item) => { return item.nombrePaquete });
            const datos = dataJson.map((item) => { return item.cantidad });
            const data = {
                labels: labels,
                datasets: [{
                    label: 'Cantidad Reservados',
                    data: datos,
                    backgroundColor: [
                        'rgb(255, 99, 132)',
                        'rgb(75, 192, 192)',
                        'rgb(255, 205, 86)',
                        'rgb(201, 203, 207)',
                        'rgb(54, 162, 235)'
                    ]
                }]
            };

            const config = {
                type: 'polarArea',
                data: data,
                options: {}
            };

            const canva = document.getElementById('chart2');
            const grafico = new Chart(canva, config)
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function resumenServicios(formatoFecha) {
    $.ajax({
        url: "/Dashboard/resumenServicios",
        method: "GET",
        data: { formatoFecha: formatoFecha },
        dataType: "json",
        success: function (dataJson) {
            const labels = dataJson.map((item) => { return item.nombreServicio });
            const datos = dataJson.map((item) => { return item.cantidad });
            const data = {
                labels: labels,
                datasets: [{
                    label: 'Cantidad Reservados',
                    data: datos,
                    backgroundColor: [
                        'rgb(255, 99, 132)',
                        'rgb(75, 192, 192)',
                        'rgb(255, 205, 86)',
                        'rgb(201, 203, 207)',
                        'rgb(54, 162, 235)'
                    ]
                }]
            };

            const config = {
                type: 'polarArea',
                data: data,
                options: {}
            };

            const canva = document.getElementById('chart3');
            const grafico = new Chart(canva, config)
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function resumenTipoHabi(formatoFecha) {
    $.ajax({
        url: "/Dashboard/resumenTipoHabi",
        method: "GET",
        data: { formatoFecha: formatoFecha },
        dataType: "json",
        success: function (dataJson) {
            const labels = dataJson.map((item) => { return item.nombreTipoHabitacion });
            const datos = dataJson.map((item) => { return item.cantidad });
            const data = {
                labels: labels,
                datasets: [{
                    label: 'Cantidad Reservados',
                    data: datos,
                    backgroundColor: [
                        'rgb(255, 99, 132)',
                        'rgb(75, 192, 192)',
                        'rgb(255, 205, 86)',
                        'rgb(201, 203, 207)',
                        'rgb(54, 162, 235)'
                    ]
                }]
            };

            const config = {
                type: 'polarArea',
                data: data,
                options: {}
            };

            const canva = document.getElementById('chart4');
            const grafico = new Chart(canva, config)
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function resumenEstadosReservas(formatoFecha) {
    $.ajax({
        url: "/Dashboard/resumenEstadosReserva",
        method: "GET",
        data: { formatoFecha: formatoFecha },
        dataType: "json",
        success: function (dataJson) {
            const labels = dataJson.map((item) => { return item.nombreEstado });
            const datos = dataJson.map((item) => { return item.cantidad });
            const data = {
                labels: labels,
                datasets: [{
                    label: 'Estados de Reservas',
                    data: datos,
                    backgroundColor: [
                        'rgb(255, 99, 132)',
                        'rgb(75, 192, 192)',
                        'rgb(255, 205, 86)',
                        'rgb(201, 203, 207)',
                        'rgb(54, 162, 235)'
                    ]
                }]
            };

            const config = {
                type: 'pie',
                data: data,
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                        title: {
                            display: true,
                            text: 'Estados de las Reservas'
                        }
                    }
                },
            };

            const canva = document.getElementById('chart5');
            const grafico = new Chart(canva, config)
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function infoMiniBox(formatoFecha) {
    $.ajax({
        url: "/Dashboard/infoBasicaDash",
        method: "GET",
        data: { formatoFecha: formatoFecha },
        dataType: "json",
        success: function (dataJson) {
            const reservasActivas = $('#reservasActivas');
            const reservasRealizadas = $('#reservasRealizadas');
            const reservasFinalizadas = $('#reservasFinalizadas');
            const Ingresos = $('#ingresos');

            reservasActivas.text(dataJson.reservasActivas);
            reservasRealizadas.text(dataJson.reservasRealizadas);
            reservasFinalizadas.text(dataJson.reservasFinalizadas);
            Ingresos.text(formatearMoneda(dataJson.ingresos));
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


var formatoFechaIndex = localStorage.getItem('formatoFechaIndex');
if (formatoFechaIndex !== null) {
    $('#selectFecha').val(formatoFechaIndex);
}

resumenReservas(formatoFechaIndex);
resumenPaquetes(formatoFechaIndex);
resumenServicios(formatoFechaIndex);
resumenTipoHabi(formatoFechaIndex);
resumenEstadosReservas(formatoFechaIndex);
infoMiniBox(formatoFechaIndex);


$('#selectFecha').change(function () {

    var formatoFechaIndex = $('#selectFecha').val();

    localStorage.setItem('formatoFechaIndex', formatoFechaIndex);

    window.location.reload();
});


