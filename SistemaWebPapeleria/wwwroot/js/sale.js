// sale.js — lógica del punto de venta (POS)

(async function () {
    const res = await fetch('/CashClosing/VerificarCaja');
    const data = await res.json();
    if (!data.abierta) {
        document.getElementById('modalCajaVentas').style.display = 'flex';
    }
})();

let carrito = [];
const porPagina = 35;
let paginaActual = 1;
let productosFiltrados = productos.filter(p => p.isActive);

// ── Render inicial del grid ──
function renderGrid(lista) {
    const grid = document.getElementById('posGrid');
    if (lista.length === 0) {
        grid.innerHTML = '<div style="color:rgba(255,255,255,0.3);font-size:13px;grid-column:1/-1;text-align:center;padding:40px 0;">No se encontraron productos</div>';
        renderPaginacion(0);
        return;
    }
    const inicio = (paginaActual - 1) * porPagina;
    const fin = inicio + porPagina;
    const pagina = lista.slice(inicio, fin);

    grid.innerHTML = pagina.map(p => `
        <div class="product-card" onclick="agregarAlCarrito(${p.productId})">
            <div class="pc-icon"><i class="bi bi-box-seam"></i></div>
            <div class="pc-name">${p.name}</div>
            <div class="pc-price">S/ ${p.salePrice.toFixed(2)}</div>
            <div class="pc-stock">${p.isService ? 'Servicio' : 'Stock: ' + p.stock}</div>
        </div>
    `).join('');

    renderPaginacion(lista.length);
}

function renderPaginacion(total) {
    const totalPaginas = Math.ceil(total / porPagina);
    const cont = document.getElementById('paginacionPos');
    cont.innerHTML = '';
    if (totalPaginas <= 1) return;

    const btnStyle = 'background: rgba(255,255,255,0.07); border: 1px solid rgba(255,255,255,0.1); border-radius: 8px; color: #ffffff; padding: 5px 11px; font-size: 13px; cursor: pointer;';
    const btnActiveStyle = 'background: rgba(99,102,241,0.4); border: 1px solid #6366f1; border-radius: 8px; color: #ffffff; padding: 5px 11px; font-size: 13px; cursor: pointer; font-weight: 700;';

    // Anterior
    const btnAnt = document.createElement('button');
    btnAnt.innerHTML = '‹';
    btnAnt.style.cssText = btnStyle;
    btnAnt.disabled = paginaActual === 1;
    btnAnt.onclick = () => { paginaActual--; renderGrid(productosFiltrados); };
    cont.appendChild(btnAnt);

    // numeros
    for (let i = 1; i <= totalPaginas; i++) {
        if (totalPaginas > 7 && i > 5 && i < totalPaginas) {
            if (i === 6) {
                const pts = document.createElement('span');
                pts.textContent = '...';
                pts.style.cssText = 'color: rgba(255,255,255,0.4); font-size: 13px; padding: 0 4px;';
                cont.appendChild(pts);
            }
            continue;
        }
        const btn = document.createElement('button');
        btn.textContent = i;
        btn.style.cssText = i === paginaActual ? btnActiveStyle : btnStyle;
        btn.onclick = () => { paginaActual = i; renderGrid(productosFiltrados); };
        cont.appendChild(btn);
    }

    // Siguiente
    const btnSig = document.createElement('button');
    btnSig.innerHTML = '›';
    btnSig.style.cssText = btnStyle;
    btnSig.disabled = paginaActual === totalPaginas;
    btnSig.onclick = () => { paginaActual++; renderGrid(productosFiltrados); };
    cont.appendChild(btnSig);
}

function filtrarProductos() {
    const texto = document.getElementById('buscarProducto').value.toLowerCase();
    productosFiltrados = productos.filter(p => p.isActive && p.name.toLowerCase().includes(texto));
    paginaActual = 1;
    renderGrid(productosFiltrados);
}

// ── Carrito ──
function agregarAlCarrito(id) {
    const producto = productos.find(p => p.productId === id);
    if (!producto) return;
    const existente = carrito.find(c => c.productId === id);
    if (existente) {
        existente.cantidad++;
    } else {
        carrito.push({ ...producto, cantidad: 1, precioInvalido: false });
    }
    renderCarrito();
}

function quitarDelCarrito(id) {
    carrito = carrito.filter(c => c.productId !== id);
    renderCarrito();
}

function cambiarCantidad(id, valor) {
    const item = carrito.find(c => c.productId === id);
    if (item) {
        item.cantidad = Math.max(1, parseInt(valor) || 1);
    }
    renderCarrito();
}

function cambiarPrecio(id, valor) {
    const item = carrito.find(c => c.productId === id);
    if (!item) return;

    const nuevoPrecio = parseFloat(valor);
    const input = document.getElementById('precioInput_' + id);
    const esValido = valor !== '' && !isNaN(nuevoPrecio) && nuevoPrecio > 0;

    item.precioInvalido = !esValido;

    if (esValido) {
        item.salePrice = nuevoPrecio;
        document.getElementById('subtotal_' + id).textContent = 'S/ ' + (item.salePrice * item.cantidad).toFixed(2);
    }

    input.classList.toggle('input-invalido', !esValido);
    actualizarTotales();
}

