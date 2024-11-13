const formulario = document.getElementById('formularioRegistro');
const inputs = document.querySelectorAll('#formularioRegistro input');
const select = document.getElementById('tipoDocumento');

const expresiones = {
	nombreApellido: /^[a-zA-ZÀ-ÿ\s]{1,40}$/,
	password: /^.{4,12}$/,
	correo: /^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$/,
	Documento: /^\d{10}$/,
	celular: /^\d{10}$/
}

const campos = {
	NumeroDoc: true,
	celular: true,
	contrasena: true,
	correo: true,
	nombres: true,
	apellidos: true
}

const validarFormulario = (e) => {
	switch (e.target.name) {
		case "oCliente.NroDocumento":
			validarCampo(expresiones.Documento, e.target, 'NumeroDoc');
			break;
		case "oCliente.Celular":
			validarCampo(expresiones.celular, e.target, 'celular');
			break;
		case "oCliente.Contrasena":
			validarCampo(expresiones.password, e.target, 'contrasena');
			validarPassword2();
			break;
		case "oCliente.ConfirmarContrasena":
			validarPassword2();
			break;
		case "oCliente.Correo":
			validarCampo(expresiones.correo, e.target, 'correo');
			break;
		case "oCliente.Nombres":
			validarCampo(expresiones.nombreApellido, e.target, 'nombres');
			break;
		case "oCliente.Apellidos":
			validarCampo(expresiones.nombreApellido, e.target, 'apellidos');
			break;
	}
}

const validarCampo = (expresion, input, campo) => {
	if (expresion.test(input.value)) {
		document.getElementById(`${campo}`).classList.remove('ErrorInput');
		document.querySelector(`.formulario__input-error${campo}`).classList.remove('formulario__input-error-activo');
		campos[campo] = true;
	} else {
		document.getElementById(`${campo}`).classList.add('ErrorInput');
		document.querySelector(`.formulario__input-error${campo}`).classList.add('formulario__input-error-activo');
		campos[campo] = false;
	}
}

const validarPassword2 = () => {
	const inputPassword1 = document.getElementById('contrasena');
	const inputPassword2 = document.getElementById('contrasena2');

	if (inputPassword1.value !== inputPassword2.value) {
		document.getElementById(`contrasena2`).classList.add('ErrorInput');
		document.querySelector(`.formulario__input-errorcontrasena2`).classList.add('formulario__input-error-activo');
		campos['password'] = false;
	} else {
		document.getElementById(`contrasena2`).classList.remove('ErrorInput');
		document.querySelector(`.formulario__input-errorcontrasena2`).classList.remove('formulario__input-error-activo');
		campos['password'] = true;
	}
}

inputs.forEach((input) => {
	input.addEventListener('keyup', validarFormulario);
	input.addEventListener('blur', validarFormulario);
});

formulario.addEventListener('submit', (e) => {
	e.preventDefault();

	const select = document.getElementById('tipoDocumento');

	const selectVacio = select.value == 0;

	if (campos.NumeroDoc && campos.celular && campos.contrasena && campos.correo && campos.nombres && campos.apellidos && !selectVacio) {

		formulario.submit();
	} else {
		document.getElementById('mensajeError').style.display = 'block';
	}
});