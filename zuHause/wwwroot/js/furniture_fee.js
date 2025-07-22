(() => {
    let editingPlanId = null;
    let deliveryPlans = [];

    function formatDateTime(raw) {
        const dt = new Date(raw);
        return dt.toLocaleString('zh-TW', {
            year: 'numeric', month: '2-digit', day: '2-digit',
            hour: '2-digit', minute: '2-digit', hour12: true
        });
    }

    // ✅ 載入所有方案進表格
    async function renderDeliveryPlans() {
        const tbody = document.querySelector('#deliveryPlansTable tbody');
        tbody.innerHTML = '載入中...';

        try {
            const res = await fetch('/Dashboard/GetAllDeliveryPlans');
            if (!res.ok) throw new Error('❌ 無法取得配送費方案');

            deliveryPlans = await res.json();
            tbody.innerHTML = '';

            deliveryPlans.forEach(plan => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${plan.planId}</td>
                    <td>${plan.planName}</td>
                    <td>NT$${plan.baseFee}</td>
                    <td>NT$${plan.remoteAreaSurcharge}</td>
                    <td>${formatDateTime(plan.startAt)} ~ ${plan.endAt ? formatDateTime(plan.endAt) : '無期限'}</td>
                    <td>${plan.isActive ? '✅ 啟用中' : '❌ 已停用'}</td>
                    <td><button class="btn btn-sm btn-outline-primary" onclick="editDeliveryPlan(${plan.planId})">✏️ 編輯</button></td>
                `;
                tbody.appendChild(tr);
            });
        } catch (err) {
            tbody.innerHTML = `<tr><td colspan="7" class="text-danger">${err.message}</td></tr>`;
        }
    }

    // ✅ 儲存或編輯方案
    window.submitDeliveryPlan = async function () {
        const plan = {
            planName: document.getElementById('deliveryPlanName').value.trim(),
            baseFee: parseFloat(document.getElementById('baseFee').value),
            remoteAreaSurcharge: parseFloat(document.getElementById('remoteFee').value),
            currencyCode: document.getElementById('deliveryCurrencyCode').value.trim(),
            startAt: document.getElementById('deliveryStartAt').value,
            endAt: document.getElementById('deliveryEndAt').value || null
        };

        if (!plan.planName || isNaN(plan.baseFee)) {
            showToast("❌ 資料不完整或格式錯誤", false);
            return;
        }

        if (editingPlanId !== null) {
            plan.planId = editingPlanId;
        }

        try {
            const res = await fetch(editingPlanId === null
                ? '/Dashboard/CreateDeliveryPlan'
                : '/Dashboard/UpdateDeliveryPlan', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(plan)
            });

            const result = await res.json();
            if (!result.success) throw new Error(result.message || '❌ 儲存失敗');

            showToast(editingPlanId === null ? "✅ 新增成功！" : "✅ 編輯完成！", true);
            renderDeliveryPlans();
            loadGroupedActiveDeliveryPlans();
            loadScheduledDeliveryPlans();
            clearDeliveryPlanForm();

        } catch (err) {
            showToast(err.message, false);
        }
    };
    window.clearDeliveryPlanForm = function () {
        editingPlanId = null;

        // 清空欄位
        document.getElementById('deliveryPlanName').value = '';
        document.getElementById('baseFee').value = '';
        document.getElementById('remoteFee').value = '';
        document.getElementById('deliveryCurrencyCode').value = 'TWD';
        document.getElementById('deliveryStartAt').value = '';
        document.getElementById('deliveryEndAt').value = '';

        // 還原標題樣式
        const card = document.querySelector('.card.mt-4');
        const header = card.querySelector('.card-header');
        const button = card.querySelector('.card-body button');

        header.classList.remove('bg-warning');
        header.classList.add('bg-success');
        header.innerText = '新增配送費方案';

        button.classList.remove('btn-warning');
        button.classList.add('btn-success');
        button.innerText = '💾 儲存方案';

        // 隱藏取消按鈕
        document.getElementById("cancelDeliveryEditBtn").classList.add("d-none");

        // 聚焦第一欄位
        document.getElementById('deliveryPlanName').focus();
    };

    // ✅ 進入編輯模式
    window.editDeliveryPlan = function (id) {
        const plan = deliveryPlans.find(p => p.planId === id);
        if (!plan) return alert("❌ 找不到方案");
        document.getElementById('deliveryPlanName').scrollIntoView({ behavior: 'smooth', block: 'center' });
        editingPlanId = id;
        document.getElementById('deliveryPlanName').value = plan.planName;
        document.getElementById('baseFee').value = plan.baseFee;
        document.getElementById('remoteFee').value = plan.remoteAreaSurcharge;
        document.getElementById('deliveryCurrencyCode').value = plan.currencyCode;
        document.getElementById('deliveryStartAt').value = plan.startAt?.substring(0, 16);
        document.getElementById('deliveryEndAt').value = plan.endAt ? plan.endAt.substring(0, 16) : '';

        document.querySelector('.card-header').classList.remove('bg-success');
        document.querySelector('.card-header').classList.add('bg-warning');
        document.querySelector('.card-header').innerText = `編輯配送費方案（ID ${id}）`;
        document.getElementById('cancelDeliveryEditBtn').classList.remove('d-none');
    };

   
    // ✅ 顯示 toast 提示
    function showToast(msg, isSuccess = false) {
        const toastId = `toast-${Date.now()}`;
        const toastHTML = `
        <div id="${toastId}" class="toast align-items-center ${isSuccess ? 'bg-success' : 'bg-danger'} text-white" role="alert" data-bs-delay="3000">
            <div class="d-flex">
                <div class="toast-body">${msg}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>`;
        const area = document.getElementById('toastArea');
        area.insertAdjacentHTML('beforeend', toastHTML);
        const toast = new bootstrap.Toast(document.getElementById(toastId));
        toast.show();
        setTimeout(() => document.getElementById(toastId)?.remove(), 3500);
    }

    // ✅ 載入目前啟用中方案
    async function loadGroupedActiveDeliveryPlans() {
        const container = document.getElementById('activeDeliveryPlanContainer');
        container.innerHTML = ''; // ✅ 加這行，先清空
        const res = await fetch('/Dashboard/GetGroupedActiveDeliveryPlans');
        const data = await res.json();
        if (!data || data.length === 0) {
            container.innerHTML += '<div class="text-danger">目前無啟用中的配送方案</div>';
            return;
        }

        const row = document.createElement('div');
        row.className = 'row';
        data.forEach(group => {
            const col = document.createElement('div');
            col.className = 'col-md-4 mb-4';
            col.innerHTML = `
            <div class="p-3 border rounded bg-white h-100">
                <h6 class="fw-bold text-primary">基礎費用：NT$${group.baseFee} 元</h6>
                <div class="row g-3">
                    ${group.plans.map(p => `
                        <div class="col-12">
                            <div class="card shadow-sm">
                                <div class="card-body">
                                    <p class="mb-1"><strong>方案：</strong>${p.planName}</p>
                                    <p class="mb-1"><strong>期間：</strong>${formatDateTime(p.startAt)} ~ ${p.endAt ? formatDateTime(p.endAt) : '無期限'}</p>
                                    <p class="mb-0"><strong>偏遠費：</strong> NT$${p.remoteAreaSurcharge} 元</p>
                                </div>
                            </div>
                        </div>`).join('')}
                </div>
            </div>`;
            row.appendChild(col);
        });
        container.appendChild(row);
    }

    // ✅ 載入已排程方案
    async function loadScheduledDeliveryPlans() {
        const container = document.getElementById('scheduledDeliveryPlanContainer');
        container.innerHTML = ''; // ✅ 加這行，先清空
        const res = await fetch('/Dashboard/GetScheduledDeliveryPlans');
        const data = await res.json();
        if (!data || data.length === 0) {
            container.innerHTML += '<div class="text-danger">沒有排程中的方案</div>';
            return;
        }

        const row = document.createElement('div');
        row.className = 'row';
        data.forEach(group => {
            const col = document.createElement('div');
            col.className = 'col-md-4 mb-4';
            col.innerHTML = `
            <div class="p-3 border rounded bg-white h-100">
                <h6 class="fw-bold text-primary border-bottom pb-1 mb-2">基礎費用：NT$${group.baseFee} 元</h6>
                <div class="row g-3">
                    ${group.plans.map(p => `
                        <div class="col-12">
                            <div class="card shadow-sm">
                                <div class="card-body">
                                    <p class="mb-1"><strong>方案：</strong>${p.planName}</p>
                                    <p class="mb-1"><strong>期間：</strong>${formatDateTime(p.startAt)} ~ ${p.endAt ? formatDateTime(p.endAt) : '無期限'}</p>
                                    <p class="mb-0"><strong>偏遠費：</strong>NT$${p.remoteAreaSurcharge} 元</p>
                                </div>
                            </div>
                        </div>`).join('')}
                </div>
            </div>`;
            row.appendChild(col);
        });
        container.appendChild(row);
    }

    // ✅ 初始化
    window.onload = () => {
        renderDeliveryPlans();
        loadGroupedActiveDeliveryPlans();
        loadScheduledDeliveryPlans();
    };
})();
