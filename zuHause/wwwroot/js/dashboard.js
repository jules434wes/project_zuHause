const tabNames = {
    overview: "📊 平台整體概況",
    monitor: "🧭 商品與出租監控",
    behavior: "👣 用戶行為監控",
    orders: "💳 房源與金流",
    system: "🛠️ 系統通知與健康",
    roles: "🛡️ 身分權限列表",
    Backend_user_list: "👨‍💻 後臺使用者",
    //member_management: "👥 前台會員管理",
    contract_template: "📄 合約範本管理",
    platform_fee: "💰 平台收費設定",
    imgup: "🖼️ 輪播圖片管理",
    furniture_fee: "📦 家具配送費",
    Marquee_edit: "🌀 跑馬燈管理",
    furniture_management: "🛋️ 家具列表管理",
    announcement_management: "📢 公告管理",
    message_template_management: "📝 後台訊息模板管理",
    
    // Admin 功能
    member_list: "👤 會員列表與驗證",
    landlord_list: "🏘️ 房東列表",
    property_list: "🏠 房源列表", 
    property_complaint_list: "⚠️ 房源投訴列表",
    customer_service_list: "🎧 客服處理",
    system_message_list: "📨 系統訊息"
};

// ====== 分組設定 ======
// 注意：分組順序會影響左側選單的顯示順序
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
    UserManagement: {
        title: "👥 用戶管理",
        keys: ['member_list', 'landlord_list']
    },
    PropertyManagement: {
        title: "🏠 房源管理",
        keys: ['property_list', 'property_complaint_list']
    },
    CustomerService: {
        title: "🎧 客戶服務",
        keys: ['customer_service_list', 'system_message_list']
    },
    Imgandtext: {
        title: "📂 平台圖片與文字資料管理",
        keys: ['imgup', 'Marquee_edit', 'furniture_management', 'announcement_management', 'message_template_management']
    },
    Fee: {
        title: "📁 平台費用設定",
        keys: ['platform_fee', 'furniture_fee']
    }
};

// ====== 初始化畫面 ======
// 管理員權限控制 - 頁面載入時的權限檢查與首頁決定邏輯
window.onload = () => {
    // 初始化左側選單（根據權限顯示功能按鈕）
    initSidebar();
    
    // === 權限資料解析 ===
    // currentUserRole: 從後端 ViewBag.Role 傳入的當前管理員角色名稱
    // roleAccess: 從後端 ViewBag.RoleAccess 序列化而來的權限物件
    //   格式1 (超級管理員): { "超級管理員": { "all": true } }
    //   格式2 (一般管理員): { "系統管理員": ["overview", "monitor", "roles"] }
    const role = currentUserRole;
    const permissions = roleAccess[role];

    let firstTab = null;

    // === 決定預設開啟的第一個頁籤 ===
    // 修改原因：避免登入後自動開啟 Admin 功能的新分頁
    // 原本邏輯會直接選擇第一個權限，但如果是 Admin 功能會用 window.open() 開新分頁
    // 造成同時顯示 Dashboard 和 Admin 頁面的問題
    if (permissions?.all === true) {
        // 超級管理員：有全部權限，開啟第一個 Dashboard 內嵌功能
        const allKeys = Object.keys(tabNames);
        const adminTabs = [
            'member_list', 'landlord_list', 'property_list', 
            'property_complaint_list', 'customer_service_list', 'system_message_list'
        ];
        firstTab = allKeys.find(key => !adminTabs.includes(key)) || allKeys[0];
    } else if (Array.isArray(permissions)) {
        // 一般管理員：優先選擇 Dashboard 內嵌功能，避免自動開啟 Admin 新分頁
        const adminTabs = [
            'member_list', 'landlord_list', 'property_list', 
            'property_complaint_list', 'customer_service_list', 'system_message_list'
        ];
        const dashboardPermissions = permissions.filter(p => !adminTabs.includes(p));
        firstTab = dashboardPermissions.length > 0 ? dashboardPermissions[0] : null;
    }
    
    // 開啟預設頁籤
    if (firstTab) openTab(firstTab);
};


