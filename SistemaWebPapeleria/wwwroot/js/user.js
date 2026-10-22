// user.js — lógica de la vista de usuarios

let correoOriginalEditar = '';
let codigoEditarVerificado = false;

function abrirEditar(btn) {
    const d = btn.dataset;
    document.getElementById('editUserId').value = d.id;
    document.getElementById('editName').value = d.nombre;
    document.getElementById('editLastName').value = d.apellido;
    document.getElementById('editEmail').value = d.email;
    document.getElementById('editRoleId').value = d.rolId;
    document.getElementById('modalEditar').style.display = 'flex';

    correoOriginalEditar = d.email.toLowerCase();
    codigoEditarVerificado = false;
    document.getElementById('btnVerificarEditEmail').classList.add('d-none');
    document.getElementById('pasoCodigoEditar').classList.add('d-none');
    document.getElementById('btnGuardarEdicion').disabled = false;
}

function onEditEmailChange() {
    const nuevoEmail = document.getElementById('editEmail').value.toLowerCase();
    const btn = document.getElementById('btnVerificarEditEmail');

    if (nuevoEmail !== correoOriginalEditar && nuevoEmail.includes('@')) {
        btn.classList.remove('d-none');
        codigoEditarVerificado = false;
        document.getElementById('btnGuardarEdicion').disabled = true;
        document.getElementById('pasoCodigoEditar').classList.add('d-none');
    } else {
        btn.classList.add('d-none');
        document.getElementById('pasoCodigoEditar').classList.add('d-none');
        document.getElementById('btnGuardarEdicion').disabled = false;
    }
}

async function enviarCodigoEditarUsuario() {
    const email = document.getElementById('editEmail').value;
    const userId = document.getElementById('editUserId').value;
    const btn = document.getElementById('btnVerificarEditEmail');
    btn.disabled = true;
    btn.textContent = 'Enviando...';

    const formData = new URLSearchParams();
    formData.append('email', email);
    formData.append('userId', userId);

    try {
        const res = await fetch('/User/EnviarCodigoEdicion', { method: 'POST', headers: csrfHeader(), body: formData });
        const data = await res.json();

        if (res.ok) {
            showToast(data.mensaje, 'success');
            document.getElementById('pasoCodigoEditar').classList.remove('d-none');
            btn.textContent = 'Reenviar código';
            btn.disabled = false;
        } else {
            showToast(data.mensaje || 'Error al enviar el código.', 'error');
            btn.textContent = 'Verificar';
            btn.disabled = false;
        }
    } catch (e) {
        showToast('Error de conexión.', 'error');
        btn.textContent = 'Verificar';
        btn.disabled = false;
    }
}

async function confirmarCodigoEditarUsuario() {
    const code = document.getElementById('inputCodigoEditar').value;
    if (!code) {
        showToast('Ingresa el código recibido.', 'error');
        return;
    }

    const formData = new URLSearchParams();
    formData.append('code', code);

    try {
        const res = await fetch('/User/VerificarCodigoEdicion', { method: 'POST', headers: csrfHeader(), body: formData });
        const data = await res.json();

        if (res.ok) {
            showToast(data.mensaje, 'success');
            codigoEditarVerificado = true;
            document.getElementById('btnGuardarEdicion').disabled = false;
            document.getElementById('pasoCodigoEditar').classList.add('d-none');
            document.getElementById('btnVerificarEditEmail').classList.add('d-none');
        } else {
            showToast(data.mensaje || 'Código incorrecto.', 'error');
        }
    } catch (e) {
        showToast('Error de conexión.', 'error');
    }
}

let codigoCrearVerificado = false;

async function enviarCodigoCrearUsuario() {
    const email = document.getElementById('inputEmailCrear').value;
    if (!email) {
        showToast('Ingresa un correo primero.', 'error');
        return;
    }

    const btn = document.getElementById('btnVerificarCorreo');
    btn.disabled = true;
    btn.textContent = 'Enviando...';

    const formData = new URLSearchParams();
    formData.append('email', email);

    try {
        const res = await fetch('/User/EnviarCodigoVerificacion', { method: 'POST', headers: csrfHeader(), body: formData });
        const data = await res.json();

        if (res.ok) {
            showToast(data.mensaje, 'success');
            document.getElementById('inputEmailCrear').readOnly = true;
            document.getElementById('pasoCodigo').classList.remove('d-none');
            btn.textContent = 'Reenviar código';
            btn.disabled = false;
        } else {
            showToast(data.mensaje || 'Error al enviar el código.', 'error');
            btn.textContent = 'Verificar';
            btn.disabled = false;
        }
    } catch (e) {
        showToast('Error de conexión.', 'error');
        btn.textContent = 'Verificar';
        btn.disabled = false;
    }
}

