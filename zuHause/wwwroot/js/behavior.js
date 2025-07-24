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

            // 熱門搜尋關鍵字長條圖
            const chartTags = document.getElementById('chartTags');
            if (chartTags) {
                const labels = data.keywords.map(k => k.keyword);
                const counts = data.keywords.map(k => k.count);

                new Chart(chartTags, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: '搜尋次數',
                            data: counts,
                            backgroundColor: '#4e73df'
                        }]
                    },
                    options: {
                        indexAxis: 'y'
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