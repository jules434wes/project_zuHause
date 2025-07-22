(() => {
    let editingRoleCode = null; // null 表示新增模式，非 null 表示編輯模式

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
        furniture_management: "🛋️ 家具列表管理",
        announcement_management: "📢 公告管理",
        member_list: "👤 會員列表與驗證",
        landlord_list: "🏘️ 房東列表",
        property_list: "🏠 房源列表",
        property_complaint_list: "⚠️ 房源投訴列表",
        customer_service_list: "🎧 客服處理",
        system_message_list: "📨 系統訊息"
    };

    // ✅ 動態產生 checkbox 欄位
    const container = document.getElementById("permissionCheckboxContainer");
    if (container) {
        Object.entries(tabNames).forEach(([key, label]) => {
            const col = document.createElement("div");
            col.className = "col-md-4"; // 3欄結構：12 / 4 = 3 欄

            col.innerHTML = `
        <div class="form-check">
            <input class="form-check-input" type="checkbox" id="chk_${key}" data-key="${key}">
            <label class="form-check-label" for="chk_${key}">${label}</label>
        </div>
    `;
            container.appendChild(col);
        });

    }

    // ✅ 權限列表讀取
    window.updateRoleListWithPermissions = async function () {
        const roleList = document.getElementById("roleList");
        if (!roleList) return;

        roleList.innerHTML = "載入中...";

        try {
            const res = await fetch("/Dashboard/roles/list");
            if (!res.ok) throw new Error("無法載入角色資料");

            const roles = await res.json();
            roleList.innerHTML = "";

            roles.forEach((role, i) => {
                const permissionLabels = (() => {
                    const perms = role.permissions || {};
                    if (perms.all === true) {
                        return "✅ 全部權限";
                    }
                    return Object.keys(perms)
                        .filter(key => perms[key])
                        .map(key => tabNames[key] || key)
                        .join("｜");
                })();

                const li = document.createElement("li");
                li.className = "list-group-item d-flex justify-content-between align-items-center";

                li.innerHTML = `
                                <div>
                                    ${i + 1}. <strong>${role.roleName}</strong>（${permissionLabels}）
                                </div>
                                <div class="d-flex gap-1">
                                    <button class="btn btn-sm btn-outline-primary" onclick='editRole(${JSON.stringify(role)})'>✏️ 編輯</button>
                                    <button class="btn btn-sm btn-outline-danger" onclick="deleteRole('${role.roleCode}')">刪除</button>
                                </div>
                                 `;
                roleList.appendChild(li);

            });
        } catch (err) {
            console.error("❌ 載入角色清單失敗：", err);
            roleList.innerHTML = "<li class='list-group-item text-danger'>載入失敗</li>";
        }
    };

    // ✅ 新增角色按鈕處理
    window.addRole = async function () {

        const input = document.getElementById("newRoleInput");
        const name = input.value.trim();
        if (!name) return alert("請輸入角色名稱");

        const accessKeys = Array.from(document.querySelectorAll("#permissionCheckboxContainer input[type='checkbox']"))
            .filter(chk => chk.checked)
            .map(chk => chk.dataset.key);

        try {
            const url = editingRoleCode ? "/Dashboard/roles/update" : "/Dashboard/roles/create";
            const method = editingRoleCode ? "PUT" : "POST";
            const payload = editingRoleCode
                ? { roleCode: editingRoleCode, roleName: name, accessKeys }
                : { roleName: name, accessKeys };

            const res = await fetch(url, {
                method,
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            if (!res.ok) throw new Error("儲存失敗");
            const result = await res.json();
            alert(result.message || "儲存成功");

            // Reset
            input.value = '';
            document.querySelectorAll("#permissionCheckboxContainer input[type='checkbox']")
                .forEach(chk => chk.checked = false);
            editingRoleCode = null;
            document.querySelector("button[onclick='addRole()']").textContent = "新增角色";
            document.getElementById("cancelEditBtn").classList.add("d-none");
            updateRoleListWithPermissions();
        } catch (err) {
            console.error("❌ 儲存角色失敗：", err);
            alert("儲存失敗，請稍後再試");
        }
    };
    window.editRole = function (role) {
        document.getElementById("newRoleInput").value = role.roleName;
        editingRoleCode = role.roleCode;

        document.querySelectorAll("#permissionCheckboxContainer input[type='checkbox']")
            .forEach(chk => {
                const key = chk.dataset.key;
                chk.checked = !!(role.permissions?.all || role.permissions?.[key]);
            });

        document.querySelector("button[onclick='addRole()']").textContent = "儲存變更";
        document.getElementById("cancelEditBtn").classList.remove("d-none");
    };



    window.deleteRole = async function (roleCode) {
        if (!confirm("確定要刪除這個角色嗎？")) return;

        try {
            const res = await fetch("/Dashboard/roles/delete", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(roleCode)
            });

            if (!res.ok) throw new Error("刪除失敗");
            const result = await res.json();
            alert(result.message || "角色已刪除");
            updateRoleListWithPermissions();
        } catch (err) {
            console.error("❌ 刪除角色失敗：", err);
            alert("刪除失敗，請稍後再試");
        }
    };
    window.cancelEdit = function () {
        document.getElementById("newRoleInput").value = '';
        document.querySelectorAll("#permissionCheckboxContainer input[type='checkbox']")
            .forEach(chk => chk.checked = false);

        editingRoleCode = null;

        document.querySelector("button[onclick='addRole()']").textContent = "新增角色";
        document.getElementById("cancelEditBtn").classList.add("d-none");
    };


    // ✅ 載入時預設呼叫
    updateRoleListWithPermissions();
})();
