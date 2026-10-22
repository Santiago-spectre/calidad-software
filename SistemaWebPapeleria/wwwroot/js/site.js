// Funciones globales reutilizables en todo el sistema

function csrfHeader() {
    return { 'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').content };
}

function showToast(mensaje, tipo = 'success') {
    const container = document.getElementById('toastContainer');
    const colores = {
        success: { bg: 'rgba(52,211,153,0.15)', border: 'rgba(52,211,153,0.3)', color: '#34d399', icon: 'bi-check-circle-fill' },
        error: { bg: 'rgba(248,113,113,0.15)', border: 'rgba(248,113,113,0.3)', color: '#f87171', icon: 'bi-x-circle-fill' },
        warning: { bg: 'rgba(251,191,36,0.15)', border: 'rgba(251,191,36,0.3)', color: '#fbbf24', icon: 'bi-exclamation-triangle-fill' },
        info: { bg: 'rgba(99,102,241,0.15)', border: 'rgba(99,102,241,0.3)', color: '#818cf8', icon: 'bi-info-circle-fill' },
    };
    const c = colores[tipo] || colores.success;
    const toast = document.createElement('div');
    toast.style.cssText = `background: ${c.bg}; border: 1px solid ${c.border}; border-radius: 12px; padding: 14px 18px; display: flex; align-items: center; gap: 12px; min-width: 280px; max-width: 380px; backdrop-filter: blur(10px); animation: slideIn 0.3s ease;`;
    toast.innerHTML = `
        <i class="bi ${c.icon}" style="color: ${c.color}; font-size: 18px; flex-shrink: 0;"></i>
        <span style="color: #ffffff; font-size: 13px; flex: 1;">${mensaje}</span>
        <button onclick="this.parentElement.remove()" style="background: none; border: none; color: rgba(255,255,255,0.4); cursor: pointer; font-size: 16px; padding: 0; line-height: 1;">×</button>
    `;
    container.appendChild(toast);
    setTimeout(() => { if (toast.parentElement) toast.remove(); }, 4000);
}

document.addEventListener('DOMContentLoaded', () => {
    const pendiente = sessionStorage.getItem('toastPendiente');
    if (pendiente) {
        sessionStorage.removeItem('toastPendiente');
        const { mensaje, tipo } = JSON.parse(pendiente);
        setTimeout(() => showToast(mensaje, tipo), 300);
    }
});

function togglePassword(inputId, icon) {
    const input = document.getElementById(inputId);
    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('bi-eye-fill');
        icon.classList.add('bi-eye-slash-fill');
    } else {
        input.type = 'password';
        icon.classList.remove('bi-eye-slash-fill');
        icon.classList.add('bi-eye-fill');
    }
}

function showBannerAndReload(message, tipo = 'success') {
    sessionStorage.setItem('flashMessage', JSON.stringify({ message, tipo }));
    setTimeout(() => location.reload(), 300);
}

document.addEventListener('DOMContentLoaded', () => {
    const flash = sessionStorage.getItem('flashMessage');
    if (!flash) return;
    sessionStorage.removeItem('flashMessage');

    const { message, tipo } = JSON.parse(flash);
    const container = document.getElementById('flashBannerContainer');
    if (!container) return;

    const esExito = tipo === 'success';
    const color = esExito ? '#34d399' : '#f87171';
    const bg = esExito ? 'rgba(52,211,153,0.15)' : 'rgba(248,113,113,0.15)';
    const icono = esExito ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill';

    const div = document.createElement('div');
    div.style.cssText = `background: ${bg}; border: 1px solid ${color}; color: ${color}; padding: 12px 18px; border-radius: 10px; margin-bottom: 20px; font-size: 13px;`;
    div.innerHTML = `<i class="bi ${icono}"></i> ${message}`;
    container.prepend(div);

    setTimeout(() => {
        div.style.transition = 'opacity 0.4s ease';
        div.style.opacity = '0';
        setTimeout(() => div.remove(), 400);
    }, 6000);
});

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.alert-auto-dismiss').forEach(alerta => {
        setTimeout(() => {
            alerta.style.transition = 'opacity 0.4s ease';
            alerta.style.opacity = '0';
            setTimeout(() => alerta.remove(), 400);
        }, 6000);
    });
});

function confirmarEliminar(formId, mensaje) {
    const modal = document.getElementById('modalConfirmarEliminarGlobal');
    document.getElementById('mensajeConfirmarEliminar').textContent = mensaje || 'Esta acción no se puede deshacer.';
    modal.style.display = 'flex';
    document.getElementById('btnConfirmarEliminarGlobal').onclick = () => {
        document.getElementById(formId).submit();
    };
}