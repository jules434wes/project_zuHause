(() => {
    if (typeof roleAccess === 'undefined') {
        console.warn("🚫 roleAccess 未定義，無法初始化角色清單");
        return;
    }

    window.updateRoleListWithPermissions = function () {
        const roleList = document.getElementById("roleList");
        if (!roleList) return;

        roleList.innerHTML = ""; // 清空原本的列表

        const emojiMap = {
            overview: "📊", monitor: "📦", behavior: "👤", orders: "🛒", system: "🛠️",
            roles: "🛡️", Backend_user_list: "👨‍💻", contract_template: "📄",
            platform_fee: "💰", imgup: "🖼️", furniture_fee: "📦",
            Marquee_edit: "🌀", furniture_management: "🛋️"
        };

        const roleNames = Object.keys(roleAccess || {});
        roleNames.forEach((role, i) => {
            const li = document.createElement("li");
            li.className = "list-group-item";
            const permissions = roleAccess[role].map(key => emojiMap[key] || key).join(" ");
            li.textContent = `${i + 1}. ${role}（${permissions}）`;
            roleList.appendChild(li);
        });
    };

    window.addRole = function () {
        const input = document.getElementById("newRoleInput");
        const name = input.value.trim();
        if (!name) return alert("請輸入角色名稱");

        const accessKeys = [];
        if (document.getElementById("chkOverview").checked) accessKeys.push("overview");
        if (document.getElementById("chkMonitor").checked) accessKeys.push("monitor");
        if (document.getElementById("chkBehavior").checked) accessKeys.push("behavior");
        if (document.getElementById("chkOrders").checked) accessKeys.push("orders");
        if (document.getElementById("chkSystem").checked) accessKeys.push("system");

        roleAccess[name] = accessKeys;

        input.value = '';
        document.querySelectorAll(".form-check-input").forEach(chk => chk.checked = false);

        window.updateRoleListWithPermissions();
    };

    // 不要再用 DOMContentLoaded，這樣會錯過事件時機
    // document.addEventListener("DOMContentLoaded", updateRoleListWithPermissions);
})();
