(() => {
    async function loadStatistics() {
        try {
            const res = await fetch("/Dashboard/dashboard/statistics");
            const data = await res.json();

            // 📊 近五日 DAU
            const chartDAU = document.getElementById('chartDAU');
            if (chartDAU) {
                const labels = data.dau.map(d => new Date(d.date).toLocaleDateString("zh-TW", { weekday: 'short' }));
                const counts = data.dau.map(d => d.count);

                new Chart(chartDAU, {
                    type: 'line',
                    data: {
                        labels,
                        datasets: [{
                            label: 'DAU',
                            data: counts,
                            borderColor: '#36b9cc',
                            fill: false
                        }]
                    }
                });
            }

            // 📅 本月每日 DAU
            const chartMonth = document.getElementById('chartMonthDAU');
            if (chartMonth) {
                const labels = data.month.map(d => new Date(d.date).getDate() + "日");
                const counts = data.month.map(d => d.count);

                new Chart(chartMonth, {
                    type: 'line',
                    data: {
                        labels,
                        datasets: [{
                            label: '本月活躍用戶',
                            data: counts,
                            borderColor: '#1cc88a',
                            fill: false
                        }]
                    }
                });
            }

            // 📆 今年每月 DAU
            const chartYear = document.getElementById('chartYearDAU');
            if (chartYear) {
                const labels = data.year.map(d => d.month + "月");
                const counts = data.year.map(d => d.count);

                new Chart(chartYear, {
                    type: 'line',
                    data: {
                        labels,
                        datasets: [{
                            label: '今年月活躍用戶',
                            data: counts,
                            borderColor: '#f6c23e',
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