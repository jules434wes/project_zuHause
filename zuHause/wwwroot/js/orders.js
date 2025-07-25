     
(() => {
    // 📊 載入統計資料
    async function loadListingFeeStats() {
        try {
            const res = await fetch("/Dashboard/listing-fee-stats");
            const data = await res.json();

            // 更新金額
            document.getElementById("totalDue").textContent = "NT$ " + data.totalDue.toLocaleString();
            document.getElementById("totalPaid").textContent = "NT$ " + data.totalPaid.toLocaleString();

            // 圓餅圖
            const chartPay = document.getElementById("chartPay");
            if (chartPay) {
                new Chart(chartPay, {
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
            }

            // 折線圖（每日收入趨勢）
            const chartTrend = document.getElementById("chartTrend");
            if (chartTrend && data.trend) {
                const labels = data.trend.map(x => x.date);
                const paid = data.trend.map(x => x.paid);
                const due = data.trend.map(x => x.due);

                new Chart(chartTrend, {
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
            }
        } catch (err) {
            console.error("上架費載入錯誤", err);
        }
    }
    async function loadMonthlyListingFeeTrend() {
        try {
            const res = await fetch("/Dashboard/monthly-trend");
            
            if (!res.ok) throw new Error("載入失敗：" + res.statusText);
            const data = await res.json();

            const labels = data.map(x => x.date);
            const due = data.map(x => x.totalDue);
            const paid = data.map(x => x.totalPaid);

            const ctx = document.getElementById('chartMonthlyPay');
            if (!ctx) return;

            new Chart(ctx, {
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
                            text: '本月每日上架費收款走勢'
                        }
                    }
                }
            });
        } catch (err) {
            console.error("📉 本月收款走勢載入失敗", err);
        }
    }

    // 📋 訂單統計（靜態）
    async function renderOrderStats() {
        try {
            const res = await fetch("/Dashboard/order-stats");
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
            console.error("📋 訂單統計載入失敗", err);
            document.getElementById("orderStatsTable").innerHTML = "<p class='text-danger'>載入失敗</p>";
        }
    }

    // 初始化
    
    loadListingFeeStats();
    loadMonthlyListingFeeTrend();
    renderOrderStats();
    })();