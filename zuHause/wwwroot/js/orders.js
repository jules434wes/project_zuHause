     
(() => {
    const orderStats = [
        { type: '✅ 租屋成交單數', count: 52, note: '已完成付款並確認租約' },
        { type: '💳 已驗證未付款單數', count: 8, note: '尚未完成金流，等待付款' },
        { type: '🚫 家具退租申請數', count: 3, note: '租客已提交家具退租申請' },
        { type: '🔎 詐欺交易警示', count: 1, note: '房東OR租客訂單檢舉成立' },
        { type: '❓  可疑訂單', count: 1, note: '房東OR租客對訂單提交檢舉' }
    ];
    const orderStatsTable = document.getElementById('orderStatsTable');
    if (orderStatsTable) {
        let html = '<table class="table table-bordered text-center"><thead><tr><th>類型</th><th>數量</th><th>備註</th></tr></thead><tbody>';
        orderStats.forEach(row => {
            html += `<tr><td>${row.type}</td><td>${row.count}</td><td>${row.note}</td></tr>`;
        });
        html += '</tbody></table>';
        orderStatsTable.innerHTML = html;
    }

    const chartPay = document.getElementById('chartPay');
    if (chartPay) {
        new Chart(chartPay, {
            type: 'doughnut',
            data: {
                labels: ['上架服務費實收款', '上架服務費未收款'],
                datasets: [{ data: [56400, 30000], backgroundColor: ['#1cc88a', '#e74a3b'] }]
            },
            options: { plugins: { title: { display: true, text: '實收款與未收款占比分析' } } }
        });
    }
    })();