function renderCarrito() {
    const list = document.getElementById('cartList');
    const empty = document.getElementById('cartEmpty');

    if (carrito.length === 0) {
        list.innerHTML = '<div class="cart-empty" id="cartEmpty"><i class="bi bi-bag-x"></i>Selecciona productos del catálogo</div>';
        actualizarTotales();
        return;
    }

    list.innerHTML = carrito.map(item => `
        <div class="cart-item">
            <div class="cart-item-name">${item.name}</div>
            <input class="cart-item-qty" type="number" value="${item.cantidad}" min="1"
                   onchange="cambiarCantidad(${item.productId}, this.value)" />
            <input class="cart-item-price-input" id="precioInput_${item.productId}" type="number" value="${item.salePrice.toFixed(2)}" min="0.01" step="0.01"
                   oninput="cambiarPrecio(${item.productId}, this.value)" />
            <div class="cart-item-price" id="subtotal_${item.productId}">S/ ${(item.salePrice * item.cantidad).toFixed(2)}</div>
            <button class="cart-item-del" onclick="quitarDelCarrito(${item.productId})">
                <i class="bi bi-trash-fill"></i>
            </button>
        </div>
    `).join('');

    actualizarTotales();
}

function actualizarTotales() {
    const bruto = carrito.reduce((sum, i) => sum + (i.salePrice * i.cantidad), 0);
    const descuento = parseFloat(document.getElementById('descuento').value) || 0;
    const total = Math.max(0, bruto - descuento);

    // IGV incluido en el precio: opGravada = total / 1.18, igv = total - opGravada
    const opGravada = total / 1.18;
    const igv = total - opGravada;

    document.getElementById('opGravada').textContent = 'S/ ' + opGravada.toFixed(2);
    document.getElementById('igvMonto').textContent = 'S/ ' + igv.toFixed(2);
    document.getElementById('totalVenta').textContent = 'S/ ' + total.toFixed(2);

    const hayPreciosInvalidos = carrito.some(i => i.precioInvalido);
    document.getElementById('btnPagar').disabled = carrito.length === 0 || hayPreciosInvalidos;
}

async function registrarVenta() {
    if (carrito.length === 0) return;

    if (carrito.some(item => item.precioInvalido)) {
        showToast('Corrige los precios inválidos antes de continuar.', 'error');
        return;
    }

    if (carrito.some(item => item.salePrice <= 0)) {
        showToast('Hay productos con precio inválido en el carrito.', 'error');
        return;
    }

    for (const item of carrito) {
        const inputQty = document.querySelector(`input.cart-item-qty[onchange*="${item.productId}"]`);
        if (inputQty) {
            const valorActual = parseInt(inputQty.value);
            if (!valorActual || valorActual <= 0) {
                showToast(`La cantidad de "${item.name}" debe ser mayor a cero.`, 'error');
                return;
            }
            item.cantidad = valorActual;
        }
    }

    const datos = {
        paymentMethod: document.getElementById('metodoPago').value,
        discount: parseFloat(document.getElementById('descuento').value) || 0,
        receiptIssued: document.getElementById('emitirComprobante').checked,
        items: carrito.map(c => ({ productId: c.productId, quantity: c.cantidad, unitPrice: c.salePrice }))
    };

    const response = await fetch('/Sale/Create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...csrfHeader() },
        body: JSON.stringify(datos)
    });

    if (response.ok) {
        const data = await response.json();
        carrito = [];
        renderCarrito();
        document.getElementById('descuento').value = '0';
        actualizarTotales();
        sessionStorage.setItem('toastPendiente', JSON.stringify({ mensaje: 'Venta registrada correctamente.', tipo: 'success' }));
        if (datos.receiptIssued) {
            window.open('/Report/GenerarComprobante/' + data.saleId, '_blank');
        }
        document.getElementById('emitirComprobante').checked = false;
        location.reload();
    } else {
        try {
            const error = await response.json();
            showToast(error.mensaje || 'Error al registrar la venta. Intenta de nuevo.', 'error');
        } catch {
            showToast('Error al registrar la venta. Intenta de nuevo.', 'error');
        }
    }
}

// Cargar grid al iniciar
renderGrid(productos.filter(p => p.isActive));

async function aplicarFiltroVentas() {
    const userId = document.getElementById('filtroUsuarioVentas')?.value ?? '';
    const url = '/Sale/GetSaleStats' + (userId ? '?userId=' + userId : '');

    const res = await fetch(url);
    const data = await res.json();

    document.getElementById('statTodaySales').textContent = data.todaySales;
    document.getElementById('statTotalSales').textContent = data.totalSales;
    document.getElementById('statAverageSales').textContent = data.averageSales;
}