(() => {
    const trigger = document.getElementById('btnAccesibilidad');
    const panel = document.getElementById('panelAccesibilidad');
    const closeButton = document.getElementById('btnCerrarAccesibilidad');
    const textSizeControl = document.getElementById('accessibilityTextSize');

    const textSizeClasses = {
        normal: '',
        large: 'a11y-text-large',
        larger: 'a11y-text-larger'
    };

    function applyTextSize(size) {
        const classNames = Object.values(textSizeClasses).filter(Boolean);
        document.documentElement.classList.remove(...classNames);

        if (textSizeClasses[size]) {
            document.documentElement.classList.add(textSizeClasses[size]);
        }

        if (textSizeControl) {
            textSizeControl.value = size;
        }
    }

    let savedTextSize = 'normal';

    try {
        savedTextSize = localStorage.getItem('a11y-text-size') || 'normal';
    } catch {
        // Si el navegador bloquea el almacenamiento, se usa el tamaño normal.
    }

    if (!Object.prototype.hasOwnProperty.call(textSizeClasses, savedTextSize)) {
        savedTextSize = 'normal';
    }

    applyTextSize(savedTextSize);

    textSizeControl?.addEventListener('change', () => {
        const selectedSize = textSizeControl.value;
        applyTextSize(selectedSize);

        try {
            localStorage.setItem('a11y-text-size', selectedSize);
        } catch {
            // El tamaño se aplica mientras esta página permanezca abierta.
        }
    });

    if (!trigger || !panel || !closeButton) return;

    function openPanel() {
        panel.hidden = false;
        trigger.setAttribute('aria-expanded', 'true');
        closeButton.focus();
    }

    function closePanel() {
        panel.hidden = true;
        trigger.setAttribute('aria-expanded', 'false');
        trigger.focus();
    }

    trigger.addEventListener('click', openPanel);
    closeButton.addEventListener('click', closePanel);

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && !panel.hidden) {
            closePanel();
        }
    });
})();