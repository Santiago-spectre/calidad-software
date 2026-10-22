// home.js — lógica del dashboard (gráficas y filtro)

const ctxSemana = document.getElementById('chartSemana').getContext('2d');
let chartSemana = new Chart(ctxSemana, {
    type: 'bar',
    data: {
        labels: weekDaysData,
        datasets: [{
            label: 'Ventas (S/)',
            data: weekSalesData,
            backgroundColor: 'rgba(99,102,241,0.5)',
            borderColor: '#6366f1',
            borderWidth: 2,
            borderRadius: 8,
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: { display: false }
        },
        scales: {
            x: { ticks: { color: 'rgba(255,255,255,0.5)' }, grid: { color: 'rgba(255,255,255,0.05)' } },
            y: { ticks: { color: 'rgba(255,255,255,0.5)' }, grid: { color: 'rgba(255,255,255,0.05)' } }
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
                borderColor: '#34d399',
                backgroundColor: 'rgba(52,211,153,0.15)',
                borderWidth: 2,
                tension: 0.3,
                fill: true,
            },
            {
                label: 'Invertido (S/)',
                data: weekInvertidoData,
                borderColor: '#fbbf24',
                backgroundColor: 'rgba(251,191,36,0.15)',
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
                labels: { color: 'rgba(255,255,255,0.6)' }
            }
        },
        scales: {
            x: { ticks: { color: 'rgba(255,255,255,0.5)' }, grid: { color: 'rgba(255,255,255,0.05)' } },
            y: { ticks: { color: 'rgba(255,255,255,0.5)' }, grid: { color: 'rgba(255,255,255,0.05)' } }
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
            backgroundColor: ['#fbbf24', '#34d399', '#60a5fa', '#6366f1'],
            borderWidth: 0,
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: {
                position: 'right',
                align: 'start',
                labels: { color: 'rgba(255,255,255,0.6)', padding: 15 }
            }
        }
    }
});

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
        tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:rgba(255,255,255,0.3);font-size:13px;padding:30px;">No hay ventas registradas aún</td></tr>';
        return;
    }
    tbody.innerHTML = data.lastSales.map(v => `
        <tr style="border-bottom: 1px solid rgba(255,255,255,0.04);">
            <td style="color:rgba(255,255,255,0.6);font-size:13px;padding:12px 15px;">#${v.saleId}</td>
            <td style="color:rgba(255,255,255,0.6);font-size:13px;padding:12px 15px;">${v.fecha}</td>
            <td style="color:#ffffff;font-size:13px;padding:12px 15px;">${v.vendedor}</td>
            <td style="color:rgba(255,255,255,0.6);font-size:13px;padding:12px 15px;">${v.paymentMethod}</td>
            <td style="color:#34d399;font-size:13px;font-weight:700;padding:12px 15px;">S/ ${v.total.toFixed(2)}</td>
            <td style="padding:12px 15px;">
                ${v.receiptIssued
            ? '<span style="background:rgba(52,211,153,0.15);color:#34d399;padding:4px 10px;border-radius:20px;font-size:11px;">Con comprobante</span>'
            : '<span style="background:rgba(251,191,36,0.15);color:#fbbf24;padding:4px 10px;border-radius:20px;font-size:11px;">Sin comprobante</span>'
        }
            </td>
        </tr>
    `).join('');
}