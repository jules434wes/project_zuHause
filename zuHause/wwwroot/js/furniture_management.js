(() => {
    window.editFurniture = function (furnitureId) {
        // 依照傳入 id 決定編輯哪一筆
        document.getElementById("formMode").innerText = "✏️ 家具編輯模式（編號 " + furnitureId + "）";
        document.getElementById("furnitureName").value = "魚皮沙發";
        document.getElementById("furnitureDesc").value = "柔軟舒適";
        document.getElementById("furnitureStock").value = 10;
        document.getElementById("furnitureType").value = "沙發";
        window.scrollTo({ top: document.getElementById("furnitureForm").offsetTop - 60, behavior: "smooth" });
    };

    window.resetForm = function () {
        document.getElementById("formMode").innerText = "🆕 家具上傳模式";
        document.getElementById("furnitureName").value = "";
        document.getElementById("furnitureDesc").value = "";
        document.getElementById("furnitureStock").value = "";
        document.getElementById("furnitureType").selectedIndex = 0;
        if (document.getElementById("imageUpload")) {
            document.getElementById("imageUpload").value = "";
        }
    };
})();
