(() => {
    const chartCPU = document.getElementById('chartCPU');
    if (chartCPU) {
        new Chart(chartCPU, {
            type: 'bar',
            data: { labels: ['CPU', 'RAM', 'API'], datasets: [{ label: '資源使用', data: [18, 62, 230], backgroundColor: '#f6c23e' }] }
        });
    } })();