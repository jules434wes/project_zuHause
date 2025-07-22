(() => {
    let listingPlans = []; // 儲存後端回傳的資料
    function formatDateTime(raw) {
        const dt = new Date(raw);
        return dt.toLocaleString('zh-TW', {
            year: 'numeric', month: '2-digit', day: '2-digit',
            hour: '2-digit', minute: '2-digit', hour12: true
        });
    }
    async function renderListingPlans() {
        console.log("📥 從資料庫載入上架方案...");
        const tbody = document.querySelector('#listingPlansTable tbody');
        tbody.innerHTML = '載入中...';

        try {
            const res = await fetch('/Dashboard/GetAllListingPlans');
            if (!res.ok) throw new Error('❌ 無法取得方案清單');

            listingPlans = await res.json();

            tbody.innerHTML = ''; // 清空原本的載入文字
            listingPlans.forEach(plan => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                <td>${plan.planId}</td>
                <td>${plan.planName}</td>
                <td>NT$${plan.pricePerDay}</td>
                <td>${plan.minListingDays}</td>
                <td>${formatDateTime(plan.startAt)} ~ ${formatDateTime(plan.endAt) ?? '無期限'}</td>
                <td>${plan.isActive ? '✅ 啟用中' : '❌ 已停用'}</td>
                <td><button class="btn btn-sm btn-outline-primary" onclick="editPlan(${plan.planId})">✏️ 編輯</button></td>
            `;
                tbody.appendChild(tr);
            });

        } catch (err) {
            console.error(err);
            tbody.innerHTML = `<tr><td colspan="7" class="text-danger text-center">${err.message}</td></tr>`;
        }
    }
    //方案上傳
    window.submitListingPlan = submitListingPlan;
    async function submitListingPlan() {
        const plan = {
            planName: document.getElementById('planName').value.trim(),
            pricePerDay: parseFloat(document.getElementById('pricePerDay').value),
            minListingDays: parseInt(document.getElementById('minListingDays').value),
            currencyCode: document.getElementById('currencyCode').value.trim(),
            startAt: document.getElementById('startAt').value,
            endAt: document.getElementById('endAt').value || null
        };

        if (!plan.planName || isNaN(plan.pricePerDay) || isNaN(plan.minListingDays)) {
            showToast("❌ 資料不完整或格式錯誤");
            return;
        }

        // 如果是編輯模式，加入 ID
        if (editingPlanId !== null) {
            plan.planId = editingPlanId;
        }

        try {
            const res = await fetch(editingPlanId === null
                ? '/Dashboard/CreateListingPlan'
                : '/Dashboard/UpdateListingPlan', // 你需要這個 API
                {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(plan)
                });

            let result;
            if (!res.ok) {
                const errText = await res.text(); // 用 text() 拿原始錯誤文字
                throw new Error(errText || '❌ 儲存失敗');
            }
            result = await res.json();

            if (!result.success) throw new Error(result.message || '❌ 儲存失敗');


            showToast(editingPlanId === null ? "✅ 新增成功！" : "✅ 編輯完成！");
            renderListingPlans();
            clearListingPlanForm();

        } catch (err) {
            showToast(err.message, false); // ❌ 錯誤 → 紅色吐司
        }
    }
    
    

    
    
    
    
    
  
    //平台收費方案初始化
    function initPlatformFee() {
        const ids = ['daysInput', 'listingFee'];
        ids.forEach(id => {
            const el = document.getElementById(id);
            if (el) el.oninput = calculateTotal;
        });

       
    }
    //編輯功能
    let editingPlanId = null; // 記錄正在編輯的方案 ID
    // 編輯方案按鈕點擊事件

    window.editPlan = editPlan;
    function editPlan(planId) {
        const plan = listingPlans.find(p => p.planId === planId);
        if (!plan) return alert("❌ 找不到要編輯的方案");

        editingPlanId = planId;
        document.getElementById("cancelEditBtn").classList.remove("d-none");
        // 帶入表單
        document.getElementById('planName').value = plan.planName;
        document.getElementById('pricePerDay').value = plan.pricePerDay;
        document.getElementById('minListingDays').value = plan.minListingDays;
        document.getElementById('currencyCode').value = plan.currencyCode;
        document.getElementById('startAt').value = plan.startAt?.substring(0, 16);
        document.getElementById('endAt').value = plan.endAt ? plan.endAt.substring(0, 16) : '';

        // 更新標題 & 按鈕樣式
        const card = document.querySelector('.card.mt-4'); // ✅ 專門選取下面新增表單那塊
        const header = card.querySelector('.card-header');
        const button = card.querySelector('.card-body button');

        // 修改樣式
        header.classList.remove('bg-success');
        header.classList.add('bg-warning');
        header.innerText = `編輯刊登費方案（方案ID${plan.planId} ${plan.planName}）`;

        button.classList.remove('btn-success');
        button.classList.add('btn-warning');
        button.innerText = '✅ 確認編輯';
        // 🔽 滾動至表單區域 + 聚焦
        document.getElementById('planName')?.focus();
        document.getElementById('planName')?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    function clearListingPlanForm() {
        const fields = [
            'planName', 'pricePerDay', 'minListingDays', 'currencyCode', 'startAt', 'endAt'
        ];
        fields.forEach(id => {
            const el = document.getElementById(id);
            if (el) el.value = '';
        });
        document.getElementById('currencyCode').value = 'TWD';
        window.editingPlanId = null;
        // 重置樣式
        const card = document.querySelector('.card.mt-4');
        const header = card.querySelector('.card-header');
        const button = card.querySelector('.card-body button');
        document.getElementById("cancelEditBtn").classList.add("d-none");
        header.classList.remove('bg-warning');
        header.classList.add('bg-success');
        header.innerText = '新增刊登費方案';

        button.classList.remove('btn-warning');
        button.classList.add('btn-success');
        button.innerText = '💾 儲存方案';
    }
    // 顯示 Toast 通知
    function showToast(message, isSuccess = false) {
        const toastId = `toast-${Date.now()}`;
        const bgClass = isSuccess ? 'bg-success' : 'bg-danger';
        const textColor = 'text-white';

        const toastHTML = `
        <div id="${toastId}" class="toast align-items-center ${bgClass} ${textColor}" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="3000">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

        const toastArea = document.getElementById('toastArea');
        toastArea.insertAdjacentHTML('beforeend', toastHTML);

        const toastElement = new bootstrap.Toast(document.getElementById(toastId));
        toastElement.show();

        // 自動移除 DOM（可選）
        setTimeout(() => {
            const el = document.getElementById(toastId);
            if (el) el.remove();
        }, 3500);
    }
    async function loadGroupedActivePlans() {
        const container = document.getElementById('activePlanContainer');

        try {
            const res = await fetch('/Dashboard/GetGroupedActivePlans');
            if (!res.ok) throw new Error('❌ 無法載入執行中方案');

            const data = await res.json();
            console.log("Active plans grouped:", data);

            if (!data || data.length === 0) {
                container.innerHTML += `<div class="text-danger">目前沒有任何執行中方案</div>`;
                return;
            }

            const row = document.createElement('div');
            row.className = 'row'; 

            data.forEach(group => {
                const col = document.createElement('div');
                col.className = 'col-md-4 mb-4'; // ❗你可以改成 col-md-4 顯示三欄

                let innerHTML = `
        <div class="p-3 border rounded bg-white h-100">
            <h6 class="fw-bold text-primary">最小上架天數：${group.minListingDays} 天</h6>
            <div class="row g-3">
    `;

                group.plans.forEach(plan => {
                    innerHTML += `
            <div class="col-12">
                <div class="card shadow-sm">
                    <div class="card-body">
                        <p class="mb-1"><strong>方案名稱：</strong>${plan.planName}</p>
                        <p class="mb-1"><strong>開始：</strong>${formatDateTime(plan.startAt)}</p>
                        <p class="mb-1"><strong>結束：</strong>${plan.endAt ? formatDateTime(plan.endAt) : '無期限'}</p>
                        <p class="mb-0"><strong>上架費用：</strong>NT$${plan.pricePerDay} 元/天</p>
                    </div>
                </div>
            </div>
        `;
                });

                innerHTML += `
            </div>
        </div>
    `;

                col.innerHTML = innerHTML;
                row.appendChild(col);
            });

            // 最後再加到 container
            container.appendChild(row);


        } catch (err) {
            console.error(err);
            container.innerHTML += `<div class="text-danger">${err.message}</div>`;
        }
    }

    
    async function loadScheduledPlans() {
        const container = document.getElementById('scheduledPlanContainer');

        try {
            const res = await fetch('/Dashboard/GetScheduledPlans');
            if (!res.ok) throw new Error('❌ 無法載入已排程方案');

            const data = await res.json();
            console.log("Scheduled plans grouped:", data);

            if (!data || data.length === 0) {
                container.innerHTML += `<div class="text-danger">目前沒有任何已排程方案</div>`;
                return;
            }

            const row = document.createElement('div');
            row.className = 'row';

            data.forEach(group => {
                const col = document.createElement('div');
                col.className = 'col-md-4 mb-4';

                let innerHTML = `
                <div class="p-3 border rounded bg-white h-100">
                    <h6 class="fw-bold text-warning">上架天數：${group.minListingDays} 天</h6>
                    <div class="row g-3">
            `;
                group.plans.forEach(plan => {
                    innerHTML += `
                    <div class="col-12">
                        <div class="card shadow-sm">
                            <div class="card-body">
                                <p class="mb-1"><strong>方案名稱：</strong>${plan.planName}</p>
                                <p class="mb-1"><strong>開始：</strong>${formatDateTime(plan.startAt)}</p>
                                <p class="mb-1"><strong>結束：</strong>${plan.endAt ? formatDateTime(plan.endAt) : '無期限'}</p>
                                <p class="mb-0"><strong>上架費用：</strong>NT$${plan.pricePerDay} 元/天</p>
                            </div>
                        </div>
                    </div>
                `;
                });

                innerHTML += `
                    </div>
                </div>
            `;

                col.innerHTML = innerHTML;
                row.appendChild(col);
            });

            container.appendChild(row);

        } catch (err) {
            console.error(err);
            container.innerHTML += `<div class="text-danger">${err.message}</div>`;
        }
    }




    // 初始化頁面
    window.onload = () => {
        renderListingPlans();
        loadScheduledPlans(); // 載入已排程方案
        loadGroupedActivePlans(); // 載入現執行方案
    };
    window.loadGroupedActivePlans = loadGroupedActivePlans;
    window.clearListingPlanForm = clearListingPlanForm;
    window.renderListingPlans = renderListingPlans;
    // 提供外部呼叫用
    
    
})();