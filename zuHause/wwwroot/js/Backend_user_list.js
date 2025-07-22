(() => {

    
    window.renderUserTable = async function () {
        const tbody = document.getElementById("backendUserTable");
        tbody.innerHTML = "載入中...";

        try {
            const res = await fetch("/Dashboard/admins/list");
            if (!res.ok) throw new Error("讀取用戶失敗");

            const users = await res.json();
            tbody.innerHTML = "";

            users.forEach(user => {
                const tr = document.createElement("tr");
                tr.innerHTML = `
                <td>${user.account}</td>
                <td>••••••</td>
                <td>${user.name}</td>
                <td>${user.roleCode}</td>
                <td>
                    <button class="btn btn-warning btn-sm me-1" onclick="editUser('${user.adminId}')">編輯</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteUser('${user.adminId}')">刪除</button>
                </td>
            `;
                tbody.appendChild(tr);
            });
        } catch (err) {
            console.error("❌ 載入後台用戶失敗：", err);
            tbody.innerHTML = `<tr><td colspan="5" class="text-danger">載入失敗</td></tr>`;
        }
    };
    window.saveBackendUser = async () => {
        const account = document.getElementById("employeeId").value.trim();
        const password = document.getElementById("employeepassword").value.trim();
        const name = document.getElementById("employeeName").value.trim();
        const roleCode = document.getElementById("employeeRole").value;

        if (!account || !password || !name || !roleCode) {
            alert("請完整填寫所有欄位");
            return;
        }

        try {
            const res = await fetch("/Dashboard/admins/create", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ account, password, name, roleCode })
            });

            if (!res.ok) {
                const err = await res.text();
                alert("❌ 新增失敗：" + err);
                return;
            }

            const result = await res.json();
            alert(result.message || "新增成功");

            document.getElementById("employeeId").value = "";
            document.getElementById("employeepassword").value = "";
            document.getElementById("employeeName").value = "";
            

            renderUserTable();
        } catch (err) {
            console.error("❌ 錯誤:", err);
            alert("新增失敗，請稍後再試");
        }
    };
    const emojiMap = {
        overview: "📊", monitor: "📦", behavior: "👤", orders: "🛒", system: "🛠️",
        roles: "🛡️", Backend_user_list: "👨‍💻", contract_template: "📄",
        platform_fee: "💰", imgup: "🖼️", furniture_fee: "📦",
        Marquee_edit: "🌀", furniture_management: "🛋️",
        announcement_management: "📢", member_list: "👤",
        landlord_list: "🏘️", property_list: "🏠",
        property_complaint_list: "⚠️", customer_service_list: "🎧",
        system_message_list: "📨"
    };

    async function loadRoleOptions() {
        const select = document.getElementById("employeeRole");
        if (!select) return console.warn("找不到 select#employeeRole");

        select.innerHTML = "<option disabled selected>載入中...</option>";

        try {
            const res = await fetch("/Dashboard/roles/activeList");
            if (!res.ok) throw new Error("載入角色失敗");
            const roles = await res.json();

            select.innerHTML = "";

            roles.forEach(role => {
                const perms = JSON.parse(role.permissions || "{}");
                const summary = Object.keys(perms)
                    .filter(k => perms[k])
                    .map(k => emojiMap[k] || k)
                    .join(" ");

                const opt = document.createElement("option");
                opt.value = role.roleCode;
                opt.textContent = `${role.roleName}（${summary || "無權限"}）`;

                select.appendChild(opt);
            });
        } catch (err) {
            console.error("❌ 載入角色下拉失敗：", err);
            select.innerHTML = "<option disabled>載入失敗</option>";
        }
    }
    window.editUser = async function (adminId) {
        try {
            const res = await fetch(`/Dashboard/admins/detail/${adminId}`);
            if (!res.ok) throw new Error("載入資料失敗");
            const user = await res.json();

            document.getElementById("employeeId").value = user.account;
            document.getElementById("employeeId").disabled = true; // 帳號不能改
            document.getElementById("employeepassword").value = ""; // 空代表不改
            document.getElementById("employeeName").value = user.name;
            document.getElementById("employeeRole").value = user.roleCode;

            document.getElementById("saveBtn").textContent = "儲存變更";
            document.getElementById("cancelEditBtn")?.classList.remove("d-none");

            window.editingAdminId = user.adminId; // 存下來給儲存用
        } catch (err) {
            console.error("❌ 載入員工資料失敗：", err);
            alert("資料載入失敗");
        }
    };
    window.saveBackendUser = async () => {
        const name = document.getElementById("employeeName").value.trim();
        const password = document.getElementById("employeepassword").value.trim();
        const roleCode = document.getElementById("employeeRole").value;

        if (!name || !roleCode) {
            alert("請完整填寫姓名與權限");
            return;
        }

        const isEdit = window.editingAdminId != null;

        const payload = isEdit
            ? { adminId: window.editingAdminId, name, roleCode, password }
            : {
                account: document.getElementById("employeeId").value.trim(),
                password,
                name,
                roleCode,
            };

        const url = isEdit ? "/Dashboard/admins/update" : "/Dashboard/admins/create";
        const method = isEdit ? "PUT" : "POST";

        try {
            const res = await fetch(url, {
                method,
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload),
            });

            if (!res.ok) throw new Error("儲存失敗");
            const result = await res.json();
            alert(result.message || "儲存成功");

            clearForm();
            renderUserTable();
        } catch (err) {
            console.error("❌ 儲存失敗：", err);
            alert("儲存失敗，請稍後再試");
        }
    };
    
    window.clearForm = function () {
        document.getElementById("employeeId").value = "";
        document.getElementById("employeeId").disabled = false;
        document.getElementById("employeepassword").value = "";
        document.getElementById("employeeName").value = "";
        document.getElementById("employeeRole").value = "";

        window.editingAdminId = null;

        // 還原按鈕狀態
        document.getElementById("saveBtn").textContent = "新增用戶";
        document.getElementById("cancelEditBtn")?.classList.add("d-none");
    };
    window.deleteUser = function (adminId) {
        if (!confirm("確定要刪除這位後台用戶？")) return;

        fetch('/Dashboard/admins/delete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(adminId)
        })
            .then(res => {
                if (!res.ok) throw new Error("刪除失敗");
                return res.text();
            })
            .then(msg => {
                alert(msg);
                renderUserTable(); // 重新載入資料
            })
            .catch(err => {
                console.error("刪除失敗：", err);
                alert("刪除過程發生錯誤");
            });
    };


    // ✅ 初始化呼叫
    loadRoleOptions();


})();
