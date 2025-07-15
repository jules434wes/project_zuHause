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
                <td>${plan.pricePerDay}</td>
                <td>${plan.minListingDays}</td>
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

    

    // 頁面載入時執行
   
    // 提供外部呼叫用

    //服務費設定
    
    
    
    
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