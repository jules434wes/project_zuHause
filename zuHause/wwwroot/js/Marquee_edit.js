(() => {
    let currentScope = "TENANT";

    // 載入跑馬燈資料
    async function fetchMarqueeList() {
        const res = await fetch(`/Dashboard/GetMarquees?scope=${currentScope}`);
        return await res.json();
    }

    // 渲染跑馬燈清單
    async function renderMarqueeList() {
        const list = await fetchMarqueeList();
        const tbody = document.getElementById("marquee-tbody");
        tbody.innerHTML = "";

        list.forEach((item, index) => {
            const row = document.createElement("tr");
            row.dataset.id = item.siteMessagesId;
            row.innerHTML = `
            <td>
                <button class="btn btn-sm btn-secondary" onclick="moveUp(this)">↑</button>
                <button class="btn btn-sm btn-secondary" onclick="moveDown(this)">↓</button>
                <span class="ms-2 display-order">${index + 1}</span>
            </td>
            <td><input type="text" class="form-control" value="${item.siteMessageContent ?? ''}"></td>
            <td><input type="datetime-local" class="form-control" value="${formatTime(item.startAt)}"></td>
            <td><input type="datetime-local" class="form-control" value="${formatTime(item.endAt)}"></td>
            
            <td><input type="checkbox" class="form-check-input" ${item.isActive ? "checked" : ""}></td>
            <td>
                <button class="btn btn-sm btn-danger" onclick="deleteMarquee(this)">刪除</button>
            </td>`;
            tbody.appendChild(row);
        });

        // 渲染完後加上全體操作按鈕（確認 / 取消）
        const controlRow = document.createElement("tr");
        controlRow.innerHTML = `
        <td colspan="7" class="text-end">
            <button class="btn btn-success me-2" onclick="saveAllMarquees()">✅ 確認儲存全部</button>
            <button class="btn btn-outline-secondary" onclick="resetMarqueeForm()">❌ 取消變更</button>
        </td>`;
        tbody.appendChild(controlRow);
        updateNextOrderDisplay(list.length);

    }

    // 上傳 / 編輯儲存跑馬燈
    window.saveAllMarquees = async function () {
        const rows = document.querySelectorAll("#marquee-tbody tr[data-id]");

        const payload = Array.from(rows).map((row, index) => ({
            siteMessagesId: parseInt(row.dataset.id),
            siteMessageContent: row.querySelector("input[type=text]").value,
            startAt: row.querySelectorAll("input[type=datetime-local]")[0].value || null,
            endAt: row.querySelectorAll("input[type=datetime-local]")[1].value || null,
            isActive: row.querySelector("input[type=checkbox]").checked,
            moduleScope: currentScope,
            category: "MARQUEE",
            displayOrder: index + 1
        }));

        const res = await fetch("/Dashboard/BatchUpdateMarquees", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            alert("✅ 已儲存全部修改");
            renderMarqueeList();
        } else {
            alert("❌ 儲存失敗");
        }
    }

    // 類別切換按鈕事件
    window.changeMarqueeTab = function (btn) {
        document.querySelectorAll("#marqueeTabs .nav-link").forEach(tab => {
            tab.classList.remove("active");
        });

        btn.classList.add("active");
        currentScope = btn.dataset.scope;
        renderMarqueeList();
    };
    //刪除跑馬燈
    window.deleteMarquee = async function (btn) {
        const tr = btn.closest("tr");
        const id = tr.dataset.id;
        if (!id) return alert("找不到 ID");

        if (!confirm("確定要刪除這筆跑馬燈嗎？")) return;

        const res = await fetch("/Dashboard/DeleteMarquee", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(parseInt(id))
        });

        if (res.ok) {
            alert("✅ 已刪除");
            renderMarqueeList(); // 重新載入
        } else {
            alert("❌ 刪除失敗");
        }
    }
    //改變順序
    window.moveUp = function (btn) {
        const row = btn.closest("tr");
        const prev = row.previousElementSibling;
        if (prev && !prev.querySelector("button.btn-success")) {
            row.parentNode.insertBefore(row, prev);
            reorderDisplayOrders();
        }

    };

    window.moveDown = function (btn) {
        const row = btn.closest("tr");
        const next = row.nextElementSibling;
        if (next && !next.querySelector("button.btn-success")) {
            row.parentNode.insertBefore(next, row); // ⬅ 正確的交換邏輯
            reorderDisplayOrders();
        }

    };
    // ⏺ 新增跑馬燈
    window.addNewMarquee = async function () {
        const payload = {
            displayOrder: parseInt(document.getElementById("next-order").innerText),
            siteMessageContent: document.getElementById("new-content").value,
            startAt: document.getElementById("new-start").value || null,
            endAt: document.getElementById("new-end").value || null,
            isActive: document.getElementById("new-active").checked,
            moduleScope: currentScope,
            category: "MARQUEE"
        };

        if (!payload.siteMessageContent) {
            alert("請填寫內容");
            return;
        }

        const res = await fetch("/Dashboard/AddMarquee", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
       

        if (res.ok) {
            alert("✅ 新增成功");
            renderMarqueeList();
            resetNewForm();
        } else {
            alert("❌ 新增失敗");
        }
    };

    // ⏺ 新增列清除
    window.resetNewForm = function () {
        document.getElementById("new-content").value = "";
        document.getElementById("new-start").value = "";
        document.getElementById("new-end").value = "";
        document.getElementById("new-active").checked = true;
    };

    // ⏺ 更新新增列排序號（於 renderMarqueeList 最後呼叫）
    function updateNextOrderDisplay(count) {
        document.getElementById("next-order").innerText = count + 1;
    }

    // 更新畫面上排序欄位數字
    function reorderDisplayOrders() {
        
        const rows = document.querySelectorAll("#marquee-tbody tr[data-id]");
        let count = 1;
        rows.forEach(row => {
            const orderEl = row.querySelector(".display-order");
            if (orderEl) orderEl.textContent = count++;
        });
    }


    // 時間格式化
    function formatTime(dt) {
        if (!dt) return "";
        return dt.replace("T", " ").substring(0, 16);
    }

    // 表單清除（可根據需要實作）
    window.resetMarqueeForm = function () {
        renderMarqueeList();
    };

    // 初始化入口
    window.initMarqueeManager = () => {
        renderMarqueeList();
    };
})();