// ====== 左側選單生成 ======
// 管理員權限控制 - 根據權限動態生成左側功能選單
function initSidebar() {
    // 顯示當前管理員資訊
    document.getElementById("roleDisplay").innerText = currentUserRole;
    document.getElementById("EmployeeID").innerText = EmployeeID;

    // 清空選單容器
    const menu = document.getElementById("menuButtons");
    menu.innerHTML = "";

    // === 權限檢查邏輯 ===
    // 從 roleAccess 物件中取得當前管理員的權限資訊
    const rolePermission = roleAccess[currentUserRole] || {};
    const isAllAccess = rolePermission.all === true; // 檢查是否為超級管理員

    // === 遍歷所有功能分組並建立選單 ===
    for (const groupKey in tabGroups) {
        const { title, keys } = tabGroups[groupKey];
        
        // 建立分組容器
        const groupWrapper = document.createElement("div");
        groupWrapper.className = "mb-3";

        // 建立分組標題
        const groupTitle = document.createElement("div");
        groupTitle.className = "fw-bold ps-2 mb-2";
        groupTitle.textContent = title;
        groupWrapper.appendChild(groupTitle);

        // === 權限控制的核心邏輯 ===
        // 遍歷分組中的每個功能，根據權限決定是否顯示
        keys.forEach(key => {
            // 權限檢查：
            // 1. 如果是超級管理員 (isAllAccess = true)：顯示所有功能
            // 2. 如果是一般管理員：檢查 rolePermission 陣列中是否包含此功能鍵值
            if (!isAllAccess && !rolePermission.includes?.(key)) return;
            
            // 建立功能按鈕
            const btn = document.createElement("button");
            btn.className = "btn btn-outline-secondary w-100 my-1 text-start";
            btn.textContent = tabNames[key];
            btn.onclick = () => openTab(key);
            groupWrapper.appendChild(btn);
        });

        // 只有包含功能按鈕的分組才會顯示（至少要有標題 + 一個按鈕）
        if (groupWrapper.children.length > 1) {
            menu.appendChild(groupWrapper);
        }
    }
}


