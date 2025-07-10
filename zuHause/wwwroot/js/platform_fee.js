(() => {
    //服務費設定
    const profiles = {
        1: { listingFee: 50, Minlistthreshold: 15, },
        2: { listingFee: 40, Minlistthreshold: 10, },
        3: { listingFee: 60, Minlistthreshold: 11, }
    };
    function loadProfileData(profileId) {
        const p = profiles[profileId];
        if (!p) {
            console.warn("⚠️ 無效的設定檔 ID：", profileId);
            return;
        }

        document.getElementById('listingFee').value = p.listingFee;
        document.getElementById('Minlistthreshold').value = p.Minlistthreshold; // 預設天數為30天
        calculateTotal();
    }
    let selectedProfile = 1;
    loadProfileData(selectedProfile);
    // 設定欄位資料
    
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
        initPlatformFee(); // 綁定輸入事件
        loadProfileData(1); // 初始載入第一組設定
    };
    // 提供外部呼叫用
    window.loadProfileData = loadProfileData;
    window.initPlatformFee = initPlatformFee;
})();