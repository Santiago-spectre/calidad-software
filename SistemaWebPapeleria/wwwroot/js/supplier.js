// supplier.js — lógica de la vista de proveedores

function abrirEditar(btn) {
    const d = btn.dataset;
    document.getElementById('editNombre').value = d.nombre;
    document.getElementById('editTelefono').value = d.telefono;
    document.getElementById('editDireccion').value = d.direccion;
    document.getElementById('editDescripcion').value = d.descripcion;
    document.getElementById('formEditar').action = '/Supplier/Edit/' + d.id;
    document.getElementById('modalEditar').style.display = 'flex';
}

document.getElementById('formCrearProveedor')?.addEventListener('submit', async function (e) {
    e.preventDefault();

    const form = e.target;
    const errorBox = document.getElementById('errorCrearProveedor');
    const errorTexto = document.getElementById('errorCrearProveedorTexto');
    errorBox.classList.add('d-none');

    const formData = new FormData(form);

    try {
        const response = await fetch('/Supplier/Create', {
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

document.getElementById('formEditar')?.addEventListener('submit', async function (e) {
    e.preventDefault();

    const form = e.target;
    const errorBox = document.getElementById('errorEditarProveedor');
    const errorTexto = document.getElementById('errorEditarProveedorTexto');
    errorBox.classList.add('d-none');

    const formData = new FormData(form);

    try {
        const response = await fetch(form.action, {
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