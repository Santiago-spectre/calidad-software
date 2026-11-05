// notifications.js — lógica del panel de notificaciones (campana)

function toggleNotificaciones() {
    const dropdown = document.getElementById('bellDropdown');
    const trigger = document.getElementById('btnNotificaciones');
    const estaAbierto = dropdown.classList.contains('show');
    if (estaAbierto) {
        dropdown.classList.remove('show');
        trigger?.setAttribute('aria-expanded', 'false');
    } else {
        dropdown.classList.add('show');
        trigger?.setAttribute('aria-expanded', 'true');
        cargarNotificaciones();
    }
}

document.addEventListener('click', (event) => {
    if (!event.target.closest('.notification-wrapper')) {
        document.getElementById('bellDropdown')?.classList.remove('show');
        document.getElementById('btnNotificaciones')?.setAttribute('aria-expanded', 'false');
    }
});

function tiempoRelativo(fechaISO) {
    const fecha = new Date(fechaISO);
    const ahora = new Date();
    const segundos = Math.floor((ahora - fecha) / 1000);
    if (segundos < 60) return 'Hace un momento';
    if (segundos < 3600) return `Hace ${Math.floor(segundos / 60)} min`;
    if (segundos < 86400) return `Hace ${Math.floor(segundos / 3600)} h`;
    return fecha.toLocaleDateString('es-PE');
}

async function cargarNotificaciones() {
    try {
        const res = await fetch('/Notification/GetRecientes');
        const data = await res.json();

        const badge = document.getElementById('bellBadge');
        if (data.noLeidas > 0) {
            badge.classList.remove('d-none');
            badge.textContent = data.noLeidas > 9 ? '9+' : data.noLeidas;
        } else {
            badge.classList.add('d-none');
        }

        const lista = document.getElementById('notifList');
        if (data.notificaciones.length === 0) {
            lista.innerHTML = '<p style="color: rgba(255,255,255,0.3); font-size: 12px; text-align: center; padding: 25px;">No tienes notificaciones</p>';
            return;
        }
        lista.innerHTML = data.notificaciones.map(n => `
            <div class="notif-item ${n.isRead ? '' : 'unread'}" style="display: flex; justify-content: space-between; align-items: flex-start; gap: 10px;">
                <div onclick="marcarLeida(${n.notificationId}, this)" style="flex: 1; cursor: pointer;">
                    <p class="title">${n.title}</p>
                    <p class="msg">${n.message}</p>
                    <p class="time">${tiempoRelativo(n.createdAt)}</p>
                </div>
                <button onclick="eliminarNotificacion(${n.notificationId}, this)" style="background: none; border: none; color: rgba(255,255,255,0.3); cursor: pointer; font-size: 14px; padding: 4px; flex-shrink: 0;">
                    <i class="bi bi-trash-fill"></i>
                </button>
            </div>
        `).join('');
    } catch (e) {
        console.error('Error al cargar notificaciones', e);
    }
}

async function marcarLeida(id, elemento) {
    elemento.classList.remove('unread');
    await fetch('/Notification/MarcarLeida?id=' + id, { method: 'POST', headers: csrfHeader() });
    cargarNotificaciones();
}

async function eliminarNotificacion(id, boton) {
    await fetch('/Notification/Eliminar?id=' + id, { method: 'POST', headers: csrfHeader() });
    boton.closest('.notif-item').remove();
    cargarNotificaciones();
}

async function eliminarTodasNotificaciones() {
    await fetch('/Notification/EliminarTodas', { method: 'POST', headers: csrfHeader() });
    cargarNotificaciones();
}

async function marcarTodasLeidas() {
    await fetch('/Notification/MarcarTodasLeidas', { method: 'POST', headers: csrfHeader() });
    cargarNotificaciones();
}

// Carga inicial y refresco automático cada 30 segundos
cargarNotificaciones();
setInterval(cargarNotificaciones, 30000);