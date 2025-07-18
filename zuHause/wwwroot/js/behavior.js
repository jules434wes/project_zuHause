(() => {
    const chartDAU = document.getElementById('chartDAU');
    if (chartDAU) {
        new Chart(chartDAU, {
            type: 'line',
            data: { labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'], datasets: [{ label: 'DAU', data: [120, 132, 109, 121, 128], borderColor: '#36b9cc', fill: false }] }
        });
    }
    const chartTags = document.getElementById('chartTags');
    if (chartTags) {
        new Chart(chartTags, {
            type: 'bar',
            data: {
                labels: ['雙人床', '捷運', '書桌', '套房', '陽台', '洗衣機', '收納櫃', '近學校', '家具全配'],
                datasets: [{ label: '搜尋次數', data: [30, 25, 18, 20, 15, 10, 12, 16, 22], backgroundColor: '#4e73df' }]
            },
            options: { indexAxis: 'y' }
        });
    }
}
)();