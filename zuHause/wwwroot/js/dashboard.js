// ====== 角色權限對應功能頁籤 ======
//const roleAccess = {
//    '超級管理員': ['overview', 'monitor', 'behavior', 'orders', 'system', 'roles', 'Backend_user_list', 'contract_template', 'platform_fee', "imgup", 'furniture_fee', 'Marquee_edit', 'furniture_management'],
//    '管理員': ['overview', 'behavior', 'orders'],
//    '房源審核員': ['monitor'],
//    '客服': ['behavior', 'orders']
//};

// ====== 頁籤名稱對應表 ======
const tabNames = {
    overview: "📊 平台整體概況",
    monitor: "🧭 商品與房源監控",
    behavior: "👣 用戶行為監控",
    orders: "💳 訂單與金流",
    system: "🛠️ 系統通知與健康",
    roles: "🛡️ 身分權限列表",
    Backend_user_list: "👨‍💻 後臺使用者",
    contract_template: "📄 合約範本管理",
    platform_fee: "💰 平台收費設定",
    imgup: "🖼️ 輪播圖片管理",
    furniture_fee: "📦 家具配送費",
    Marquee_edit: "🌀 跑馬燈管理",
    furniture_management: "🛋️ 家具列表管理"
};

// ====== 分組設定 ======
const tabGroups = {
    Dashboard: {
        title: "📊 儀表板",
        keys: ['overview', 'monitor', 'behavior', 'orders', 'system']
    },
    Permission: {
        title: "🛡️ 權限管理",
        keys: ['roles', 'Backend_user_list']
    },
    Template: {
        title: "📂 模板管理",
        keys: ['contract_template']
    },
    Imgandtext: {
        title: "📂 平台圖片與文字資料管理",
        keys: ['imgup', 'Marquee_edit', 'furniture_management']
    },
    Fee: {
        title: "📁 平台費用設定",
        keys: ['platform_fee', 'furniture_fee']
    }
};

// ====== 初始化畫面 ======
window.onload = () => {
    initSidebar();
    const role = currentUserRole;
    const firstTab = roleAccess[role]?.[0];
    if (firstTab) openTab(firstTab);
};

// ====== 左側選單生成 ======
function initSidebar() {
    document.getElementById("roleDisplay").innerText = currentUserRole;
    document.getElementById("EmployeeID").innerText = EmployeeID;

    const menu = document.getElementById("menuButtons");
    menu.innerHTML = "";

    for (const groupKey in tabGroups) {
        const { title, keys } = tabGroups[groupKey];
        const groupWrapper = document.createElement("div");
        groupWrapper.className = "mb-3";

        const groupTitle = document.createElement("div");
        groupTitle.className = "fw-bold ps-2 mb-2";
        groupTitle.textContent = title;
        groupWrapper.appendChild(groupTitle);

        keys.forEach(key => {
            if (!roleAccess[currentUserRole]?.includes(key)) return;
            const btn = document.createElement("button");
            btn.className = "btn btn-outline-secondary w-100 my-1 text-start";
            btn.textContent = tabNames[key];
            btn.onclick = () => openTab(key);
            groupWrapper.appendChild(btn);
        });

        if (groupWrapper.children.length > 1) {
            menu.appendChild(groupWrapper);
        }
    }
}

