// profile.js — lógica del modal "Mi Perfil"

let correoOriginalPerfil = '';

async function abrirModalPerfil() {
    document.getElementById('menuUsuarioDropdown').classList.remove('show');
    try {
        const res = await fetch('/Profile/GetMisDatos');
        const data = await res.json();
        document.getElementById('perfilNombre').value = data.name;
        document.getElementById('perfilApellido').value = data.lastName;
        document.getElementById('perfilEmail').value = data.email;
        document.getElementById('perfilPassword').value = '';
        document.getElementById('perfilConfirmPassword').value = '';
        document.getElementById('modalMiPerfil').style.display = 'flex';

        correoOriginalPerfil = data.email.toLowerCase();
        document.getElementById('btnVerificarPerfilEmail').classList.add('d-none');
        document.getElementById('pasoCodigoPerfil').classList.add('d-none');
        document.getElementById('btnGuardarPerfil').disabled = false;
    } catch (e) {
        showToast('No se pudieron cargar tus datos.', 'error');
    }
}

function onPerfilEmailChange() {
    const nuevoEmail = document.getElementById('perfilEmail').value.toLowerCase();
    const btn = document.getElementById('btnVerificarPerfilEmail');

    if (nuevoEmail !== correoOriginalPerfil && nuevoEmail.includes('@')) {
        btn.classList.remove('d-none');
        document.getElementById('btnGuardarPerfil').disabled = true;
        document.getElementById('pasoCodigoPerfil').classList.add('d-none');
    } else {
        btn.classList.add('d-none');
        document.getElementById('pasoCodigoPerfil').classList.add('d-none');
        document.getElementById('btnGuardarPerfil').disabled = false;
    }
}

async function enviarCodigoPerfil() {
    const email = document.getElementById('perfilEmail').value;
    const btn = document.getElementById('btnVerificarPerfilEmail');
    btn.disabled = true;
    btn.textContent = 'Enviando...';

    const formData = new URLSearchParams();
    formData.append('email', email);

    try {
        const res = await fetch('/Profile/EnviarCodigoVerificacion', { method: 'POST', headers: csrfHeader(), body: formData });
        const data = await res.json();

        if (res.ok) {
            showToast(data.mensaje, 'success');
            document.getElementById('pasoCodigoPerfil').classList.remove('d-none');
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

async function confirmarCodigoPerfil() {
    const code = document.getElementById('inputCodigoPerfil').value;
    if (!code) {
        showToast('Ingresa el código recibido.', 'error');
        return;
    }

    const formData = new URLSearchParams();
    formData.append('code', code);

    try {
        const res = await fetch('/Profile/VerificarCodigo', { method: 'POST', headers: csrfHeader(), body: formData });
        const data = await res.json();

        if (res.ok) {
            showToast(data.mensaje, 'success');
            document.getElementById('btnGuardarPerfil').disabled = false;
            document.getElementById('pasoCodigoPerfil').classList.add('d-none');
            document.getElementById('btnVerificarPerfilEmail').classList.add('d-none');
        } else {
            showToast(data.mensaje || 'Código incorrecto.', 'error');
        }
    } catch (e) {
        showToast('Error de conexión.', 'error');
    }
}

async function guardarPerfil(event) {
    event.preventDefault();

    const pass = document.getElementById('perfilPassword').value;
    const confirm = document.getElementById('perfilConfirmPassword').value;
    if (pass !== confirm) {
        showToast('Las contraseñas no coinciden.', 'error');
        return;
    }

    const formData = new URLSearchParams();
    formData.append('Name', document.getElementById('perfilNombre').value);
    formData.append('LastName', document.getElementById('perfilApellido').value);
    formData.append('Email', document.getElementById('perfilEmail').value);
    formData.append('Password', document.getElementById('perfilPassword').value);

    try {
        const res = await fetch('/Profile/Edit', { method: 'POST', headers: csrfHeader(), body: formData });

        if (res.ok) {
            document.getElementById('modalMiPerfil').style.display = 'none';
            showToast('Perfil actualizado correctamente.', 'success');
            setTimeout(() => location.reload(), 1000);
        } else {
            const error = await res.json();
            showToast(error.mensaje || 'Error al actualizar el perfil.', 'error');
        }
    } catch (e) {
        showToast('Error de conexión al guardar el perfil.', 'error');
    }
}