async function confirmarCodigoCrearUsuario() {
    const code = document.getElementById('inputCodigoCrear').value;
    if (!code) {
        showToast('Ingresa el código recibido.', 'error');
        return;
    }

    const formData = new URLSearchParams();
    formData.append('code', code);

    try {
        const res = await fetch('/User/VerificarCodigoCreacion', { method: 'POST', headers: csrfHeader(), body: formData });
        const data = await res.json();

        if (res.ok) {
            showToast(data.mensaje, 'success');
            document.getElementById('pasoCodigo').classList.add('d-none');
            document.getElementById('pasoDatos').classList.remove('d-none');
            document.getElementById('btnVerificarCorreo').classList.add('d-none');
            codigoCrearVerificado = true;
        } else {
            showToast(data.mensaje || 'Código incorrecto.', 'error');
        }
    } catch (e) {
        showToast('Error de conexión.', 'error');
    }
}

function cerrarModalAgregarUsuario() {
    document.getElementById('formCrearUsuario').reset();
    document.getElementById('pasoCodigo').classList.add('d-none');
    document.getElementById('pasoDatos').classList.add('d-none');
    document.getElementById('inputEmailCrear').readOnly = false;
    document.getElementById('btnVerificarCorreo').textContent = 'Verificar';
    codigoCrearVerificado = false;
    document.getElementById('modalAgregar').style.display = 'none';
}

function validarPasswordsCrear() {
    const pass = document.getElementById('crearPassword').value;
    const confirm = document.getElementById('crearConfirmPassword').value;
    if (pass !== confirm) {
        showToast('Las contraseñas no coinciden.', 'error');
        return false;
    }
    return true;
}

function validarPasswordsEditar() {
    const pass = document.getElementById('editPassword').value;
    const confirm = document.getElementById('editConfirmPassword').value;
    if (pass !== confirm) {
        showToast('Las contraseñas no coinciden.', 'error');
        return false;
    }
    return true;
}

document.getElementById('formCrearUsuario')?.addEventListener('submit', async function (e) {
    e.preventDefault();
    if (!validarPasswordsCrear()) return;

    const form = e.target;
    const errorBox = document.getElementById('errorCrearUsuario');
    const errorTexto = document.getElementById('errorCrearUsuarioTexto');
    errorBox.classList.add('d-none');

    const formData = new FormData(form);

    try {
        const response = await fetch('/User/Create', {
            method: 'POST',
            headers: csrfHeader(),
            body: formData
        });
        const data = await response.json();

        if (data.success) {
            showBannerAndReload(data.message, 'success');
        } else {
            errorTexto.textContent = data.message;
            errorBox.classList.remove('d-none');
        }
    } catch (err) {
        errorTexto.textContent = 'Ocurrió un error inesperado. Intenta de nuevo.';
        errorBox.classList.remove('d-none');
    }
});

document.getElementById('formEditarUsuario')?.addEventListener('submit', async function (e) {
    e.preventDefault();
    if (!validarPasswordsEditar()) return;

    const form = e.target;
    const errorBox = document.getElementById('errorEditarUsuario');
    const errorTexto = document.getElementById('errorEditarUsuarioTexto');
    errorBox.classList.add('d-none');

    const formData = new FormData(form);

    try {
        const response = await fetch('/User/Edit', {
            method: 'POST',
            headers: csrfHeader(),
            body: formData
        });
        const data = await response.json();

        if (data.success) {
            showBannerAndReload(data.message, 'success');
        } else {
            errorTexto.textContent = data.message;
            errorBox.classList.remove('d-none');
        }
    } catch (err) {
        errorTexto.textContent = 'Ocurrió un error inesperado. Intenta de nuevo.';
        errorBox.classList.remove('d-none');
    }
});