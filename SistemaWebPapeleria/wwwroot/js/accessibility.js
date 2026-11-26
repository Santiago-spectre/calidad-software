(() => {
    const trigger = document.getElementById('btnAccesibilidad');
    const panel = document.getElementById('panelAccesibilidad');
    const closeButton = document.getElementById('btnCerrarAccesibilidad');
    const textSizeControl = document.getElementById('accessibilityTextSize');
    const languageControl = document.getElementById('accessibilityLanguage');
    const speechRateControl = document.getElementById('accessibilitySpeechRate');
    const readPageButton = document.getElementById('accessibilityReadPage');
    const stopReadingButton = document.getElementById('accessibilityStopReading');
    const speechStatus = document.getElementById('accessibilitySpeechStatus');
    const colorVisionControl = document.getElementById('accessibilityColorVision');
    const highContrastControl = document.getElementById('accessibilityHighContrast');
    const focusModeControl = document.getElementById('accessibilityFocusMode');
    const dyslexiaModeControl = document.getElementById('accessibilityDyslexiaMode');

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

        window.dispatchEvent(new CustomEvent('a11y:textsizechange', {
            detail: { size, scale: size === 'larger' ? 1.25 : size === 'large' ? 1.125 : 1 }
        }));
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

    const spanishToEnglish = {
        'accesibilidad': 'Accessibility', 'opciones de accesibilidad': 'Accessibility options',
        'cerrar opciones de accesibilidad': 'Close accessibility options',
        'personaliza la presentación y el uso del sistema.': 'Customize how the system looks and works.',
        'idioma': 'Language', 'idioma de la interfaz': 'Interface language',
        'cambia el idioma de los textos de la interfaz.': 'Changes the language of interface text.',
        'lectura': 'Reading', 'tamaño del texto': 'Text size', 'normal': 'Normal',
        'grande': 'Large', 'muy grande': 'Extra large', 'visión y color': 'Vision and color',
        'paleta diferenciada para daltonismo': 'Color vision friendly palette',
        'los gráficos y estados usan colores distinguibles y conservan sus etiquetas.': 'Charts and statuses use distinguishable colors and keep their labels.',
        'alto contraste': 'High contrast', 'aumenta el contraste de fondos, texto, bordes y controles.': 'Increases contrast for backgrounds, text, borders, and controls.',
        'dashboard': 'Dashboard', 'ventas': 'Sales', 'caja general': 'Cash register',
        'reportes': 'Reports', 'productos': 'Products', 'proveedores': 'Suppliers', 'usuarios': 'Users',
        'bienvenido,': 'Welcome,', 'notificaciones': 'Notifications', 'cargando...': 'Loading...',
        'marcar todas como leídas': 'Mark all as read', 'marcar todas las notificaciones como leídas': 'Mark all notifications as read',
        'eliminar todas': 'Delete all', 'eliminar todas las notificaciones': 'Delete all notifications',
        'abrir menú de usuario': 'Open user menu', 'mi perfil': 'My profile', 'cerrar sesión': 'Sign out',
        'nombre': 'First name', 'apellido': 'Last name', 'correo': 'Email', 'verificar': 'Verify',
        'código de verificación': 'Verification code', '6 dígitos': '6 digits',
        'nueva contraseña (opcional)': 'New password (optional)', 'dejar en blanco para no cambiarla': 'Leave blank to keep unchanged',
        'repetir contraseña': 'Repeat password', 'repite la nueva contraseña': 'Repeat the new password',
        'guardar cambios': 'Save changes', '¿confirmar eliminación?': 'Confirm deletion?',
        'esta acción no se puede deshacer.': 'This action cannot be undone.', 'cancelar': 'Cancel', 'eliminar': 'Delete',
        'gestión de caja': 'Cash management', 'estado de caja hoy': "Today's cash register status",
        'vendedor': 'Seller', 'apertura': 'Opening', 'monto inicial': 'Opening amount', 'total ventas': 'Total sales',
        'devuelto': 'Refunded', 'monto final': 'Closing amount', 'estado': 'Status', 'acciones': 'Actions',
        'abierta': 'Open', 'cerrada': 'Closed', 'sin abrir': 'Not opened', 'historial de caja': 'Cash register history',
        'no hay registros de caja': 'No cash register records', 'abrir caja': 'Open cash register',
        'editar monto inicial': 'Edit opening amount', 'nuevo monto inicial (s/)': 'New opening amount (S/)',
        'total productos': 'Total products', 'total usuarios': 'Total users',
        'ventas del día': "Today's sales", 'ganancia neta del día': "Today's net profit",
        'sin alertas de stock': 'No stock alerts', 'sin datos aún': 'No data yet', 'unid.': 'units',
        'efectivo': 'Cash', 'tarjeta': 'Card', 'método de pago': 'Payment method',
        'fecha': 'Date', 'id': 'ID', 'total': 'Total',
        'con comprobante': 'With receipt', 'sin comprobante': 'Without receipt',
        'gestión de inventario': 'Inventory management', 'stock bajo': 'Low stock', 'agotados': 'Out of stock',
        'buscar producto': 'Search products', 'buscar producto...': 'Search products...',
        'todos los tipos': 'All types', 'todos los estados': 'All statuses', 'tipo': 'Type', 'categoría': 'Category',
        'precio venta': 'Sale price', 'activo': 'Active', 'inactivo': 'Inactive', 'servicio': 'Service', 'producto': 'Product',
        'categorías': 'Categories', 'sin categorías aún': 'No categories yet', 'agregar producto': 'Add product',
        'nombre del producto': 'Product name', 'descripción (opcional)': 'Description (optional)', 'precio venta (s/)': 'Sale price (S/)',
        'precio compra (s/)': 'Purchase price (S/)', 'stock inicial': 'Initial stock', 'stock mínimo (alerta)': 'Minimum stock (alert)',
        'seleccionar categoría': 'Select category', 'sin proveedor': 'No supplier', 'proveedor (opcional)': 'Supplier (optional)',
        'agregar servicio': 'Add service', 'nombre del servicio': 'Service name', 'nueva categoría': 'New category',
        'editar categoría': 'Edit category', 'editar producto': 'Edit product', 'stock mínimo': 'Minimum stock',
        'tipo de producto': 'Product type', 'análisis y estadísticas': 'Analysis and statistics',
        'reporte del día': "Today's report", 'n° ventas': 'No. of sales', 'total recaudado': 'Total collected',
        'sin cerrar': 'Not closed', 'historial últimos 7 días': 'Last 7 days history',
        'historial de ventas': 'Sales history', 'tabla de ventas y generación de comprobantes': 'Sales table and receipt generation',
        'filtrar por vendedor': 'Filter by seller', 'todos los vendedores': 'All sellers', 'buscar venta': 'Search sales',
        'comprobante': 'Receipt', 'emitido': 'Issued', 'no generada': 'Not generated',
        'informes de ventas mensuales': 'Monthly sales reports', 'generar informes de ventas por mes': 'Generate monthly sales reports',
        'no hay informes disponibles aún': 'No reports available yet', 'detalle de venta': 'Sale details',
        'cantidad': 'Quantity', 'p. unit.': 'Unit price', 'subtotal': 'Subtotal',
        'selecciona los productos y la cantidad a devolver:': 'Select the products and quantity to return:',
        'vendido': 'Sold', 'a devolver': 'To return', 'motivo (opcional)': 'Reason (optional)',
        'ventas de hoy': "Today's sales", 'ventas totales mensuales': 'Total monthly sales', 'promedio de ventas': 'Average sales',
        'venta actual': 'Current sale', 'descuento (s/)': 'Discount (S/)', 'op. gravada': 'Taxable amount',
        'igv (18%)': 'VAT (18%)', 'pagar': 'Pay', 'caja no aperturada': 'Cash register not opened',
        'gestión de proveedores': 'Supplier management', 'teléfono': 'Phone', 'dirección': 'Address',
        'agregar proveedor': 'Add supplier', 'nombre del proveedor': 'Supplier name',
        'editar proveedor': 'Edit supplier', 'gestión de usuarios': 'User management', 'rol': 'Role',
        'no hay usuarios registrados': 'No users registered', 'agregar usuario': 'Add user', 'contraseña': 'Password',
        'seleccionar rol': 'Select role', 'editar usuario': 'Edit user', 'algo salió mal': 'Something went wrong',
        'agregar categoría': 'Add category', 'editar servicio': 'Edit service',
        'editar': 'Edit', 'guardar': 'Save',
        'cerrar': 'Close', 'buscar...': 'Search...', 'todos': 'All',
        '¿eliminar esta categoría?': 'Delete this category?', '¿eliminar este proveedor?': 'Delete this supplier?',
        '¿eliminar este usuario?': 'Delete this user?', 'administrador': 'Administrator',
        'iniciar sesión': 'Sign in', 'sistema integral de gestión comercial': 'Integrated business management system',
        'iniciar sesión - papelería sonia': 'Sign in - Papelería Sonia',
        'recuperar contraseña - papelería sonia': 'Recover password - Papelería Sonia',
        'narración con voz': 'Text to speech', 'velocidad de lectura': 'Reading speed',
        'lenta': 'Slow', 'rápida': 'Fast', 'leer contenido': 'Read content', 'detener': 'Stop',
        'concentración': 'Focus', 'modo de concentración': 'Focus mode',
        'reduce animaciones y efectos decorativos para ayudar a mantener la atención.': 'Reduces animations and decorative effects to help maintain focus.',
        'lectura accesible': 'Reading support', 'formato de lectura para dislexia': 'Dyslexia friendly reading format',
        'usa una fuente sans serif y ajusta el espaciado para facilitar la lectura.': 'Uses a sans serif font and adjusts spacing to make reading easier.',
        'gestión de ventas': 'Sales management', 'control de stock': 'Stock control', 'reportes en tiempo real': 'Real-time reports',
        'bienvenido a papelería sonia': 'Welcome to Papelería Sonia',
        'ingrese su correo electrónico y contraseña para iniciar sesión en su cuenta.': 'Enter your email and password to sign in to your account.',
        'correo electrónico': 'Email address', 'ingrese su correo': 'Enter your email', 'ingrese su contraseña': 'Enter your password',
        'mostrar u ocultar contraseña': 'Show or hide password', '¿has olvidado tu contraseña?': 'Forgot your password?',
        'recuperar contraseña': 'Recover password', 'ingresa tu correo y te enviaremos un código de verificación.': 'Enter your email and we will send you a verification code.',
        'enviar código': 'Send code', 'volver al inicio de sesión': 'Back to sign in', 'verificar código': 'Verify code',
        'ingresa el código de 6 dígitos que enviamos a tu correo.': 'Enter the 6-digit code we sent to your email.',
        'nueva contraseña': 'New password', 'ingresa y confirma tu nueva contraseña.': 'Enter and confirm your new password.',
        'ingrese nueva contraseña': 'Enter new password', 'repita la contraseña': 'Repeat the password',
        'restablecer contraseña': 'Reset password', '¡contraseña restablecida!': 'Password reset!',
        'tu contraseña fue actualizada correctamente. ya puedes iniciar sesión con tu nueva contraseña.': 'Your password was updated. You can now sign in with your new password.',
        'ir al inicio de sesión': 'Go to sign in'
    };

    const originalText = new WeakMap();
    const originalAttributes = new WeakMap();
    const translatableAttributes = ['placeholder', 'title', 'aria-label'];
    const normalizeTranslationKey = value => value.replace(/\s+/g, ' ').trim().toLocaleLowerCase('es');
    const englishToSpanish = Object.fromEntries(
        Object.entries(spanishToEnglish).map(([spanish, english]) => [normalizeTranslationKey(english), spanish])
    );

    function splitOuterWhitespace(value) {
        let start = 0;
        let end = value.length;

        while (start < end && /\s/.test(value[start])) {
            start++;
        }

        while (end > start && /\s/.test(value[end - 1])) {
            end--;
        }

        return [
            value.slice(0, start),
            value.slice(start, end),
            value.slice(end)
        ];
    }

    function translateTextNode(node, language) {
        if (!originalText.has(node)) originalText.set(node, node.nodeValue);

        const original = originalText.get(node);
        const [leading, content, trailing] = splitOuterWhitespace(original);
        const key = normalizeTranslationKey(content);

        if (language === 'es') {
            const spanishText = Object.prototype.hasOwnProperty.call(spanishToEnglish, key)
                ? content
                : englishToSpanish[key] || content;

            node.nodeValue = leading + spanishText + trailing;
            return;
        }

        const englishText = spanishToEnglish[key] || content;
        node.nodeValue = leading + englishText + trailing;
    }

    function translateElementAttributes(element, language) {
        let saved = originalAttributes.get(element);
        if (!saved) {
            saved = new Map();
            originalAttributes.set(element, saved);
        }
        translatableAttributes.forEach(attribute => {
            if (!element.hasAttribute(attribute)) return;
            if (!saved.has(attribute)) saved.set(attribute, element.getAttribute(attribute));
            const original = saved.get(attribute);
            const key = normalizeTranslationKey(original);
            const translated = language === 'en'
                ? spanishToEnglish[key] || original
                : Object.prototype.hasOwnProperty.call(spanishToEnglish, key) ? original : englishToSpanish[key] || original;
            element.setAttribute(attribute, translated);
        });
    }

    function translateSubtree(root, language) {
        if (root.nodeType === Node.ELEMENT_NODE) translateElementAttributes(root, language);
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT | NodeFilter.SHOW_TEXT, {
            acceptNode(node) {
                if (node.nodeType === Node.ELEMENT_NODE) {
                    return /^(SCRIPT|STYLE|NOSCRIPT)$/i.test(node.tagName) ? NodeFilter.FILTER_REJECT : NodeFilter.FILTER_ACCEPT;
                }
                return NodeFilter.FILTER_ACCEPT;
            }
        });
        while (walker.nextNode()) {
            const node = walker.currentNode;
            if (node.nodeType === Node.TEXT_NODE) translateTextNode(node, language);
            else translateElementAttributes(node, language);
        }
    }

    function applyLanguage(language) {
        const selectedLanguage = language === 'en' ? 'en' : 'es';
        document.documentElement.lang = selectedLanguage;
        if (languageControl) languageControl.value = selectedLanguage;
        translateSubtree(document.body, selectedLanguage);
        if (selectedLanguage === 'en') {
            const titleParts = originalPageTitle.split(' - ');
            const translatedTitle = spanishToEnglish[normalizeTranslationKey(titleParts[0])] || titleParts[0];
            document.title = titleParts.length > 1
                ? `${translatedTitle} - ${titleParts.slice(1).join(' - ')}`
                : translatedTitle;
        } else {
            document.title = originalPageTitle || document.title;
        }
        try {
            localStorage.setItem('a11y-language', selectedLanguage);
        } catch {
            // El idioma se aplica aunque el navegador bloquee el almacenamiento.
        }
        window.dispatchEvent(new CustomEvent('a11y:languagechange', { detail: { language: selectedLanguage } }));
    }

    const originalPageTitle = document.title;
    let savedLanguage = 'es';
    try {
        savedLanguage = localStorage.getItem('a11y-language') === 'en' ? 'en' : 'es';
    } catch {
        // Se usa español como idioma predeterminado.
    }
    applyLanguage(savedLanguage);
    languageControl?.addEventListener('change', () => applyLanguage(languageControl.value));

    const languageObserver = new MutationObserver(records => {
        const language = document.documentElement.lang === 'en' ? 'en' : 'es';
        records.forEach(record => record.addedNodes.forEach(node => {
            if (node.nodeType === Node.ELEMENT_NODE || node.nodeType === Node.TEXT_NODE) {
                translateSubtree(node, language);
            }
        }));
    });
    languageObserver.observe(document.body, { childList: true, subtree: true });

    let speechJobId = 0;
    function speechMessage(spanish, english) {
        return document.documentElement.lang === 'en' ? english : spanish;
    }

    function setSpeechStatus(spanish, english) {
        if (speechStatus) speechStatus.textContent = speechMessage(spanish, english);
    }

    function stopNarration(showStatus = true) {
        speechJobId++;
        if ('speechSynthesis' in window) window.speechSynthesis.cancel();
        if (readPageButton) readPageButton.disabled = false;
        if (stopReadingButton) stopReadingButton.disabled = true;
        if (showStatus) setSpeechStatus('Narración detenida.', 'Narration stopped.');
    }

    function splitSpeechText(text, maxLength = 220) {
        const chunks = [];
        let current = '';
        text.split(/\s+/).forEach(word => {
            if (current && `${current} ${word}`.length > maxLength) {
                chunks.push(current);
                current = word;
            } else {
                current = current ? `${current} ${word}` : word;
            }
        });
        if (current) chunks.push(current);
        return chunks;
    }

    function readVisibleContent() {
        if (!('speechSynthesis' in window) || !('SpeechSynthesisUtterance' in window)) {
            setSpeechStatus('Este navegador no ofrece narración por voz.', 'This browser does not support text to speech.');
            return;
        }

        const content = document.querySelector('.main-content')
            || document.querySelector('.login-wrapper')
            || document.querySelector('.recover-nav')?.parentElement
            || document.body;
        const textWalker = document.createTreeWalker(content, NodeFilter.SHOW_TEXT, {
            acceptNode(node) {
                const parent = node.parentElement;
                if (!parent || parent.closest('.accessibility-panel, .accessibility-trigger, script, style, [aria-hidden="true"]')) {
                    return NodeFilter.FILTER_REJECT;
                }

                for (let element = parent; element; element = element.parentElement) {
                    if (element.hidden) return NodeFilter.FILTER_REJECT;
                    const style = window.getComputedStyle(element);
                    if (style.display === 'none' || style.visibility === 'hidden') return NodeFilter.FILTER_REJECT;
                    if (element === content) break;
                }
                return NodeFilter.FILTER_ACCEPT;
            }
        });
        const textParts = [];
        while (textWalker.nextNode()) {
            const part = textWalker.currentNode.nodeValue.trim();
            if (part) textParts.push(part);
        }
        const text = textParts.join(' ').replace(/\s+/g, ' ').trim();
        if (!text) {
            setSpeechStatus('No hay contenido visible para leer.', 'There is no visible content to read.');
            return;
        }

        const chunks = splitSpeechText(text);
        const currentJob = ++speechJobId;
        window.speechSynthesis.cancel();
        if (readPageButton) readPageButton.disabled = true;
        if (stopReadingButton) stopReadingButton.disabled = false;
        setSpeechStatus('Narración en curso.', 'Reading aloud.');

        chunks.forEach((chunk, index) => {
            const utterance = new SpeechSynthesisUtterance(chunk);
            utterance.lang = document.documentElement.lang === 'en' ? 'en-US' : 'es-PE';
            utterance.rate = Number(speechRateControl?.value || 1);
            utterance.onend = () => {
                if (currentJob !== speechJobId || index !== chunks.length - 1) return;
                if (readPageButton) readPageButton.disabled = false;
                if (stopReadingButton) stopReadingButton.disabled = true;
                setSpeechStatus('Narración finalizada.', 'Reading finished.');
            };
            utterance.onerror = event => {
                if (currentJob !== speechJobId || event.error === 'canceled' || event.error === 'interrupted') return;
                if (readPageButton) readPageButton.disabled = false;
                if (stopReadingButton) stopReadingButton.disabled = true;
                setSpeechStatus('No se pudo reproducir la narración.', 'Speech could not be played.');
            };
            window.speechSynthesis.speak(utterance);
        });
    }

    readPageButton?.addEventListener('click', readVisibleContent);
    stopReadingButton?.addEventListener('click', () => stopNarration());
    window.addEventListener('a11y:languagechange', () => stopNarration(false));

    textSizeControl?.addEventListener('change', () => {
        const selectedSize = textSizeControl.value;
        applyTextSize(selectedSize);

        try {
            localStorage.setItem('a11y-text-size', selectedSize);
        } catch {
            // El tamaño se aplica mientras esta página permanezca abierta.
        }
    });

    function readSavedBoolean(key) {
        try {
            return localStorage.getItem(key) === 'true';
        } catch {
            return false;
        }
    }

    if (focusModeControl) {
        focusModeControl.checked = readSavedBoolean('a11y-focus-mode');
    }

    function applyFocusMode() {
        const enabled = Boolean(focusModeControl?.checked);
        document.documentElement.dataset.a11yFocus = enabled ? 'on' : 'off';
        try {
            localStorage.setItem('a11y-focus-mode', String(enabled));
        } catch {
            // El modo se mantiene activo mientras esta página permanezca abierta.
        }
    }

    applyFocusMode();
    focusModeControl?.addEventListener('change', applyFocusMode);

    if (dyslexiaModeControl) {
        dyslexiaModeControl.checked = readSavedBoolean('a11y-dyslexia-mode');
    }

    function applyDyslexiaMode() {
        const enabled = Boolean(dyslexiaModeControl?.checked);
        document.documentElement.dataset.a11yDyslexia = enabled ? 'on' : 'off';
        try {
            localStorage.setItem('a11y-dyslexia-mode', String(enabled));
        } catch {
            // El formato se mantiene mientras esta página permanezca abierta.
        }
    }

    applyDyslexiaMode();
    dyslexiaModeControl?.addEventListener('change', applyDyslexiaMode);

    if (colorVisionControl) {
        colorVisionControl.checked = readSavedBoolean('a11y-color-vision');
    }
    if (highContrastControl) {
        highContrastControl.checked = readSavedBoolean('a11y-high-contrast');
    }

    function applyVisionSettings() {
        const colorVision = Boolean(colorVisionControl?.checked);
        const highContrast = Boolean(highContrastControl?.checked);
        const root = document.documentElement;

        root.dataset.a11yColorVision = colorVision ? 'safe' : 'standard';
        root.dataset.a11yContrast = highContrast ? 'high' : 'standard';

        try {
            localStorage.setItem('a11y-color-vision', String(colorVision));
            localStorage.setItem('a11y-high-contrast', String(highContrast));
        } catch {
            // La preferencia se aplica aunque el navegador bloquee el almacenamiento.
        }

        window.dispatchEvent(new CustomEvent('a11y:visionchange', {
            detail: { colorVision, highContrast }
        }));
    }

    applyVisionSettings();
    colorVisionControl?.addEventListener('change', applyVisionSettings);
    highContrastControl?.addEventListener('change', applyVisionSettings);

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
