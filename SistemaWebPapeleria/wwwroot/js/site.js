// Funciones globales reutilizables en todo el sistema

function csrfHeader() {
    return { 'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').content };
}

function showToast(mensaje, tipo = 'success') {
    const container = document.getElementById('toastContainer');
    const colores = {
        success: { bg: 'rgba(var(--a11y-positive-rgb),0.15)', border: 'rgba(var(--a11y-positive-rgb),0.3)', color: 'var(--color-success)', icon: 'bi-check-circle-fill' },
        error: { bg: 'rgba(var(--a11y-danger-rgb),0.15)', border: 'rgba(var(--a11y-danger-rgb),0.3)', color: 'var(--color-danger)', icon: 'bi-x-circle-fill' },
        warning: { bg: 'rgba(var(--a11y-warning-rgb),0.15)', border: 'rgba(var(--a11y-warning-rgb),0.3)', color: 'var(--color-warning)', icon: 'bi-exclamation-triangle-fill' },
        info: { bg: 'rgba(var(--color-primary-rgb),0.15)', border: 'rgba(var(--color-primary-rgb),0.3)', color: 'var(--color-primary)', icon: 'bi-info-circle-fill' },
    };
    const c = colores[tipo] || colores.success;
    const toast = document.createElement('div');
    toast.style.cssText = `background: ${c.bg}; border: 1px solid ${c.border}; border-radius: 12px; padding: 14px 18px; display: flex; align-items: center; gap: 12px; min-width: 280px; max-width: 380px; backdrop-filter: blur(10px); animation: slideIn 0.3s ease;`;
    toast.innerHTML = `
        <i class="bi ${c.icon}" style="color: ${c.color}; font-size: calc(18px * var(--a11y-text-scale, 1)); flex-shrink: 0;"></i>
        <span style="color: #ffffff; font-size: calc(13px * var(--a11y-text-scale, 1)); flex: 1;">${mensaje}</span>
        <button onclick="this.parentElement.remove()" style="background: none; border: none; color: rgba(255,255,255,0.4); cursor: pointer; font-size: calc(16px * var(--a11y-text-scale, 1)); padding: 0; line-height: 1;">×</button>
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
    const color = esExito ? 'var(--color-success)' : 'var(--color-danger)';
    const bg = esExito ? 'rgba(var(--a11y-positive-rgb),0.15)' : 'rgba(var(--a11y-danger-rgb),0.15)';
    const icono = esExito ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill';

    const div = document.createElement('div');
    div.style.cssText = `background: ${bg}; border: 1px solid ${color}; color: ${color}; padding: 12px 18px; border-radius: 10px; margin-bottom: 20px; font-size: calc(13px * var(--a11y-text-scale, 1));`;
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