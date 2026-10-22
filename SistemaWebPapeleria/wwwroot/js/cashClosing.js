// cashClosing.js — lógica de la vista de caja general

function abrirEditarMonto(id, montoActual) {
    document.getElementById('editCajaId').value = id;
    document.getElementById('editMontoInicial').value = montoActual;
    document.getElementById('modalEditarMonto').style.display = 'flex';
}