(() => {
    // 編輯家具（🔧 測試用資料，未串接後端）
    window.editFurniture = function (furnitureId) {
        document.getElementById("formMode").innerText = "✏️ 家具編輯模式（編號 " + furnitureId + "）";
        document.getElementById("furnitureName").value = "魚皮沙發";
        document.getElementById("furnitureDesc").value = "柔軟舒適";
        document.getElementById("furnitureStock").value = 10;
        document.getElementById("furnitureType").value = "沙發";
        window.scrollTo({ top: document.getElementById("furnitureForm").offsetTop - 60, behavior: "smooth" });
    };

    // 重設表單
    window.resetForm = function () {
        document.getElementById("formMode").innerText = "🆕 家具上傳模式";
        document.getElementById("furnitureName").value = "";
        document.getElementById("furnitureDesc").value = "";
        document.getElementById("furnitureStock").value = "";
        document.getElementById("furnitureType").selectedIndex = 0;
        document.getElementById("originalPrice").value = "";
        document.getElementById("rentPerDay").value = "";
        document.getElementById("listDate").value = "";
        document.getElementById("delistDate").value = "";
        document.getElementById("productStatus").selectedIndex = 0;
        if (document.getElementById("imageUpload")) {
            document.getElementById("imageUpload").value = "";
        }

    };
  
    
    // 軟刪除家具
    window.deleteFurniture = function (furnitureId) {
        if (!confirm("確定要刪除這筆家具嗎？")) return;
        fetch(`/Dashboard/SoftDeleteFurniture?id=${furnitureId}`, { method: "POST" })
            .then(res => res.text())
            .then(msg => {
                alert(msg);
                openTab("furniture_management");
            });
    };

    // 提前下架家具
    window.setProductOffline = function (furnitureId) {
        if (!confirm(`確定要提前下架家具 ${furnitureId} 嗎？`)) return;
            
        fetch("/Dashboard/SetOffline", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ id: furnitureId })  // ✅ 用物件包裝
        })
            .then(response => {
                if (!response.ok) throw new Error("伺服器回應失敗");
                return response.text();
            })
            .then(msg => {
                alert(msg);
               
                

                // ✅ 重新載入家具列表區域
                fetch('/Dashboard/furniture_management')
                    .then(res => res.text())
                    .then(html => {
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(html, 'text/html');
                        const newList = doc.querySelector('.furniture-list-scroll');
                        if (newList) {
                            document.querySelector('.furniture-list-scroll').replaceWith(newList);
                            console.log("✅ 家具狀態已更新！");
                        }
                    });
            })
            .catch(err => alert("❌ 發生錯誤：" + err.message));
    };

    // 提交家具資料
    window.submitFurniture = function () {
        alert("你有觸發 submitFurniture！");

        const data = {
            Name: $("#furnitureName").val().trim(),
            Description: $("#furnitureDesc").val().trim(),
            Type: $("#furnitureType").val(),
            OriginalPrice: parseFloat($("#originalPrice").val()),
            RentPerDay: parseFloat($("#rentPerDay").val()),
            Stock: parseInt($("#furnitureStock").val()),
            StartDate: $("#listDate").val(),
            EndDate: $("#delistDate").val(),
            Status: $("#productStatus").val() === "true"
        };


        console.log("送出的資料：", data);

        if (!data.Name || isNaN(data.OriginalPrice) || isNaN(data.RentPerDay) || isNaN(data.Stock)) {
            alert("❌ 請確認所有欄位都有正確填寫！");
            return;
        }


        fetch('/Dashboard/UploadFurniture', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(data)
        })
            .then(res => res.text())
            .then(msg => {
                alert(msg);
                resetForm();

                // 部分重新載入家具卡片區域
                fetch('/Dashboard/furniture_management')
                    .then(res => res.text())
                    .then(html => {
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(html, 'text/html');
                        const newList = doc.querySelector('.furniture-list-scroll');
                        if (newList) {
                            document.querySelector('.furniture-list-scroll').replaceWith(newList);
                            alert("✅ 家具已上傳並即時刷新！");
                        }
                    });
            })
            .catch(err => alert("❌ 錯誤：" + err.message));
    };
    //家具庫存歷史紀錄表
    window.loadAllInventoryEvents = function () {
        console.log("查詢庫存歷史");
        fetch("/Dashboard/AllInventoryEvents")
            .then(res => res.json())
            .then(data => {
                const tbody = document.getElementById("inventoryEventBody");
                tbody.innerHTML = "";
                data.forEach(row => {
                    tbody.innerHTML += `
                    <tr>
                        <td>${row.productId}</td>
                        <td>${row.eventType}</td>
                        <td>${row.quantity}</td>
                        <td>${row.sourceType}</td>
                        <td>${row.sourceId}</td>
                        <td>${row.occurredAt}</td>
                        <td>${row.recordedAt}</td>
                    </tr>`;
                });
            });
    }
    window.submitInventoryAdjustment = function () {
        const data = {
            ProductId: document.getElementById("adjustProductId").value.trim(),
            Quantity: parseInt(document.getElementById("adjustQuantity").value),
            SourceType: document.getElementById("adjustSourceType").value.trim(),
            SourceId: document.getElementById("adjustSourceId").value.trim()
        };

        if (!data.ProductId || isNaN(data.Quantity) || !data.SourceType) {
            alert("❌ 請完整填寫 商品ID、異動數量 和 來源類型！");
            return;
        }

        fetch("/Dashboard/AdjustInventory", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        })
            .then(res => res.text())
            .then(msg => {
                alert(msg);
                document.getElementById("adjustProductId").value = "";
                document.getElementById("adjustQuantity").value = "";
                document.getElementById("adjustSourceType").value = "";
                document.getElementById("adjustSourceId").value = "";

                // 🌀 異動成功 → 重新載入紀錄表
                if (typeof loadAllInventoryEvents === "function") loadAllInventoryEvents();
            })
            .catch(err => alert("❌ 發生錯誤：" + err.message));
    };



    // 👉 確保載入成功後綁定 submit 按鈕
    const btn = document.getElementById("submitBtn");
    if (btn) {
        console.log("✅ 綁定 submitBtn");
        btn.addEventListener("click", submitFurniture);
    } else {
        console.warn("⚠️ 找不到 #submitBtn，可能 DOM 還沒載入");
    }

})();
