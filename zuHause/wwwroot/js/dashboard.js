window.addEventListener("DOMContentLoaded", () => {
    if (document.getElementById("tab-overview")) {
        drawOverviewCharts();
    }
});

function drawOverviewCharts() {
    // 📈 本週註冊
    const ctx1 = document.getElementById("total_registration");
    if (ctx1) {
        new Chart(ctx1, {
            type: "bar",
            data: {
                labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
                datasets: [{
                    label: "註冊人數",
                    data: [120, 180, 90, 160, 200, 140, 170],
                    backgroundColor: "#4e73df"
                }]
            }
        });
    }

    // 🏠 本週上架房源
    const ctx2 = document.getElementById("Total_Listings_Houses");
    if (ctx2) {
        new Chart(ctx2, {
            type: "line",
            data: {
                labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
                datasets: [{
                    label: "上架房源",
                    data: [32, 45, 28, 60, 70, 52, 33],
                    borderColor: "#1cc88a",
                    fill: false
                }]
            }
        });
    }

    // 🛋️ 本週出租家具
    const ctx3 = document.getElementById("Total_Shelves_Furniture");
    if (ctx3) {
        new Chart(ctx3, {
            type: "line",
            data: {
                labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
                datasets: [{
                    label: "出租家具",
                    data: [50, 65, 40, 80, 90, 85, 70],
                    borderColor: "#36b9cc",
                    fill: false
                }]
            }
        });
    }

    // 📊 訂單數量分布
    const ctx4 = document.getElementById("chartOrders");
    if (ctx4) {
        new Chart(ctx4, {
            type: "doughnut",
            data: {
                labels: ["完成", "未付款", "退租申請"],
                datasets: [{
                    data: [52, 8, 3],
                    backgroundColor: ["#1cc88a", "#f6c23e", "#e74a3b"]
                }]
            }
        });
    }

    // 💰 每日營收
    const ctx5 = document.getElementById("chartRevenue");
    if (ctx5) {
        new Chart(ctx5, {
            type: "bar",
            data: {
                labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
                datasets: [{
                    label: "每日營收 (NT$)",
                    data: [5000, 7000, 4000, 9000, 11000, 9500, 6000],
                    backgroundColor: "#f6c23e"
                }]
            }
        });
    }
}
const tabs = [];

function openTab(tabId, tabTitle, url) {
    const existing = tabs.find(t => t.id === tabId);
    if (existing) {
        selectTab(tabId);
        return;
    }

    const tabContainer = document.getElementById("tabContainer");

    const tabItem = document.createElement("div");
    tabItem.className = "tab-item bg-light px-3 py-1 rounded d-flex align-items-center";
    tabItem.setAttribute("data-id", tabId);
    tabItem.innerHTML = `
        <span class="me-2">${tabTitle}</span>
        <button class="btn-close btn-sm" onclick="closeTab('${tabId}')"></button>
    `;
    tabItem.onclick = () => selectTab(tabId);

    tabContainer.appendChild(tabItem);

    tabs.push({ id: tabId, url, title: tabTitle });
    selectTab(tabId);
}

function selectTab(tabId) {
    document.querySelectorAll(".tab-item").forEach(el => {
        el.classList.toggle("bg-secondary", el.getAttribute("data-id") === tabId);
        el.classList.toggle("text-white", el.getAttribute("data-id") === tabId);
    });

    const tab = tabs.find(t => t.id === tabId);
    if (tab) {
        fetch(tab.url)
            .then(res => res.text())
            .then(html => {
                document.querySelector(".col-md-10").innerHTML = html;
            });
    }
}

function closeTab(tabId) {
    const index = tabs.findIndex(t => t.id === tabId);
    if (index > -1) {
        tabs.splice(index, 1);
        const tabEl = document.querySelector(`.tab-item[data-id="${tabId}"]`);
        if (tabEl) tabEl.remove();

        // 自動切換到前一個
        if (tabs.length > 0) {
            const nextTab = tabs[tabs.length - 1];
            selectTab(nextTab.id);
        } else {
            document.querySelector(".col-md-10").innerHTML = "<p>請從左邊選單開啟功能</p>";
        }
    }
}
