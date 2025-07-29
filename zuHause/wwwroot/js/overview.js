(() => {
    // 🔁 記錄圖表實例
    const chartInstances = {};

    // ✅ 通用繪圖函式（不要放在 initOverviewDashboard 裡）
    const renderChart = (canvasId, label, data, labels, type = 'line', color = '#1cc88a') => {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        // 銷毀舊的圖表
        if (chartInstances[canvasId]) {
            chartInstances[canvasId].destroy();
        }

        const chart = new Chart(canvas, {
            type,
            data: {
                labels: labels,
                datasets: [{
                    label,
                    data,
                    borderColor: color,
                    backgroundColor: type === 'bar' ? color : 'transparent',
                    fill: false
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true }
                },
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });

        chartInstances[canvasId] = chart;
    };

    // ✅ 主邏輯
    window.initOverviewDashboard = async function () {
        const select = document.getElementById('dataRangeSelect');
        const range = select?.value || 'week';
        const rangeLabel = range === "month" ? "本月" : "本週";

        // ✅ 更新所有相關文字標籤
        document.querySelectorAll("#rangeLabel").forEach(el => el.textContent = rangeLabel);

        try {
            const res = await fetch(`/Dashboard/dashboard/stats?range=${range}`);
            const stats = await res.json();

            // ✅ 動態標籤處理
            let label;
            if (range === "week") label = "本週";
            else if (range === "month") label = "本月";
            else label = `${parseInt(range)} 月`;

            // ✅ 更新卡片文字

            document.getElementById('rangeLabelRegister').textContent = label;
            document.getElementById('rangeLabelProperty').textContent = label;
            document.getElementById('rangeLabelFurniture').textContent = label;
            document.querySelector('#todayRegister').textContent = `${stats.today.register} 位`;
            document.querySelector('#todayProperty').textContent = `${stats.today.property} 間`;
            document.querySelector('#todayFurniture').textContent = `${stats.today.furniture} 件`;
            document.querySelector('#weekRegister').textContent = `${stats.weekly.register.reduce((a, b) => a + b, 0)} 位`;
            document.querySelector('#weekProperty').textContent = `${stats.weekly.property.reduce((a, b) => a + b, 0)} 間`;
            document.querySelector('#weekFurniture').textContent = `${stats.weekly.furniture.reduce((a, b) => a + b, 0)} 件`;
            document.getElementById('todayRegister').textContent = `${stats.today.register} 位`;

            // ✅ 初始化所有圖表（用外部的 renderChart）
            const labels = stats.weekly.labels;

            renderChart('total_registration', '註冊量', stats.weekly.register, labels);
            renderChart('Total_Listings_Houses', '上架房源量', stats.weekly.property, labels);
            renderChart('Total_Shelves_Furniture', '出租家具訂單量', stats.weekly.furniture, labels);
            renderChart('chartOrders', '出租家具金額', stats.weekly.furnitureRevenue, labels, 'bar', '#4e73df');
            renderChart('chartRevenue', '上架服務費金額', stats.weekly.listingFee, labels, 'line', '#36b9cc');

            // ✅ 綁定下拉只做一次
            if (!select.dataset.bound) {
                select.addEventListener("change", () => {
                    initOverviewDashboard();
                });
                select.dataset.bound = "true";
            }

        } catch (err) {
            console.error('📉 統計資料載入失敗', err);
        }
    };

})();
