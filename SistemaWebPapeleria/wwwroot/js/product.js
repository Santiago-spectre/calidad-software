// product.js — lógica de la vista de productos y categorías

const searchInput = document.getElementById('searchInput');
const filterTipo = document.getElementById('filterTipo');
const filterEstado = document.getElementById('filterEstado');
const rowsPerPage = 10;
let currentPage = 1;

function getFilasVisibles() {
    const texto = searchInput.value.toLowerCase();
    const tipo = filterTipo.value.toLowerCase();
    const estado = filterEstado.value.toLowerCase();
    const filas = Array.from(document.querySelectorAll('tbody tr'));
    return filas.filter(fila => {
        if (fila.cells.length < 6) return false;
        const nombre = fila.cells[0].textContent.toLowerCase();
        const tipoCelda = fila.cells[2].textContent.trim().toLowerCase();
        const estadoCelda = fila.cells[5].textContent.trim().toLowerCase();
        return nombre.includes(texto) && (tipo === '' || tipoCelda.includes(tipo)) && (estado === '' || estadoCelda.includes(estado));
    });
}

function renderPagina() {
    const filas = Array.from(document.querySelectorAll('tbody tr'));
    const visibles = getFilasVisibles();
    filas.forEach(f => f.style.display = 'none');
    const inicio = (currentPage - 1) * rowsPerPage;
    const fin = inicio + rowsPerPage;
    visibles.slice(inicio, fin).forEach(f => f.style.display = '');
    renderPaginacion(visibles.length);
}

function renderPaginacion(total) {
    const totalPaginas = Math.ceil(total / rowsPerPage);
    let paginacion = document.getElementById('paginacion');
    paginacion.innerHTML = '';
    if (totalPaginas <= 1) return;
    const btnStyle = 'background: rgba(255,255,255,0.07); border: 1px solid rgba(255,255,255,0.1); border-radius: 8px; color: #ffffff; padding: 5px 11px; font-size: 13px; cursor: pointer;';
    const btnActiveStyle = 'background: rgba(99,102,241,0.4); border: 1px solid #6366f1; border-radius: 8px; color: #ffffff; padding: 5px 11px; font-size: 13px; cursor: pointer; font-weight: 700;';

    const btnAnterior = document.createElement('button');
    btnAnterior.textContent = '‹';
    btnAnterior.style.cssText = btnStyle;
    btnAnterior.disabled = currentPage === 1;
    btnAnterior.onclick = () => { currentPage--; renderPagina(); };
    paginacion.appendChild(btnAnterior);

    for (let i = 1; i <= totalPaginas; i++) {
        const btn = document.createElement('button');
        btn.textContent = i;
        btn.style.cssText = i === currentPage ? btnActiveStyle : btnStyle;
        btn.onclick = () => { currentPage = i; renderPagina(); };
        paginacion.appendChild(btn);
    }

    const btnSiguiente = document.createElement('button');
    btnSiguiente.textContent = '›';
    btnSiguiente.style.cssText = btnStyle;
    btnSiguiente.disabled = currentPage === totalPaginas;
    btnSiguiente.onclick = () => { currentPage++; renderPagina(); };
    paginacion.appendChild(btnSiguiente);
}

function filtrar() { currentPage = 1; renderPagina(); }

searchInput.addEventListener('input', filtrar);
filterTipo.addEventListener('change', filtrar);
filterEstado.addEventListener('change', filtrar);
renderPagina();

// ── Paginación categorías ──
const catsPorPagina = 9;
let catPaginaActual = 1;
const todasCats = document.querySelectorAll('.cat-item');

function renderCats() {
    const total = todasCats.length;
    const inicio = (catPaginaActual - 1) * catsPorPagina;
    const fin = inicio + catsPorPagina;
    todasCats.forEach((c, i) => c.style.display = (i >= inicio && i < fin) ? 'flex' : 'none');
    renderPaginacionCats(total);
}

