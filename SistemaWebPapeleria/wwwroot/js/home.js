// home.js — lógica del dashboard (gráficas y filtro)

function getAccessibilityColor(token, fallback) {
    const value = getComputedStyle(document.documentElement).getPropertyValue(token).trim();
    return value || fallback;
}

function getAccessibilityChartFontSize(baseSize) {
    const scale = Number(getComputedStyle(document.documentElement).getPropertyValue('--a11y-text-scale').trim()) || 1;
    return Math.round(baseSize * scale);
}

const ctxSemana = document.getElementById('chartSemana').getContext('2d');
let chartSemana = new Chart(ctxSemana, {
    type: 'bar',
    data: {
        labels: weekDaysData,
        datasets: [{
            label: 'Ventas (S/)',
            data: weekSalesData,
            backgroundColor: getAccessibilityColor('--a11y-chart-primary-fill', 'rgba(99,102,241,0.5)'),
            borderColor: getAccessibilityColor('--a11y-chart-primary', '#6366f1'),
            borderWidth: 2,
            borderRadius: 8,
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: { display: false },
            tooltip: {
                titleFont: { size: getAccessibilityChartFontSize(14) },
                bodyFont: { size: getAccessibilityChartFontSize(14) }
            }
        },
        scales: {
            x: { ticks: { color: getAccessibilityColor('--a11y-chart-ticks', 'rgba(255,255,255,0.5)'), font: { size: getAccessibilityChartFontSize(12) } }, grid: { color: getAccessibilityColor('--a11y-chart-grid', 'rgba(255,255,255,0.05)') } },
            y: { ticks: { color: getAccessibilityColor('--a11y-chart-ticks', 'rgba(255,255,255,0.5)'), font: { size: getAccessibilityChartFontSize(12) } }, grid: { color: getAccessibilityColor('--a11y-chart-grid', 'rgba(255,255,255,0.05)') } }
        }
    }
});

const ctxGanancia = document.getElementById('chartGanancia').getContext('2d');
let chartGanancia = new Chart(ctxGanancia, {
    type: 'line',
    data: {
        labels: weekDaysData,
        datasets: [
            {
                label: 'Ganancia (S/)',
                data: weekGananciaData,
                borderColor: getAccessibilityColor('--a11y-chart-success', '#34d399'),
                backgroundColor: getAccessibilityColor('--a11y-chart-success-fill', 'rgba(52,211,153,0.15)'),
                borderWidth: 2,
                tension: 0.3,
                fill: true,
            },
            {
                label: 'Invertido (S/)',
                data: weekInvertidoData,
                borderColor: getAccessibilityColor('--a11y-chart-warning', '#fbbf24'),
                backgroundColor: getAccessibilityColor('--a11y-chart-warning-fill', 'rgba(251,191,36,0.15)'),
                borderWidth: 2,
                tension: 0.3,
                fill: true,
            }
        ]
    },
    options: {
        responsive: true,
        plugins: {
            legend: {
                labels: { color: getAccessibilityColor('--a11y-chart-text', 'rgba(255,255,255,0.6)'), font: { size: getAccessibilityChartFontSize(12) } }
            },
            tooltip: {
                titleFont: { size: getAccessibilityChartFontSize(14) },
                bodyFont: { size: getAccessibilityChartFontSize(14) }
            }
        },
        scales: {
            x: { ticks: { color: getAccessibilityColor('--a11y-chart-ticks', 'rgba(255,255,255,0.5)'), font: { size: getAccessibilityChartFontSize(12) } }, grid: { color: getAccessibilityColor('--a11y-chart-grid', 'rgba(255,255,255,0.05)') } },
            y: { ticks: { color: getAccessibilityColor('--a11y-chart-ticks', 'rgba(255,255,255,0.5)'), font: { size: getAccessibilityChartFontSize(12) } }, grid: { color: getAccessibilityColor('--a11y-chart-grid', 'rgba(255,255,255,0.05)') } }
        }
    }
});

const ctxPago = document.getElementById('chartPago').getContext('2d');

let chartPago = new Chart(ctxPago, {
    type: 'doughnut',
    data: {
        labels: ['Efectivo', 'Yape', 'Plin', 'Tarjeta'],
        datasets: [{
            data: pagoData,
            backgroundColor: [
                getAccessibilityColor('--a11y-chart-payment-cash', '#fbbf24'),
                getAccessibilityColor('--a11y-chart-payment-yape', '#34d399'),
                getAccessibilityColor('--a11y-chart-payment-plin', '#60a5fa'),
                getAccessibilityColor('--a11y-chart-payment-card', getAccessibilityColor('--a11y-chart-primary', '#6366f1'))
            ],
            borderWidth: 0,
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: {
                position: 'right',
                align: 'start',
                labels: { color: getAccessibilityColor('--a11y-chart-text', 'rgba(255,255,255,0.6)'), padding: 15, font: { size: getAccessibilityChartFontSize(12) } }
            },
            tooltip: {
                titleFont: { size: getAccessibilityChartFontSize(14) },
                bodyFont: { size: getAccessibilityChartFontSize(14) }
            }
        }
    }
});

function actualizarColoresAccesiblesDeGraficas() {
    const chartText = getAccessibilityColor('--a11y-chart-text', 'rgba(255,255,255,0.6)');
    const tickColor = getAccessibilityColor('--a11y-chart-ticks', 'rgba(255,255,255,0.5)');
    const gridColor = getAccessibilityColor('--a11y-chart-grid', 'rgba(255,255,255,0.05)');

    chartSemana.data.datasets[0].backgroundColor = getAccessibilityColor('--a11y-chart-primary-fill', 'rgba(99,102,241,0.5)');
    chartSemana.data.datasets[0].borderColor = getAccessibilityColor('--a11y-chart-primary', '#6366f1');
    chartGanancia.data.datasets[0].borderColor = getAccessibilityColor('--a11y-chart-success', '#34d399');
    chartGanancia.data.datasets[0].backgroundColor = getAccessibilityColor('--a11y-chart-success-fill', 'rgba(52,211,153,0.15)');
    chartGanancia.data.datasets[1].borderColor = getAccessibilityColor('--a11y-chart-warning', '#fbbf24');
    chartGanancia.data.datasets[1].backgroundColor = getAccessibilityColor('--a11y-chart-warning-fill', 'rgba(251,191,36,0.15)');
    chartPago.data.datasets[0].backgroundColor = [
        getAccessibilityColor('--a11y-chart-payment-cash', '#fbbf24'),
        getAccessibilityColor('--a11y-chart-payment-yape', '#34d399'),
        getAccessibilityColor('--a11y-chart-payment-plin', '#60a5fa'),
        getAccessibilityColor('--a11y-chart-payment-card', '#6366f1')
    ];

    chartGanancia.options.plugins.legend.labels.color = chartText;
    chartPago.options.plugins.legend.labels.color = chartText;
    [chartSemana, chartGanancia].forEach(chart => {
        chart.options.scales.x.ticks.color = tickColor;
        chart.options.scales.y.ticks.color = tickColor;
        chart.options.scales.x.grid.color = gridColor;
        chart.options.scales.y.grid.color = gridColor;
    });

    chartSemana.update();
    chartGanancia.update();
    chartPago.update();
}

window.addEventListener('a11y:visionchange', actualizarColoresAccesiblesDeGraficas);

function actualizarTamanoTextoDeGraficas() {
    const fontSize = getAccessibilityChartFontSize(12);
    const tooltipFontSize = getAccessibilityChartFontSize(14);
    [chartSemana, chartGanancia].forEach(chart => {
        chart.options.scales.x.ticks.font.size = fontSize;
        chart.options.scales.y.ticks.font.size = fontSize;
    });
    chartGanancia.options.plugins.legend.labels.font.size = fontSize;
    chartPago.options.plugins.legend.labels.font.size = fontSize;
    [chartSemana, chartGanancia, chartPago].forEach(chart => {
        chart.options.plugins.tooltip.titleFont.size = tooltipFontSize;
        chart.options.plugins.tooltip.bodyFont.size = tooltipFontSize;
        chart.update();
    });
}

window.addEventListener('a11y:textsizechange', actualizarTamanoTextoDeGraficas);

async function aplicarFiltro() {
    const userId = document.getElementById('filtroUsuario')?.value ?? '';
    const url = '/Home/GetDashboardData' + (userId ? '?userId=' + userId : '');

    const res = await fetch(url);
    const data = await res.json();

    // Actualizar gráfica de semana
    chartSemana.data.labels = data.weekDays;
    chartSemana.data.datasets[0].data = data.weekSales;
    chartSemana.update();

    // Actualizar gráfica de ganancia/invertido
    chartGanancia.data.labels = data.weekDays;
    chartGanancia.data.datasets[0].data = data.weekGanancia;
    chartGanancia.data.datasets[1].data = data.weekInvertido;
    chartGanancia.update();

    // Actualizar badge de ganancia neta
    const netoSemana = data.weekGanancia.reduce((a, b) => a + b, 0);
    const badge = document.getElementById('gananciaNetaBadge');
    badge.textContent = 'Neto: S/ ' + netoSemana.toFixed(2);
    badge.classList.toggle('pill-badge--success', netoSemana >= 0);
    badge.classList.toggle('pill-badge--danger', netoSemana < 0);

    // Actualizar gráfica de dona
    chartPago.data.datasets[0].data = [data.efectivo, data.yape, data.plin, data.tarjeta];
    chartPago.update();

    // Actualizar resumen de pagos
    document.getElementById('resEfectivo').textContent = 'S/ ' + data.efectivo.toFixed(2);
    document.getElementById('resYape').textContent = 'S/ ' + data.yape.toFixed(2);
    document.getElementById('resPlin').textContent = 'S/ ' + data.plin.toFixed(2);
    document.getElementById('resTarjeta').textContent = 'S/ ' + data.tarjeta.toFixed(2);

    // Actualizar tabla últimas ventas
    const tbody = document.getElementById('tbodyVentas');
    if (data.lastSales.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--a11y-muted-text, rgba(255,255,255,0.3));font-size: calc(13px * var(--a11y-text-scale, 1));padding:30px;">No hay ventas registradas aún</td></tr>';
        return;
    }
    tbody.innerHTML = data.lastSales.map(v => `
        <tr style="border-bottom: 1px solid rgba(255,255,255,0.04);">
            <td style="color:var(--a11y-muted-text, rgba(255,255,255,0.6));font-size: calc(13px * var(--a11y-text-scale, 1));padding:12px 15px;">#${v.saleId}</td>
            <td style="color:var(--a11y-muted-text, rgba(255,255,255,0.6));font-size: calc(13px * var(--a11y-text-scale, 1));padding:12px 15px;">${v.fecha}</td>
            <td style="color:#ffffff;font-size: calc(13px * var(--a11y-text-scale, 1));padding:12px 15px;">${v.vendedor}</td>
            <td style="color:var(--a11y-muted-text, rgba(255,255,255,0.6));font-size: calc(13px * var(--a11y-text-scale, 1));padding:12px 15px;">${v.paymentMethod}</td>
            <td style="color:var(--color-success);font-size: calc(13px * var(--a11y-text-scale, 1));font-weight:700;padding:12px 15px;">S/ ${v.total.toFixed(2)}</td>
            <td style="padding:12px 15px;">
                ${v.receiptIssued
            ? '<span style="background:rgba(var(--a11y-positive-rgb),0.18);color:var(--color-success);padding:4px 10px;border-radius:20px;font-size: calc(11px * var(--a11y-text-scale, 1));">Con comprobante</span>'
            : '<span style="background:rgba(var(--a11y-warning-rgb),0.18);color:var(--color-warning);padding:4px 10px;border-radius:20px;font-size: calc(11px * var(--a11y-text-scale, 1));">Sin comprobante</span>'
        }
            </td>
        </tr>
    `).join('');
}
