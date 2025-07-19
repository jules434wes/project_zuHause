(() => {
    const chartOrders = document.getElementById('chartOrders');
    if (chartOrders) {
        new Chart(chartOrders, {
            type: 'bar',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
                datasets: [{
                    label: '出租家具金額',
                    data: [5, 9, 7, 14, 10],
                    backgroundColor: '#4e73df'
                }]
            }
        });
    }

    const chartRevenue = document.getElementById('chartRevenue');
    if (chartRevenue) {
        new Chart(chartRevenue, {
            type: 'line',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
                datasets: [{
                    label: '上架服務費金額',
                    data: [12000, 18000, 11000, 23000, 17500],
                    borderColor: '#1cc88a',
                    fill: false
                }]
            }
        });
    }

    const totalReg = document.getElementById('total_registration');
    if (totalReg) {
        new Chart(totalReg, {
            type: 'line',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
                datasets: [{
                    label: '註冊量',
                    data: [2000, 18000, 11000, 23000, 17500],
                    borderColor: '#1cc88a',
                    fill: false
                }]
            }
        });
    }

    const totalHouse = document.getElementById('Total_Listings_Houses');
    if (totalHouse) {
        new Chart(totalHouse, {
            type: 'line',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
                datasets: [{
                    label: '周上架房源量',
                    data: [64, 50, 78, 60, 68],
                    borderColor: '#1cc88a',
                    fill: false
                }]
            }
        });
    }

    const totalFurniture = document.getElementById('Total_Shelves_Furniture');
    if (totalFurniture) {
        new Chart(totalFurniture, {
            type: 'line',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
                datasets: [{
                    label: '周出租家具量',
                    data: [136, 142, 90, 182, 140],
                    borderColor: '#1cc88a',
                    fill: false
                }]
            }
        });
    }
})();
