(() => {
    async function loadStatistics() {
        try {
            const res = await fetch("/Dashboard/dashboard/statistics");
            const data = await res.json();

            // DAU 折線圖
            const chartDAU = document.getElementById('chartDAU');
            if (chartDAU) {
                const labels = data.dau.map(d => new Date(d.date).toLocaleDateString("zh-TW", { weekday: 'short' }));
                const counts = data.dau.map(d => d.count);

                new Chart(chartDAU, {
                    type: 'line',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'DAU',
                            data: counts,
                            borderColor: '#36b9cc',
                            fill: false
                        }]
                    }
                });
            }

           

        } catch (err) {
            console.error("📊 統計圖表載入失敗", err);
        }
    }

    // 初始化執行
    loadStatistics();
}
)();