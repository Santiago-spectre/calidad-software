function iniciarReloj() {
    function actualizarFechaHora() {
        const ahora = new Date();
        const opciones = {
            weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
            hour: '2-digit', minute: '2-digit', second: '2-digit',
            timeZone: 'America/Lima', hour12: true
        };
        const texto = ahora.toLocaleString('es-PE', opciones);
        const el = document.getElementById('fechaHoraActual');
        if (el) el.textContent = texto.charAt(0).toUpperCase() + texto.slice(1);
    }
    actualizarFechaHora();
    setInterval(actualizarFechaHora, 1000);
}

iniciarReloj();

// Dropdown del menú de usuario
function toggleMenuUsuario() {
    const dropdown = document.getElementById('menuUsuarioDropdown');
    const trigger = document.getElementById('btnMenuUsuario');
    const estaAbierto = dropdown.classList.contains('show');
    document.getElementById('bellDropdown')?.classList.remove('show');
    document.getElementById('btnNotificaciones')?.setAttribute('aria-expanded', 'false');
    if (estaAbierto) {
        dropdown.classList.remove('show');
        trigger?.setAttribute('aria-expanded', 'false');
    } else {
        dropdown.classList.add('show');
        trigger?.setAttribute('aria-expanded', 'true');
    }
}

document.addEventListener('click', (event) => {
    if (!event.target.closest('.user-menu-wrapper')) {
        document.getElementById('menuUsuarioDropdown')?.classList.remove('show');
        document.getElementById('btnMenuUsuario')?.setAttribute('aria-expanded', 'false');
    }
});

