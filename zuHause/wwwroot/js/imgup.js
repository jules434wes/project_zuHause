(() => {
   
    // 取得圖片列表
    async function fetchCarouselImages() {
        const res = await fetch('/Dashboard/GetCarouselImages');
        return await res.json();
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
                        <p class="mb-1"><strong>類別：</strong>${img.category}</p>
                        <p class="mb-1"><strong>位置：</strong>${img.displayOrder}</p>
                        <p class="mb-1"><strong>播放：</strong>${formatTime(img.startAt)} ~ ${formatTime(img.endAt)}</p>
                        <div class="d-flex gap-2 mt-2">
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

        formData.append("imageFile", imageFile);
        formData.append("ImagesName", document.getElementById("imagesName").value);
        formData.append("Category", document.getElementById("category").value);
        formData.append("DisplayOrder", document.getElementById("displayOrder").value);
        //formData.append("PageCode", document.getElementById("pageCode").value);
        formData.append("StartAt", document.getElementById("startAt").value);
        formData.append("EndAt", document.getElementById("endAt").value);

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
        console.log("帶入編輯");
        document.getElementById("carouselImageId").value = item.carouselImageId;
        document.getElementById("imagesName").value = item.imagesName;
        document.getElementById("category").value = item.category;
        document.getElementById("displayOrder").value = item.displayOrder;
        //document.getElementById("pageCode").value = item.pageCode ?? "";
        document.getElementById("startAt").value = item.startAt?.slice(0, 16);
        document.getElementById("endAt").value = item.endAt?.slice(0, 16);
        document.getElementById("imagePreview").src = item.imageUrl;
        document.getElementById("imagePreview").classList.remove("d-none");
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
            //pageCode: document.getElementById("pageCode").value,
            startAt: document.getElementById("startAt").value,
            endAt: document.getElementById("endAt").value,
            isActive: true
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
        //document.getElementById("pageCode").value = "";
        document.getElementById("startAt").value = "";
        document.getElementById("endAt").value = "";
        document.getElementById("imagePreview").classList.add("d-none");
        document.getElementById("imagePreview").src = "#";
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