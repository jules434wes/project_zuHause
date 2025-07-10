(() => { // 範例三組設定檔
    const shipProfiles = {
        1: { base: 100, remote: 50, execute: '2025-07-01T08:00' },
        2: { base: 80, remote: 40, execute: '2025-07-02T08:00' },
        3: { base: 120, remote: 60, execute: '2025-07-03T08:00' }
    };
    let selectedShipProfile = 1;

    function loadShipProfile(id) {
        const p = shipProfiles[id];
        document.getElementById('shipBaseFee').value = p.base;
        document.getElementById('shipRemoteFee').value = p.remote;
        document.getElementById('shipExecuteTime').value = p.execute;

        calculateShip();
    }

    // 試算函式
    function calculateShip() {
        const nHouse = +document.getElementById('calcShipHouse').value;

        const isRemote = document.getElementById('calcShipRemoteChk').checked;

        const base = +document.getElementById('shipBaseFee').value;

        const remote = isRemote ? +document.getElementById('shipRemoteFee').value : 0;

        document.getElementById('calcShipBaseTxt').innerText = `基礎配送費：${base} x ${nHouse} = ${base * nHouse} 元`;

        document.getElementById('calcShipRemoteTxt').innerText = isRemote
            ? `偏遠/離島額外費：${remote} 元`
            : `偏遠/離島額外費：0 元`;

        const total = base * nHouse + remote;
        document.getElementById('calcShipTotal').innerText = total;
    }


    //家具配送費設定初始化
    function initShipFee() {
        const container = document.getElementById('tabContent');
        if (!container) return; // 若未載入 tab，直接退出

        // 1. profile 按鈕
        const shipBtns = container.querySelectorAll('.profile-btns button[data-id]');
        shipBtns.forEach(btn => {
            btn.onclick = () => {
                selectedShipProfile = +btn.dataset.id;
                loadShipProfile(selectedShipProfile);
                shipBtns.forEach(b => b.classList.remove('btn-success'));
                btn.classList.add('btn-success');
            };
        });
        

        // 2. 綁定試算欄位
        ['calcShipHouse', 'calcShipRemoteChk', 'shipBaseFee', 'shipRemoteFee']
            .forEach(id => {
                const el = container.querySelector('#' + id);
                // checkbox 用 change 也行
                el.oninput = calculateShip;
                el.onchange = calculateShip;
            });

        // 3. 載入一次
        loadShipProfile(selectedShipProfile);
    }

    window.initShipFee = initShipFee;
    window.loadShipProfile = loadShipProfile; // 若你未來也要外部切換

})();