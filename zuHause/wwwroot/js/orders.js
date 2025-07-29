     
(() => {
    let chartPayInstance = null;
   
    let chartMonthlyPayInstance = null;

    function showLoading(id, show) {
        document.getElementById(id).style.display = show ? '' : 'none';
    }

    async function loadListingFeeStats(year, month) {
        showLoading('chartPayLoading', true);
        try {
            const res = await fetch(`/Dashboard/listing-fee-stats?year=${year}&month=${month}`);
            const data = await res.json();

            // 更新金額
            document.getElementById("totalDue").textContent = "NT$ " + data.totalDue.toLocaleString();
            document.getElementById("totalPaid").textContent = "NT$ " + data.totalPaid.toLocaleString();
            document.getElementById("totalUnpaid").textContent = "NT$ " + data.totalUnpaid.toLocaleString(); // 新增這行

            // 圓餅圖
            const chartPay = document.getElementById("chartPay");
            if (chartPay) {
                if (!chartPayInstance) {
                    chartPayInstance = new Chart(chartPay, {
                        type: "doughnut",
                        data: {
                            labels: ["上架服務費實收款", "上架服務費未收款"],
                            datasets: [{
                                data: [data.totalPaid, data.totalDue - data.totalPaid],
                                backgroundColor: ["#1cc88a", "#e74a3b"]
                            }]
                        },
                        options: {
                            plugins: {
                                title: { display: true, text: "實收款與未收款占比分析" }
                            }
                        }
                    });
                } else {
                    chartPayInstance.data.datasets[0].data = [data.totalPaid, data.totalDue - data.totalPaid];
                    chartPayInstance.update();
                }
            }

            // 折線圖（每日收入趨勢）
            const chartTrend = document.getElementById("chartTrend");
            if (chartTrend && data.trend) {
                const labels = data.trend.map(x => x.date);
                const paid = data.trend.map(x => x.paid);
                const due = data.trend.map(x => x.due);

                if (!chartTrendInstance) {
                    chartTrendInstance = new Chart(chartTrend, {
                        type: 'line',
                        data: {
                            labels,
                            datasets: [
                                { label: "實收金額", data: paid, borderColor: "#1cc88a", fill: false },
                                { label: "應收金額", data: due, borderColor: "#f6c23e", fill: false }
                            ]
                        },
                        options: {
                            plugins: { title: { display: true, text: "近五日上架費收入走勢圖" } }
                        }
                    });
                } else {
                    chartTrendInstance.data.labels = labels;
                    chartTrendInstance.data.datasets[0].data = paid;
                    chartTrendInstance.data.datasets[1].data = due;
                    chartTrendInstance.update();
                }
            }
        } catch (err) {
            document.getElementById("totalDue").textContent = "-";
            document.getElementById("totalPaid").textContent = "-";
            document.getElementById("totalUnpaid").textContent = "-";
            console.error("上架費載入錯誤", err);
        } finally {
            showLoading('chartPayLoading', false);
        }
    }
    async function renderPropertyStatusStats(year, month) {
        showLoading('propertyStatusStatsLoading', true);
        try {
            const res = await fetch(`/Dashboard/property-status-stats?year=${year}&month=${month}`);
            if (!res.ok) throw new Error("載入失敗");

            const stats = await res.json();
            const container = document.getElementById("propertyStatusStatsTable");

            let html = `<table class="table table-bordered text-center">
                      <thead><tr><th>類型</th><th>數量</th><th>備註</th></tr></thead><tbody>`;

            for (const row of stats) {
                html += `<tr><td>${row.type}</td><td>${row.count}</td><td>${row.note}</td></tr>`;
            }

            html += "</tbody></table>";
            container.innerHTML = html;
        } catch (err) {
            document.getElementById("propertyStatusStatsTable").innerHTML = "<p class='text-danger'>載入失敗</p>";
            console.error("🏠 房源狀態統計載入失敗", err);
        } finally {
            showLoading('propertyStatusStatsLoading', false);
        }
    }

    function initYearMonthSelector() {
        const now = new Date();
        const yearSel = document.getElementById('selectYear');
        const monthSel = document.getElementById('selectMonth');
        if (!yearSel || !monthSel) return;
        const thisYear = now.getFullYear();
        for (let y = thisYear - 5; y <= thisYear + 1; y++) {
            yearSel.innerHTML += `<option value="${y}"${y === thisYear ? ' selected' : ''}>${y}年</option>`;
        }
        for (let m = 1; m <= 12; m++) {
            monthSel.innerHTML += `<option value="${m}"${m === now.getMonth() + 1 ? ' selected' : ''}>${m}月</option>`;
        }
        yearSel.addEventListener('change', reloadAllStats);
        monthSel.addEventListener('change', reloadAllStats);
    }
    async function loadMonthlyListingFeeTrend(year, month) {
        showLoading('chartMonthlyPayLoading', true);
        try {
            const url = `/Dashboard/monthly-trend?year=${year}&month=${month}`;
            const res = await fetch(url);
            
            if (!res.ok) throw new Error("載入失敗：" + res.statusText);
            const data = await res.json();

            const labels = data.map(x => x.date);
            const due = data.map(x => x.totalDue);
            const paid = data.map(x => x.totalPaid);

            const ctx = document.getElementById('chartMonthlyPay');
            if (!ctx) return;

            if (!chartMonthlyPayInstance) {
                chartMonthlyPayInstance = new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: labels,
                        datasets: [
                            {
                                label: "應收",
                                data: due,
                                borderColor: "#e74a3b",
                                fill: false
                            },
                            {
                                label: "實收",
                                data: paid,
                                borderColor: "#1cc88a",
                                fill: false
                            }
                        ]
                    },
                    options: {
                        plugins: {
                            title: {
                                display: true,
                                text: `${year}年${month}月每日上架費收款走勢`
                            }
                        }
                    }
                });
            } else {
                chartMonthlyPayInstance.data.labels = labels;
                chartMonthlyPayInstance.data.datasets[0].data = due;
                chartMonthlyPayInstance.data.datasets[1].data = paid;
                chartMonthlyPayInstance.options.plugins.title.text = `${year}年${month}月每日上架費收款走勢`;
                chartMonthlyPayInstance.update();
            }
        } catch (err) {
            console.error("📉 本月收款走勢載入失敗", err);
        } finally {
            showLoading('chartMonthlyPayLoading', false);
        }
    }
    function reloadMonthlyTrend() {
        const year = document.getElementById('selectYear').value;
        const month = document.getElementById('selectMonth').value;
        loadMonthlyListingFeeTrend(year, month);
    }

    // 📋 訂單統計（靜態）
    async function renderOrderStats(year, month) {
        showLoading('orderStatsLoading', true);
        try {
            const res = await fetch(`/Dashboard/order-stats?year=${year}&month=${month}`);
            if (!res.ok) throw new Error("載入失敗");

            const orderStats = await res.json();
            const container = document.getElementById("orderStatsTable");

            let html = `<table class="table table-bordered text-center">
                        <thead><tr><th>類型</th><th>數量</th><th>備註</th></tr></thead><tbody>`;

            for (const row of orderStats) {
                html += `<tr><td>${row.type}</td><td>${row.count}</td><td>${row.note}</td></tr>`;
            }

            html += "</tbody></table>";
            container.innerHTML = html;
        } catch (err) {
            document.getElementById("orderStatsTable").innerHTML = "<p class='text-danger'>載入失敗</p>";
            console.error("📋 訂單統計載入失敗", err);
        } finally {
            showLoading('orderStatsLoading', false);
        }
    }

   
    function reloadAllStats() {
        const year = document.getElementById('selectYear').value;
        const month = document.getElementById('selectMonth').value;
        loadListingFeeStats(year, month);
        loadMonthlyListingFeeTrend(year, month);
        renderOrderStats(year, month);
        renderPropertyStatusStats(year, month);
        // 更新表格標題
        document.getElementById('orderStatsTitle').textContent = `${year}年${month}月房源概況`;
        document.getElementById('chartMonthlyPayTitle').textContent = `${year}年${month}月每日上架費收款走勢`;
    }

    // 初始化
    
    initYearMonthSelector();
    const now = new Date();
    reloadAllStats();
    
    })();