function renderPaginacionCats(total) {
    const totalPaginas = Math.ceil(total / catsPorPagina);
    const cont = document.getElementById('paginacionCats');
    cont.innerHTML = '';
    if (totalPaginas <= 1) return;

    const btnStyle = 'background: rgba(255,255,255,0.07); border: 1px solid rgba(255,255,255,0.1); border-radius: 8px; color: #ffffff; padding: 4px 10px; font-size: 12px; cursor: pointer;';
    const btnActiveStyle = 'background: rgba(99,102,241,0.4); border: 1px solid #6366f1; border-radius: 8px; color: #ffffff; padding: 4px 10px; font-size: 12px; cursor: pointer; font-weight: 700;';

    const btnAnt = document.createElement('button');
    btnAnt.textContent = '‹';
    btnAnt.style.cssText = btnStyle;
    btnAnt.disabled = catPaginaActual === 1;
    btnAnt.onclick = () => { catPaginaActual--; renderCats(); };
    cont.appendChild(btnAnt);

    for (let i = 1; i <= totalPaginas; i++) {
        const btn = document.createElement('button');
        btn.textContent = i;
        btn.style.cssText = i === catPaginaActual ? btnActiveStyle : btnStyle;
        btn.onclick = () => { catPaginaActual = i; renderCats(); };
        cont.appendChild(btn);
    }

    const btnSig = document.createElement('button');
    btnSig.textContent = '›';
    btnSig.style.cssText = btnStyle;
    btnSig.disabled = catPaginaActual === totalPaginas;
    btnSig.onclick = () => { catPaginaActual++; renderCats(); };
    cont.appendChild(btnSig);
}

renderCats();

function abrirEditarCategoria(btn) {
    const d = btn.dataset;
    document.getElementById('editCategoriaNombre').value = d.nombre;
    document.getElementById('editCategoriaDescripcion').value = d.descripcion;
    document.getElementById('formEditarCategoria').action = '/Category/Edit/' + d.id;
    document.getElementById('modalEditarCategoria').style.display = 'flex';
}

function abrirEditarProducto(btn) {
    const d = btn.dataset;
    document.getElementById('editNombre').value = d.nombre;
    document.getElementById('editDescripcion').value = d.descripcion;
    document.getElementById('editPrecioVenta').value = d.precioVenta;
    document.getElementById('editPrecioCompra').value = d.precioCompra;
    document.getElementById('editStock').value = d.stock;
    document.getElementById('editStockMinimo').value = d.stockMinimo;
    document.getElementById('editCategoria').value = d.categoriaId;
    document.getElementById('editProveedor').value = d.proveedorId;
    const esServicio = d.esServicio === 'true';
    document.getElementById('editIsServiceHidden').value = esServicio ? 'true' : 'false';
    document.getElementById('editTipoLabel').textContent = esServicio ? 'Servicio' : 'Producto físico';
    document.getElementById('editStockField').style.display = esServicio ? 'none' : 'grid';
    document.getElementById('editProveedorField').style.display = esServicio ? 'none' : 'block';
    document.getElementById('formEditarProducto').action = '/Product/Edit/' + d.id;
    document.getElementById('modalEditarProducto').style.display = 'flex';
    document.getElementById('editPrecioCompraField').style.display = esServicio ? 'none' : 'block';
}

document.getElementById('formCrearProducto')?.addEventListener('submit', async function (e) {
    e.preventDefault();

    const form = e.target;
    const errorBox = document.getElementById('errorCrearProducto');
    const errorTexto = document.getElementById('errorCrearProductoTexto');
    errorBox.classList.add('d-none');

    const formData = new FormData(form);

    try {
        const response = await fetch('/Product/Create', {
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

document.getElementById('formCrearServicio')?.addEventListener('submit', async function (e) {
    e.preventDefault();

    const form = e.target;
    const errorBox = document.getElementById('errorCrearServicio');
    const errorTexto = document.getElementById('errorCrearServicioTexto');
    errorBox.classList.add('d-none');

    const formData = new FormData(form);

    try {
        const response = await fetch('/Product/CreateService', {
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


document.getElementById('formEditarProducto')?.addEventListener('submit', async function (e) {
    e.preventDefault();

    const form = e.target;
    const errorBox = document.getElementById('errorEditarProducto');
    const errorTexto = document.getElementById('errorEditarProductoTexto');
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