// ====== 開啟分頁 ======
// 功能頁籤開啟邏輯 - 區分 Dashboard 內嵌功能與 Admin 獨立頁面
function openTab(tabKey) {
    // === Admin 功能：開新分頁處理 ===
    // 定義需要開新分頁的 admin 功能清單
    // 這些功能使用獨立的 AdminController，不在 Dashboard 內嵌顯示
    const adminTabs = [
        'member_list',              // 會員列表與驗證
        'landlord_list',            // 房東列表
        'property_list',            // 房源列表
        'property_complaint_list',  // 房源投訴列表
        'customer_service_list',    // 客服處理
        'system_message_list'       // 系統訊息
    ];
    
    // 如果點擊的是 admin 功能，用新分頁開啟對應的 Admin 路由
    if (adminTabs.includes(tabKey)) {
        const adminUrls = {
            member_list: '/Admin/admin_usersList',
            landlord_list: '/Admin/admin_landlordList',
            property_list: '/Admin/admin_propertiesList',
            property_complaint_list: '/Admin/admin_propertyComplaints',
            customer_service_list: '/Admin/admin_customerServiceList',
            system_message_list: '/Admin/admin_systemMessageList'
        };
        
        // 開新分頁並結束函數執行
        window.open(adminUrls[tabKey], '_blank');
        return;
    }

    // === Dashboard 內嵌功能：AJAX 載入處理 ===
    // 以下處理 Dashboard 內部的功能頁籤（如 overview, monitor 等）

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
            const timestamp = new Date().getTime();
            const scriptMap = {
                overview: `/js/overview.js?v=${timestamp}`,
                monitor: `/js/monitor.js?v=${timestamp}`,
                behavior: `/js/behavior.js?v=${timestamp}`,
                orders: `/js/orders.js?v=${timestamp}`,
                system: `/js/system.js?v=${timestamp}`,
                roles: `/js/roles.js?v=${timestamp}`,
                Backend_user_list: `/js/Backend_user_list.js?v=${timestamp}`,
                //member_management: `/js/member_management.js?v=${timestamp}`,
                contract_template: `/js/contract_template.js?v=${timestamp}`,
                platform_fee: `/js/platform_fee.js?v=${timestamp}`,
                imgup: `/js/imgup.js?v=${timestamp}`,
                furniture_fee: `/js/furniture_fee.js?v=${timestamp}`,
                Marquee_edit: `/js/Marquee_edit.js?v=${timestamp}`,
                furniture_management: `/js/furniture_management.js?v=${timestamp}`,
                announcement_management: `/js/announcement_management.js?v=${timestamp}`,
                message_template_management: `/js/message_template_management.js?v=${timestamp}`
            };

            if (scriptMap[tabKey]) {
                const script = document.createElement('script');
                script.src = scriptMap[tabKey];

                script.onload = () => {
                    if (tabKey === "monitor") {
                        if (typeof loadHotRankings === "function") {
                            loadHotRankings();
                        }
                    }

                    if (tabKey === "system") {
                        if (typeof loadServerStatusChart === "function") {
                            loadServerStatusChart(); // ✅ 初始化並自動更新
                        }
                    }

                  
                    if (tabKey === "overview") {
                        if (typeof initOverviewDashboard === "function") {
                            initOverviewDashboard(); // ✅ 呼叫看板初始化函式
                        }
                    }

                    if (tabKey === "roles" && typeof updateRoleListWithPermissions === "function") {
                        updateRoleListWithPermissions();
                    }
                    if (tabKey === "Backend_user_list" && typeof renderUserTable === "function") {
                        if (typeof loadRoleOptions === "function") loadRoleOptions();
                        if (typeof renderUserTable === "function") renderUserTable();
                    }
                    //if (tabKey === "member_management" && typeof initMemberManagement === "function") {
                    //    initMemberManagement();
                    //}
                    if (tabKey === "contract_template") {
                        if (typeof renderTemplateList === "function") renderTemplateList();
                        if (typeof bindContractUploadEvents === "function") bindContractUploadEvents(); // 👈 這要新增
                    }
                    if (tabKey === "furniture_management"){
                    // 你也可以判斷是否要初始化資料或執行 resetForm
                        if (typeof resetForm === "function") resetForm();
                    }
                    if (tabKey === "platform_fee") {
                        if (typeof onload === "function") onload();
                        //if (typeof initPlatformFee === "function") initPlatformFee();
                        if (typeof renderListingPlans === "function") renderListingPlans();
                        
                      
                    }
                    if (tabKey === "furniture_fee") {
                        if (typeof onload === "function") onload();
                        
                    }
                   
                    if (tabKey === "furniture_management") {
                       
                        if (typeof resetForm === "function") resetForm();
                       
                        if (typeof loadAllInventoryEvents === "function")loadAllInventoryEvents();
                        

                        if (typeof submitFurniture === "function") {
                            const btn = document.getElementById("submitBtn");
                            if (btn) {
                                btn.addEventListener("click", submitFurniture);
                            }
                        }
                       
                        // 🔧 綁定「提前下架」按鈕
                        if (typeof setProductOffline === "function") {
                            const offlineBtns = document.querySelectorAll(".btn-warning");
                            offlineBtns.forEach(btn => {
                                const id = btn.getAttribute("onclick")?.match(/'(.+)'/)?.[1];
                                if (id) {
                                    btn.addEventListener("click", () => setProductOffline(id));
                                    // 🔄 清掉原本的 onclick
                                    btn.removeAttribute("onclick");
                                }
                            });
                        }
                    }

                    if (tabKey === "imgup") {
                        if (typeof initCarouselManager === "function") {
                            initCarouselManager(); // ✅ 執行初始化
                        }
                        
                       
                    }
                    if (tabKey === "Marquee_edit") {
                        if (typeof initMarqueeManager === "function") {
                            initMarqueeManager();
                        }
                    }
                    if (tabKey === "announcement_management") {
                        if (typeof initAnnouncementManager === "function") {
                            initAnnouncementManager();
                        }
                    }
                    if (tabKey === "message_template_management") {
                        if (typeof initMessageTemplateManager === "function") {
                            initMessageTemplateManager();
                        }
                    }

                    //onloadx內
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
// 引入 bootstrap toast 物件控制
function showToast(message, type = 'success') {
    const toastEl = document.getElementById('customToast');
    const toastBody = document.getElementById('toastBody');

    toastBody.innerText = message;

    // 根據 type 設定背景色
    toastEl.className = 'toast align-items-center text-white border-0'; // 清空 class
    if (type === 'success') toastEl.classList.add('bg-success');
    else if (type === 'error') toastEl.classList.add('bg-danger');
    else if (type === 'info') toastEl.classList.add('bg-info');
    else toastEl.classList.add('bg-secondary');

    // 顯示 Toast
    const toast = new bootstrap.Toast(toastEl);
    toast.show();
}