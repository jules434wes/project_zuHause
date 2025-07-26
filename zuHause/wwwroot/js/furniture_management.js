(() => {
    
    document.getElementById("imageUpload").addEventListener("change", function () {
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                const preview = document.getElementById("imagePreview");
                preview.src = e.target.result;
                preview.style.display = "block";
            };
            reader.readAsDataURL(file);
        }
    });


    window.editFurniture = function (furnitureId) {
        fetch(`/Dashboard/GetFurnitureById?id=${furnitureId}`)
            .then(res => {
                if (!res.ok) throw new Error("查無此家具");
                return res.json();
            })
            .then(data => {
                // 進入編輯模式 UI
                document.getElementById("formMode").innerText = `✏️ 家具編輯模式（編號 ${furnitureId}）`;
                document.getElementById("returnToUploadBtn").style.display = "inline-block";
                document.getElementById("stockInputGroup").style.display = "none";
                // 進入編輯模式時觸發：
                
                    document.getElementById("submitBtn").style.display = "none";
                    document.getElementById("updateBtn").style.display = "inline-block";
                
                // 填入資料
                document.getElementById("furnitureName").value = data.productName;
                document.getElementById("furnitureDesc").value = data.description;
                document.getElementById("furnitureType").value = data.categoryId;
                document.getElementById("originalPrice").value = data.listPrice;
                document.getElementById("rentPerDay").value = data.dailyRental;
                document.getElementById("furnitureSafeStock").value = data.safetyStock;
                document.getElementById("listDate").value = data.listedAt;
                document.getElementById("delistDate").value = data.delistedAt;
                document.getElementById("productStatus").value = data.status ? "true" : "false";
                console.log("回傳資料", data);
                // 存起來目前編輯的 ID（可隱藏欄位或變數）
                document.getElementById("furnitureForm").dataset.editingId = furnitureId;

                // 滾到表單區
                window.scrollTo({ top: document.getElementById("furnitureForm").offsetTop - 60, behavior: "smooth" });

                // 切換提交按鈕行為
                document.getElementById("updateBtn").onclick = () => updateFurniture(furnitureId);
            })
            .catch(err => alert("❌ 讀取資料失敗：" + err.message));
    };
    function updateFurniture(furnitureId) {
        const formData = new FormData();
        formData.append("FurnitureProductId", furnitureId);
        formData.append("Name", $("#furnitureName").val().trim());
        formData.append("Description", $("#furnitureDesc").val().trim());
        formData.append("Type", $("#furnitureType").val());
        formData.append("OriginalPrice", $("#originalPrice").val());
        formData.append("RentPerDay", $("#rentPerDay").val());
        formData.append("SafetyStock", $("#furnitureSafeStock").val());
        formData.append("StartDate", $("#listDate").val());
        formData.append("EndDate", $("#delistDate").val());
        formData.append("Status", $("#productStatus").val() === "true");

        const imageInput = document.getElementById("imageUpload");
        if (imageInput.files.length > 0) {
            formData.append("ImageFile", imageInput.files[0]);
        }

        fetch("/Dashboard/UpdateFurniture", {
            method: "POST",
            body: formData
        })
            .then(res => res.text())
            .then(msg => {
                alert(msg);
                resetForm();
                openTab("furniture_management");
                fetch('/Dashboard/furniture_management')
                    .then(res => res.text())
                    .then(html => {
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(html, 'text/html');
                        const newList = doc.querySelector('.furniture-list-scroll');
                        if (newList) {
                            document.querySelector('.furniture-list-scroll').replaceWith(newList);
                        }
                    });
            })
            .catch(err => alert("❌ 更新失敗：" + err.message));
    }




    // 重設表單
    window.resetForm = function () {
        document.getElementById("furnitureForm").dataset.editingId = "";
        document.getElementById("formMode").innerText = "🆕 家具上傳模式";
        document.getElementById("returnToUploadBtn").style.display = "none"; // 隱藏返回按鈕
        document.getElementById("stockInputGroup").style.display = "block"; // 顯示上架庫存欄
        document.getElementById("submitBtn").style.display = "inline-block";
        document.getElementById("updateBtn").style.display = "none";
        document.getElementById("furnitureName").value = "";
        document.getElementById("furnitureDesc").value = "";
        document.getElementById("furnitureStock").value = "";
        document.getElementById("furnitureSafeStock").value = "";
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
                fetch('/Dashboard/furniture_management')
                    .then(res => res.text())
                    .then(html => {
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(html, 'text/html');
                        const newList = doc.querySelector('.furniture-list-scroll');
                        if (newList) {
                            document.querySelector('.furniture-list-scroll').replaceWith(newList);
                          
                        }
                    });
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
    const formData = new FormData();
    formData.append("Name", $("#furnitureName").val().trim());
    formData.append("Description", $("#furnitureDesc").val().trim());
    formData.append("Type", $("#furnitureType").val());
    formData.append("OriginalPrice", $("#originalPrice").val());
    formData.append("RentPerDay", $("#rentPerDay").val());
    formData.append("Stock", $("#furnitureStock").val());
    formData.append("SafetyStock", $("#furnitureSafeStock").val());
    formData.append("StartDate", $("#listDate").val());
    formData.append("EndDate", $("#delistDate").val());
    formData.append("Status", $("#productStatus").val() === "true");

    const imageInput = document.getElementById("imageUpload");
    if (imageInput.files.length > 0) {
        formData.append("ImageFile", imageInput.files[0]);
    }

    fetch("/Dashboard/UploadFurniture", {
        method: "POST",
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: formData
    })
    .then(res => res.text())
    .then(msg => {
        alert(msg);
        resetForm();
        fetch('/Dashboard/furniture_management')
            .then(res => res.text())
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newList = doc.querySelector('.furniture-list-scroll');
                if (newList) {
                    document.querySelector('.furniture-list-scroll').replaceWith(newList);
                    showToast("✅ 家具已上傳並即時刷新！");
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
                        <td>${row.eventType === 'adjust_in' ? '入庫' : '出庫'}</td>
                        <td>${row.quantity}</td>
                         <td>
                            ${row.sourceType === 'shrinkage' ? '🧹 減損' :
                             row.sourceType === 'restock' ? '📦 補貨' :
                            row.sourceType === 'manual' ? '✋ 手動' : row.sourceType}
                        </td>
                        <td>${row.sourceId}</td>
                        <td>${row.occurredAt}</td>
                        <td>${row.recordedAt}</td>
                    </tr>`;
                });
                // 部分重新載入家具卡片區域
                fetch('/Dashboard/furniture_management')
                    .then(res => res.text())
                    .then(html => {
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(html, 'text/html');
                        const newList = doc.querySelector('.furniture-list-scroll');
                        if (newList) {
                            document.querySelector('.furniture-list-scroll').replaceWith(newList);
                           
                        }
                    });
            });
    }
    window.submitInventoryAdjustment = function () {
        const data = {
            productId: document.getElementById("adjustProductId").value.trim(),
            quantity: parseInt(document.getElementById("adjustQuantity").value),
            sourceType: document.getElementById("adjustSourceType").value,
            sourceId: document.getElementById("adjustSourceId").value.trim()
        };

        if (!data.productId || isNaN(data.quantity)) {
            alert("❌ 請完整填寫 商品ID 與 異動數量！");
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
                document.getElementById("adjustSourceId").value = "";
                if (typeof loadAllInventoryEvents === "function") loadAllInventoryEvents();
            })
            .catch(err => alert("❌ 發生錯誤：" + err.message));
    };
    // 家具卡片篩選功能
    document.getElementById("filterCategory").addEventListener("change", filterFurnitureCards);
    document.getElementById("searchKeyword").addEventListener("input", filterFurnitureCards);

    function filterFurnitureCards() {
        const category = document.getElementById("filterCategory").value.trim().toLowerCase();
        const keyword = document.getElementById("searchKeyword").value.trim().toLowerCase();

        const cards = document.querySelectorAll(".furniture-list-scroll .col");
        const noResultEl = document.getElementById("noResultMessage");

        let anyVisible = false;

        cards.forEach(card => {
            const typeEl = card.querySelector(".furniture-type");
            const nameEl = card.querySelector(".furniture-name");
            const descEl = card.querySelector(".furniture-desc");

            const Type = typeEl?.innerText.toLowerCase() || "";
            const Name = nameEl?.innerText.toLowerCase() || "";
            const Desc = descEl?.title?.toLowerCase() || "";

            const matchType = !category || Type.includes(category);
            const matchKeyword = !keyword || Name.includes(keyword) || Desc.includes(keyword);

            const shouldShow = matchType && matchKeyword;
            card.style.display = shouldShow ? "" : "none";

            if (shouldShow) anyVisible = true;
        });

        // 顯示或隱藏「查無結果」
        if (noResultEl) {
            noResultEl.style.display = anyVisible ? "none" : "block";
        }
    }




    // 👉 確保載入成功後綁定 submit 按鈕
    const btn = document.getElementById("submitBtn");
    if (btn) {
        console.log("✅ 綁定 submitBtn");
        btn.addEventListener("click", submitFurniture);
    } else {
        console.warn("⚠️ 找不到 #submitBtn，可能 DOM 還沒載入");
    }

})();
