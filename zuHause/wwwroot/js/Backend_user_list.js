(() => {
    // 📦 用戶資料儲存在 localStorage 的 key
    const STORAGE_KEY = "backendUsers";

    // 👥 目前是否處於編輯模式
    let editingIndex = null;

    // 🚀 頁面載入後初始化
    document.addEventListener("DOMContentLoaded", () => {
        renderUserTable();
    });

    // 📥 儲存用戶
    window.saveBackendUser = () => {
        const id = document.getElementById("employeeId").value.trim();
        const password = document.getElementById("employeepassword").value.trim();
        const name = document.getElementById("employeeName").value.trim();
        const role = document.getElementById("employeeRole").value;

        if (!id || !password || !name || !role) {
            alert("請完整填寫所有欄位");
            return;
        }

        const users = getUsers();

        // 若為編輯模式，更新該筆資料
        if (editingIndex !== null) {
            users[editingIndex] = { id, password, name, role };
            editingIndex = null;
        } else {
            users.push({ id, password, name, role });
        }

        saveUsers(users);
        clearForm();
        renderUserTable();
    };

    // 🧹 清空表單
    function clearForm() {
        document.getElementById("employeeId").value = "";
        document.getElementById("employeepassword").value = "";
        document.getElementById("employeeName").value = "";
        document.getElementById("employeeRole").value = "超級管理員";
        editingIndex = null;
    }

    // 🔁 渲染表格
    function renderUserTable() {
        const users = getUsers();
        const tbody = document.getElementById("backendUserTable");
        tbody.innerHTML = "";

        users.forEach((user, index) => {
            const tr = document.createElement("tr");

            tr.innerHTML = `
                <td>${user.id}</td>
                <td>${user.password}</td>
                <td>${user.name}</td>
                <td>${user.role}</td>
                <td>
                    <button class="btn btn-warning btn-sm me-1" onclick="editUser(${index})">編輯</button>
                    <button class="btn btn-danger btn-sm" onclick="deleteUser(${index})">刪除</button>
                </td>
            `;

            tbody.appendChild(tr);
        });
    }

    // ✏️ 編輯用戶
    window.editUser = (index) => {
        const user = getUsers()[index];
        document.getElementById("employeeId").value = user.id;
        document.getElementById("employeepassword").value = user.password;
        document.getElementById("employeeName").value = user.name;
        document.getElementById("employeeRole").value = user.role;
        editingIndex = index;
    };

    // ❌ 刪除用戶
    window.deleteUser = (index) => {
        if (!confirm("確定要刪除這位用戶？")) return;
        const users = getUsers();
        users.splice(index, 1);
        saveUsers(users);
        renderUserTable();
    };

    // 🧠 工具函式：取得所有用戶
    function getUsers() {
        return JSON.parse(localStorage.getItem(STORAGE_KEY) || "[]");
    }

    // 💾 工具函式：儲存所有用戶
    function saveUsers(users) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(users));
    }
})();
