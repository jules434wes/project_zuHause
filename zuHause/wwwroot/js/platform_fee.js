(() => {
    let listingPlans = []; // 儲存後端回傳的資料

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
                <td>${plan.discountTrigger}</td>
                <td>${plan.discountUnit}</td>
                <td>${plan.startAt} ~ ${plan.endAt ?? '無期限'}</td>
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
            discountTrigger: document.getElementById('discountTrigger').value || null,
            discountUnit: document.getElementById('discountUnit').value || null,
            currencyCode: document.getElementById('currencyCode').value.trim(),
            startAt: document.getElementById('startAt').value,
            endAt: document.getElementById('endAt').value || null
        };

        // 轉換成數值或 null
        plan.discountTrigger = plan.discountTrigger !== null ? parseInt(plan.discountTrigger) : null;
        plan.discountUnit = plan.discountUnit !== null ? parseInt(plan.discountUnit) : null;

        // 驗證必要欄位
        if (!plan.planName || isNaN(plan.pricePerDay) || isNaN(plan.minListingDays)) {
            alert("❌ 資料不完整或格式錯誤");
            return;
        }

        try {
            const res = await fetch('/Dashboard/CreateListingPlan', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(plan)
            });

            if (!res.ok) throw new Error("❌ 儲存失敗");

            const result = await res.json();
            alert("✅ 新增成功！");
            renderListingPlans(); // 重新渲染表格
            clearListingPlanForm();
        } catch (err) {
            alert(err.message);
        }
    }
    function clearListingPlanForm() {
        const fields = [
            'planName',
            'pricePerDay',
            'minListingDays',
            'discountTrigger',
            'discountUnit',
            'currencyCode',
            'startAt',
            'endAt'
        ];

        fields.forEach(id => {
            const el = document.getElementById(id);
            if (el) el.value = '';
        });

        // 重設幣別為 TWD
        const currency = document.getElementById('currencyCode');
        if (currency) currency.value = 'TWD';
    }
    

    
    
    
    
    // 計算試算區
    function calculateTotal() {
        const days = parseInt(document.getElementById('daysInput')?.value || 0);
        const listingFee = parseFloat(document.getElementById('listingFee')?.value || 0);
        const totalListing = listingFee * days;
        const total = totalListing;

        // 顯示試算明細
        document.getElementById('calcService').innerText = `上架服務費：${listingFee} * ${days} = ${totalListing} 元`;
        document.getElementById('calcTotal').innerText = `總計：${total.toFixed(0)} 元`;
    }
    window.editPlan = editPlan;
    function editPlan(planId) {
        const plan = listingPlans.find(p => p.planId === planId);
        if (!plan) {
            alert("❌ 找不到該方案");
            return;
        }

        document.getElementById('listingFee').value = plan.pricePerDay;
        document.getElementById('Minlistthreshold').value = plan.minListingDays;
        document.getElementById('daysInput').value = plan.minListingDays;

        const modeLabel = document.getElementById('formMode');
        if (modeLabel)
            modeLabel.innerText = `✏️ 編輯中：${plan.planName}（ID: ${plan.planId}）`;

        calculateTotal();
    }
    //平台收費方案初始化
    function initPlatformFee() {
        const ids = ['daysInput', 'listingFee'];
        ids.forEach(id => {
            const el = document.getElementById(id);
            if (el) el.oninput = calculateTotal;
        });

        calculateTotal();
    }
    
    window.onload = () => {
        renderListingPlans();
        initPlatformFee();
        
    };
    window.renderListingPlans = renderListingPlans;
    // 提供外部呼叫用
    
    
})();