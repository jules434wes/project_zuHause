(() => {
    let serverChart;
    let refreshInterval;

    async function loadServerStatusChart() {
        try {
            const res = await fetch("/Dashboard/server-status");
            const data = await res.json();

            const chartCPU = document.getElementById('chartCPU');

            if (chartCPU) {
                if (serverChart) {
                    // 更新資料
                    serverChart.data.datasets[0].data = [data.cpu, data.ram, data.api];
                    serverChart.update();
                } else {
                    // 第一次建立圖表
                    serverChart = new Chart(chartCPU, {
                        type: 'bar',
                        data: {
                            labels: ['CPU (%)', 'RAM (MB)', 'API 請求數'],
                            datasets: [{
                                label: '資源使用情況',
                                data: [data.cpu, data.ram, data.api],
                                backgroundColor: ['#f6c23e', '#4e73df', '#1cc88a']
                            }]
                        },
                        options: {
                            plugins: {
                                title: {
                                    display: true,
                                    text: '主機資源使用狀況'
                                }
                            }
                        }
                    });
                }
            }
        } catch (err) {
            console.error("⚠️ 主機資源載入失敗", err);
        }
    }

    // 初始化 + 每10秒刷新
    window.loadServerStatusChart = function () {
        if (refreshInterval) clearInterval(refreshInterval);
        loadServerStatusChart(); // 第一次
        refreshInterval = setInterval(loadServerStatusChart, 1000); // 每 1 秒
    };
})();