// ====== 開啟分頁 ======
function openTab(tabKey) {
    const tabId = `tab-${tabKey}`;
    const tabExists = document.getElementById(tabId);

    // 移除目前所有 active
    document.querySelectorAll("#tabHeader .nav-link").forEach(btn => btn.classList.remove("active"));
    document.querySelectorAll("#tabContent .tab-pane").forEach(pane => pane.classList.remove("show", "active"));

    if (tabExists) {
        switchTab(tabId);
        return;
    }

    // 建立 tab header
    const tabHeader = document.createElement("li");
    tabHeader.className = "nav-item";
    tabHeader.id = tabId;

    tabHeader.innerHTML = `
        <button class="nav-link active d-flex justify-content-between align-items-center" data-tab="${tabId}">
            ${tabNames[tabKey]}
            <span class="ms-2 text-danger fw-bold close-tab" style="cursor:pointer;">×</span>
        </button>
    `;
    document.getElementById("tabHeader").appendChild(tabHeader);
    // 綁定點擊 header tab 本身會切換
    tabHeader.querySelector(".nav-link").onclick = (e) => {
        // 避免點到關閉符號時觸發切換
        if (e.target.classList.contains("close-tab")) return;
        switchTab(tabId);
    };
    // 建立 tab content
    const tabContent = document.createElement("div");
    tabContent.className = "tab-pane fade show active";
    tabContent.id = `${tabId}-content`;
    tabContent.setAttribute("role", "tabpanel");
    tabContent.innerHTML = `<div>🔄 正在載入 ${tabNames[tabKey]}...</div>`;
    document.getElementById("tabContent").appendChild(tabContent);

    // AJAX 載入內容
    fetch(`/Dashboard/${tabKey}`)
        .then(r => r.text())
        .then(html => {
            tabContent.innerHTML = html;

            const scriptMap = {
                overview: '/js/overview.js?v=1',
                monitor: '/js/monitor.js?v=1',
                behavior: '/js/behavior.js?v=1',
                orders: '/js/orders.js?v=1',
                system: '/js/system.js?v=1',
                roles: '/js/roles.js?v=1',
                Backend_user_list: '/js/Backend_user_list.js?v=1',
                contract_template: '/js/contract_template.js?v=1',
                platform_fee: '/js/platform_fee.js?v=1',
                imgup: '/js/imgup.js?v=1',
                furniture_fee: '/js/furniture_fee.js?v=1',
                Marquee_edit: '/js/Marquee_edit.js?v=1',
                furniture_management: '/js/furniture_management.js?v=1'
            };

            if (scriptMap[tabKey]) {
                const script = document.createElement('script');
                script.src = scriptMap[tabKey];

                script.onload = () => {
                    if (tabKey === "roles" && typeof updateRoleListWithPermissions === "function") {
                        updateRoleListWithPermissions();
                    }
                     if (tabKey === "Backend_user_list" && typeof renderUserTable === "function") {
                        renderUserTable();
                    }
                    if (tabKey === "contract_template") {
                        if (typeof renderTemplateList === "function") renderTemplateList();
                        if (typeof bindContractUploadEvents === "function") bindContractUploadEvents(); // 👈 這要新增
                    }
                    if (tabKey === "furniture_management") {
            // 你也可以判斷是否要初始化資料或執行 resetForm
            if (typeof resetForm === "function") resetForm();
        }
                };

                document.body.appendChild(script);
            }
        });


    // 關閉 tab 的邏輯
    tabHeader.querySelector(".close-tab").onclick = () => closeTab(tabId);
}

// ====== 切換 tab 顯示區 ======
function switchTab(tabId) {
    // 清除 active
    document.querySelectorAll("#tabHeader .nav-link").forEach(btn => btn.classList.remove("active"));
    document.querySelectorAll("#tabContent .tab-pane").forEach(pane => pane.classList.remove("show", "active"));

    // 加入 active
    const targetBtn = document.querySelector(`#${tabId} .nav-link`);
    const targetPane = document.getElementById(`${tabId}-content`);
    if (targetBtn) targetBtn.classList.add("active");
    if (targetPane) targetPane.classList.add("show", "active");
}

// ====== 關閉分頁 ======
function closeTab(tabId) {
    const header = document.getElementById(tabId);
    const content = document.getElementById(`${tabId}-content`);
    const wasActive = header.querySelector(".nav-link").classList.contains("active");

    header?.remove();
    content?.remove();

    // 若關閉的是當前分頁 → 切到最後一個分頁
    if (wasActive) {
        const tabs = document.querySelectorAll("#tabHeader .nav-link");
        if (tabs.length > 0) {
            const lastTabId = tabs[tabs.length - 1].closest("li").id;
            switchTab(lastTabId);
        }
    }
}
    