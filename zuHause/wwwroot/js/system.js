(() => {
    let serverCharts = {};
    let refreshInterval;
    const historyLength = 60;
    const metrics = ['cpu', 'ram', 'disk', 'netUpload', 'netDownload'];
    const history = {};
    metrics.forEach(m => history[m] = []);
    history.labels = [];

    async function loadServerStatusChart() {
        try {
            const res = await fetch("/Dashboard/server-status");
            const data = await res.json();
            const now = new Date();
            const label = now.toLocaleTimeString();

            // 維持歷史長度
            metrics.forEach(key => {
                history[key].push(data[key]);
                if (history[key].length > historyLength) history[key].shift();
            });
            history.labels.push(label);
            if (history.labels.length > historyLength) history.labels.shift();

            metrics.forEach(metric => {
                const chartId = `chart_${metric}`;
                const chartElem = document.getElementById(chartId);
                if (chartElem) {
                    if (serverCharts[metric]) {
                        serverCharts[metric].data.labels = history.labels;
                        serverCharts[metric].data.datasets[0].data = history[metric];
                        serverCharts[metric].update();
                    } else {
                        serverCharts[metric] = new Chart(chartElem, {
                            type: 'line',
                            data: {
                                labels: history.labels,
                                datasets: [
                                    {
                                        label: getMetricLabel(metric),
                                        data: history[metric],
                                        borderColor: getMetricColor(metric),
                                        backgroundColor: 'rgba(0,0,0,0)',
                                        tension: 0.2
                                    }
                                ]
                            },
                            options: {
                                plugins: {
                                    title: {
                                        display: true,
                                        text: getMetricLabel(metric) + ' 歷史趨勢'
                                    }
                                },
                                scales: {
                                    y: { beginAtZero: true }
                                }
                            }
                        });
                    }
                }
            });
        } catch (err) {
            console.error("⚠️ 主機資源載入失敗", err);
        }
    }

    function getMetricLabel(metric) {
        switch (metric) {
            case 'cpu': return 'CPU (%)';
            case 'ram': return 'RAM (MB)';
            case 'disk': return '磁碟剩餘(GB)';
            case 'netUpload': return '上傳速率 (Mbps)';
            case 'netDownload': return '下載速率 (Mbps)';
            default: return metric;
        }
    }
    function getMetricColor(metric) {
        switch (metric) {
            case 'cpu': return '#f6c23e';
            case 'ram': return '#4e73df';
            case 'disk': return '#36b9cc';
            case 'netUpload': return '#e74a3b';
            case 'netDownload': return '#1cc88a';
            default: return '#858796';
        }
    }

    window.loadServerStatusChart = function () {
        if (refreshInterval) clearInterval(refreshInterval);
        loadServerStatusChart();
        refreshInterval = setInterval(loadServerStatusChart, 5000);
    };
})();
