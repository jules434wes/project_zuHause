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
            // 📝 申請行為統計圖（近五日）
            const chartApp = document.getElementById('chartApplication');
            if (chartApp) {
                const labels = data.application.map(d => new Date(d.date).toLocaleDateString("zh-TW", { weekday: 'short' }));
                const rentalCounts = data.application.map(d => d.rentalCount);
                const viewingCounts = data.application.map(d => d.viewingCount);

                new Chart(chartApp, {
                    type: 'line',
                    data: {
                        labels,
                        datasets: [
                            {
                                label: '租賃申請',
                                data: rentalCounts,
                                borderColor: '#4e73df',
                                fill: false
                            },
                            {
                                label: '看房申請',
                                data: viewingCounts,
                                borderColor: '#e74a3b',
                                fill: false
                            }
                        ]
                    }
                });
            }
            // 📅 今月每日申請趨勢
            const chartAppMonth = document.getElementById('chartApplicationMonth');
            if (chartAppMonth) {
                const labels = data.applicationMonth.map(d => new Date(d.date).getDate() + "日");
                const rentalCounts = data.applicationMonth.map(d => d.rentalCount);
                const viewingCounts = data.applicationMonth.map(d => d.viewingCount);

                new Chart(chartAppMonth, {
                    type: 'line',
                    data: {
                        labels,
                        datasets: [
                            {
                                label: '租賃申請',
                                data: rentalCounts,
                                borderColor: '#4e73df',
                                fill: false
                            },
                            {
                                label: '看房申請',
                                data: viewingCounts,
                                borderColor: '#e74a3b',
                                fill: false
                            }
                        ]
                    }
                });
            }


            // 📆 今年每月申請趨勢
            const chartAppYear = document.getElementById('chartApplicationYear');
            if (chartAppYear) {
                const labels = data.applicationYear.map(d => d.month + "月");
                const rentalCounts = data.applicationYear.map(d => d.rentalCount);
                const viewingCounts = data.applicationYear.map(d => d.viewingCount);

                new Chart(chartAppYear, {
                    type: 'line',
                    data: {
                        labels,
                        datasets: [
                            {
                                label: '租賃申請',
                                data: rentalCounts,
                                borderColor: '#1cc88a',
                                fill: false
                            },
                            {
                                label: '看房申請',
                                data: viewingCounts,
                                borderColor: '#f6c23e',
                                fill: false
                            }
                        ]
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