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
    const estaAbierto = dropdown.classList.contains('show');
    document.getElementById('bellDropdown')?.classList.remove('show');
    if (estaAbierto) {
        dropdown.classList.remove('show');
    } else {
        dropdown.classList.add('show');
    }
}

document.addEventListener('click', () => {
    document.getElementById('menuUsuarioDropdown')?.classList.remove('show');
});

