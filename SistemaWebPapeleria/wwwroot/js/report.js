// report.js — lógica de la vista de reportes (filtros, detalle de venta, devoluciones)

const filasPorPagina = 20;
let paginaActual = 1;

function paginar() {
    const todasLasFilas = Array.from(document.querySelectorAll('#tablaVentas tr[data-vendedor]'));
    const filasFiltradas = todasLasFilas.filter(f => !f.classList.contains('filtrada-oculta'));

    const totalPaginas = Math.ceil(filasFiltradas.length / filasPorPagina);
    if (paginaActual > totalPaginas) paginaActual = 1;

    todasLasFilas.forEach(f => f.style.display = 'none');

    filasFiltradas.forEach((fila, i) => {
        if (i >= (paginaActual - 1) * filasPorPagina && i < paginaActual * filasPorPagina)
            fila.style.display = '';
    });

    document.getElementById('infoPagina').textContent = `Página ${paginaActual} de ${totalPaginas || 1}`;
    document.getElementById('btnAnterior').disabled = paginaActual === 1;
    document.getElementById('btnSiguiente').disabled = paginaActual >= totalPaginas;
}

function cambiarPagina(direccion) {
    paginaActual += direccion;
    paginar();
}

function filtrar() {
    const vendedor = document.getElementById('filtroUsuario')?.value.toLowerCase() ?? '';
    const fecha = document.getElementById('filtroFecha')?.value ?? '';
    const buscar = document.getElementById('filtroBuscar')?.value.toLowerCase() ?? '';
    const filas = document.querySelectorAll('#tablaVentas tr[data-vendedor]');

    filas.forEach(fila => {
        const v = fila.dataset.vendedor?.toLowerCase() ?? '';
        const f = fila.dataset.fecha ?? '';
        const texto = fila.textContent.toLowerCase();

        const matchVendedor = vendedor === '' || v.includes(vendedor);
        const matchFecha = fecha === '' || f === fecha;
        const matchBuscar = buscar === '' || texto.includes(buscar);

        if (matchVendedor && matchFecha && matchBuscar) {
            fila.classList.remove('filtrada-oculta');
        } else {
            fila.classList.add('filtrada-oculta');
        }
    });

    paginaActual = 1;
    paginar();
}

function generarComprobante(saleId) {
    window.open('/Report/GenerarComprobante/' + saleId, '_blank');
}

function descargarReporte(mes, anio) {
    window.open('/Report/GenerarReporteMensual?mes=' + mes + '&anio=' + anio, '_blank');
}

async function verDetalle(saleId) {
    const res = await fetch('/Report/GetSaleDetail/' + saleId);
    const data = await res.json();

    document.getElementById('modalSaleId').textContent = '#' + data.saleId;
    document.getElementById('modalFecha').textContent = '📅 ' + data.fecha;
    document.getElementById('modalPago').textContent = '💳 ' + data.paymentMethod;

    document.getElementById('modalItems').innerHTML = data.items.map(i => `
        <tr style="border-bottom:1px solid rgba(255,255,255,0.04);">
            <td style="color:#ffffff;font-size:13px;padding:12px 15px;">${i.producto}</td>
            <td style="color:rgba(255,255,255,0.6);font-size:13px;padding:12px 15px;text-align:center;">
                ${i.cantidad}
                ${i.devuelto > 0 ? `<br><span style="color:#f87171;font-size:11px;">(${i.devuelto} devuelto${i.devuelto > 1 ? 's' : ''})</span>` : ''}
            </td>
            <td style="color:rgba(255,255,255,0.6);font-size:13px;padding:12px 15px;text-align:right;">S/ ${i.precioUnitario.toFixed(2)}</td>
            <td style="color:#34d399;font-size:13px;font-weight:700;padding:12px 15px;text-align:right;">S/ ${i.subtotal.toFixed(2)}</td>
        </tr>
    `).join('');

    document.getElementById('modalDescuento').textContent = data.discount > 0 ? 'Descuento: S/ ' + data.discount.toFixed(2) : '';
    document.getElementById('modalTotal').textContent = 'TOTAL: S/ ' + data.total.toFixed(2);

    document.getElementById('modalDetalle').style.display = 'flex';
}

function cerrarModal() {
    document.getElementById('modalDetalle').style.display = 'none';
}

let devSaleIdActual = null;
let devItemsActual = [];

async function abrirDevolucion(saleId) {
    const res = await fetch('/Report/GetSaleDetail/' + saleId);
    const data = await res.json();

    devSaleIdActual = saleId;
    devItemsActual = data.items;

    document.getElementById('devSaleId').textContent = '#' + data.saleId;
    document.getElementById('devMotivo').value = '';

    document.getElementById('devItems').innerHTML = data.items.map((i, index) => `
        <tr style="border-bottom:1px solid rgba(255,255,255,0.04);">
            <td style="color:#ffffff;font-size:13px;padding:12px 15px;">${i.producto}</td>
            <td style="color:rgba(255,255,255,0.6);font-size:13px;padding:12px 15px;text-align:center;">
                ${i.disponibleDevolucion} disponible${i.devuelto > 0 ? `<br><span style="color:rgba(255,255,255,0.3);font-size:11px;">(${i.devuelto} ya devuelto)</span>` : ''}
            </td>
            <td style="padding:12px 15px;text-align:center;">
                <input type="number" id="devCantidad_${index}" min="0" max="${i.disponibleDevolucion}" value="0"
                       style="width:60px;padding:5px;background:rgba(255,255,255,0.07);border:1px solid rgba(255,255,255,0.1);border-radius:8px;color:#fff;font-size:13px;text-align:center;outline:none;" />
            </td>
        </tr>
    `).join('');

    document.getElementById('modalDevolucion').style.display = 'flex';
}

function cerrarDevolucion() {
    document.getElementById('modalDevolucion').style.display = 'none';
}

async function confirmarDevolucion() {
    const motivo = document.getElementById('devMotivo').value;
    const items = devItemsActual
        .map((i, index) => ({
            productId: i.productId,
            quantity: Number.parseInt(document.getElementById('devCantidad_' + index).value, 10) || 0
        }))
        .filter(i => i.quantity > 0);

    if (items.length === 0) {
        showToast('Debes ingresar al menos una cantidad a devolver.', 'error');
        return;
    }

    const res = await fetch('/Return/Create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...csrfHeader() },
        body: JSON.stringify({ saleId: devSaleIdActual, reason: motivo, items })
    });

    if (res.ok) {
        cerrarDevolucion();
        sessionStorage.setItem('toastPendiente', JSON.stringify({ mensaje: 'Devolución registrada correctamente.', tipo: 'success' }));
        location.reload();
    } else {
        try {
            const error = await res.json();
            showToast(error.mensaje || 'Error al registrar la devolución.', 'error');
        } catch {
            showToast('Error al registrar la devolución.', 'error');
        }
    }
}

paginar();