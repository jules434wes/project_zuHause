(() => {
    let currentCategory = "tenant";
    // 取得圖片列表
    async function fetchCarouselImages() {
        const res = await fetch('/Dashboard/GetCarouselImages');
        const all = await res.json();
        return all.filter(x => x.category === currentCategory); // 根據當前分類過濾
    }

    
    // 渲染圖片列表
    async function renderCarouselList() {
        const list = await fetchCarouselImages();
        const container = document.getElementById("carouselImageList");
        container.innerHTML = "";

        list.forEach(img => {
            const card = document.createElement("div");
            card.className = "card mb-3";
            card.innerHTML = `
            <div class="row g-0 align-items-center">
                <div class="col-auto">
                    <img src="${img.imageUrl}" class="img-thumbnail me-3" style="width: 80px; height: 60px; object-fit: cover;" />
                </div>
                <div class="col">
                    <div class="card-body py-2 px-2">
                        <p class="mb-1"><strong>編號：</strong>${img.carouselImageId}</p>
                        <p class="mb-1"><strong>名稱：</strong>${img.imagesName}</p>
                        <p class="mb-1"><strong>類別：</strong>
                          ${img.category === 'tenant' ? '租客首頁輪播' :
                            img.category === 'landlord' ? '房東首頁輪播' :
                            img.category === 'furniture' ? '家具首頁輪播' : '未知類別'}
                        </p>
                        <p class="mb-1"><strong>連結：</strong>
                          ${img.webUrl ? `<a href="${img.webUrl}" target="_blank">${img.webUrl}</a>` : ""}
                        </p>

                        <p class="mb-1"><strong>位置：</strong>${img.displayOrder}</p>
                         <p class="mb-1"><strong>狀態：</strong>${img.isActive ? '✅ 啟用' : '❌ 停用'}</p>
                        <p class="mb-1"><strong>播放時間：</strong>${formatTime(img.startAt)} ~ ${img.endAt ? formatTime(img.endAt) : "無期限"}</p>

                            <button class="btn btn-sm btn-secondary" onclick="moveUp(${img.carouselImageId})">↑</button>
                            <button class="btn btn-sm btn-secondary" onclick="moveDown(${img.carouselImageId})">↓</button>
                            <button class="btn btn-sm btn-warning" onclick="toggleActive(${img.carouselImageId})">
                                ${img.isActive ? '停用' : '啟用'}
                            </button>
                            <button class="btn btn-sm btn-primary" onclick="fillEditForm(${img.carouselImageId})">編輯</button>
                            <button class="btn btn-sm btn-danger" onclick="deleteCarouselImage(${img.carouselImageId})">刪除</button>
                        </div>
                    </div>
                </div>
            </div>`;
            
            container.appendChild(card);

        });
    }
    window.uploadCarouselImage = uploadCarouselImage;

    // 上傳圖片（含檔案）
    async function uploadCarouselImage() {
        const formData = new FormData();
        const imageFile = document.getElementById("imageFile").files[0];
        if (!imageFile) return alert("請選擇圖片");

        // 🧩 根據分類選項 value 直接做為 PageCode 與 Category
        const pageCode = document.getElementById("category").value;

        formData.append("PageCode", pageCode);
        formData.append("Category", pageCode); // 不要重複 append 了

        formData.append("imageFile", imageFile);
        formData.append("ImagesName", document.getElementById("imagesName").value);
        formData.append("DisplayOrder", document.getElementById("displayOrder").value);
        formData.append("WebUrl", document.getElementById("webUrl").value);
        formData.append("StartAt", document.getElementById("startAt").value);
        formData.append("EndAt", document.getElementById("endAt").value || "");
        formData.append("IsActive", document.getElementById("isActive").value === "true");

        const res = await fetch("/Dashboard/UploadCarouselImage", {
            method: "POST",
            body: formData
        });

        if (res.ok) {
            alert("✅ 上傳成功！");
            resetCarouselForm();
            renderCarouselList();
        } else {
            alert("❌ 上傳失敗");
        }
    }

    window.fillEditForm = fillEditForm;
    // 編輯填入表單
    async function fillEditForm(id) {
        console.log("進入編輯");

        const list = await fetchCarouselImages();
        const item = list.find(x => x.carouselImageId === id);
        if (!item) return;

        // 填入表單欄位
        document.getElementById("carouselImageId").value = item.carouselImageId;
        document.getElementById("imagesName").value = item.imagesName;

        // 🟩 category 與 pageCode 實際是同一值，只要填一個即可
        document.getElementById("category").value = item.category;

        document.getElementById("displayOrder").value = item.displayOrder;
        document.getElementById("webUrl").value = item.webUrl ?? "";

        document.getElementById("startAt").value = item.startAt?.slice(0, 16) ?? "";
        document.getElementById("endAt").value = item.endAt?.slice(0, 16) ?? "";

        // 圖片預覽
        document.getElementById("imagePreview").src = item.imageUrl;
        document.getElementById("imagePreview").classList.remove("d-none");

        // 是否啟用
        document.getElementById("isActive").value = item.isActive ? "true" : "false";
    }

    window.toggleActive = toggleActive;
    async function toggleActive(id) {
        const res = await fetch("/Dashboard/ToggleCarouselActive", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(id)
        });

        if (res.ok) {
            const msg = await res.text();
            alert("✅ " + msg);
            renderCarouselList();
        } else {
            alert("❌ 切換啟用狀態失敗");
        }
    }

    window.updateCarouselImage = updateCarouselImage;
    // 更新（不包含圖片檔案）
    async function updateCarouselImage() {
        const id = document.getElementById("carouselImageId").value;
        if (!id) return alert("請先選擇要編輯的圖片");

        const body = {
            carouselImageId: parseInt(id),
            imagesName: document.getElementById("imagesName").value,
            category: document.getElementById("category").value,
            displayOrder: parseInt(document.getElementById("displayOrder").value),
            webUrl: document.getElementById("webUrl").value,
            startAt: document.getElementById("startAt").value,
            endAt: document.getElementById("endAt").value || null, // 可空
            isActive: document.getElementById("isActive").value === "true"
        };

        const res = await fetch("/Dashboard/UpdateCarouselImage", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (res.ok) {
            alert("✅ 編輯完成");
            resetCarouselForm();
            renderCarouselList();
        } else {
            alert("❌ 編輯失敗");
        }
    }
    window.moveUp = moveUp;
    async function moveUp(id) {
        const list = await fetchCarouselImages();
        const index = list.findIndex(x => x.carouselImageId === id);
        if (index <= 0) return; // 已經在最上面

        const targetId = list[index - 1].carouselImageId;
        await swapOrder(id, targetId);
    }
    window.moveDown = moveDown;
    async function moveDown(id) {
        const list = await fetchCarouselImages();
        const index = list.findIndex(x => x.carouselImageId === id);
        if (index === -1 || index >= list.length - 1) return; // 已經最下面

        const targetId = list[index + 1].carouselImageId;
        await swapOrder(id, targetId);
    }

    async function swapOrder(id1, id2) {
        const res = await fetch("/Dashboard/SwapCarouselOrder", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ imageId1: id1, imageId2: id2 })
        });

        if (res.ok) {
            renderCarouselList(); // 重新載入排序後的列表
        } else {
            const msg = await res.text();
            alert(msg || "❌ 順序交換失敗");
        }
    }

    window.changeCategoryTab = function (btn) {
        // 移除舊的 active
        document.querySelectorAll("#carouselCategoryTabs .nav-link").forEach(tab => {
            tab.classList.remove("active");
        });

        // 加上新的 active
        btn.classList.add("active");

        // 更新分類
        currentCategory = btn.getAttribute("data-category");

        // 重新渲染列表
        renderCarouselList();
    }

    window.deleteCarouselImage = deleteCarouselImage;
    // 刪除
    async function deleteCarouselImage(id) {
        if (!confirm("確定要刪除這張圖片？")) return;
        const res = await fetch("/Dashboard/DeleteCarouselImage", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(id)
        });

        if (res.ok) {
            alert("✅ 已刪除");
            renderCarouselList();
        } else {
            alert("❌ 刪除失敗");
        }
    }
    //輪播分類選擇
    async function loadCategoryOptions() {
        const res = await fetch('/Dashboard/GetCarouselCategories');
        const list = await res.json();

        const select = document.getElementById("category");
        select.innerHTML = "";

        list.forEach(item => {
            const opt = document.createElement("option");
            opt.value = item.pageCode;
            opt.textContent = item.pageName;
            select.appendChild(opt);
        });
    }
    window.resetCarouselForm = resetCarouselForm
    // 清空表單
    function resetCarouselForm() {
        document.getElementById("carouselImageId").value = "";
        document.getElementById("imageFile").value = "";
        document.getElementById("imagesName").value = "";
        document.getElementById("category").value = "";
        document.getElementById("displayOrder").value = "";
        document.getElementById("webUrl").value = "";
        document.getElementById("startAt").value = "";
        document.getElementById("endAt").value = "";
        document.getElementById("imagePreview").classList.add("d-none");
        document.getElementById("imagePreview").src = "#";
        document.getElementById("isActive").value = "true";

    }

    // 工具：格式化時間
    function formatTime(dt) {
        if (!dt) return "";
        return dt.replace("T", " ").substring(0, 16);
    }
    
    // 初始化
    window.initCarouselManager = () => {
        loadCategoryOptions(); //載入分類
        renderCarouselList();
        resetCarouselForm();
       

    };

})();