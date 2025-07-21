(() => {
    document.addEventListener("DOMContentLoaded", () => {
        renderTemplateList();
        bindContractUploadEvents();
    });
    window.updateLivePreview = () => {
        const html = document.getElementById("templateEditor").value;
        const iframe = document.getElementById("livePreview");
        iframe.srcdoc = html;
    };

    // 預覽用
    function handleFilePreview(event) {
        const file = event.target.files[0];
        if (!file || !file.name.endsWith(".html")) {
            alert("請上傳 .html 檔案");
            return;
        }

        const reader = new FileReader();
        reader.onload = (e) => {
            const htmlContent = e.target.result;
            const blob = new Blob([htmlContent], { type: "text/html" });
            const previewUrl = URL.createObjectURL(blob);
            const iframe = document.getElementById("pdfPreview");

            if (iframe.dataset.url) {
                URL.revokeObjectURL(iframe.dataset.url);
            }

            iframe.src = previewUrl;
            iframe.dataset.url = previewUrl;
        };
        reader.readAsText(file);
    }

    // 上傳
    window.uploadFile = async () => {
        const name = document.getElementById("fileNameInput").value.trim();
        const file = document.getElementById("fileInput").files[0];
        if (!name || !file) {
            alert("請輸入檔案名稱並選擇檔案");
            return;
        }

        const reader = new FileReader();
        reader.onload = async (e) => {
            const htmlContent = e.target.result;

            const res = await fetch('/Dashboard/UploadContractTemplate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ templateName: name, templateContent: htmlContent })
            });

            if (!res.ok) {
                alert("❌ 儲存失敗");
                return;
            }

            document.getElementById("uploadSuccess").classList.remove("d-none");
            setTimeout(() => document.getElementById("uploadSuccess").classList.add("d-none"), 3000);
            renderTemplateList();
            cancelUpload();
        };
        reader.readAsText(file);
    };

    // 渲染列表
    window.renderTemplateList = async () => {
        const list = await getTemplates();
        const container = document.getElementById("fileList");
        container.innerHTML = "";

        if (list.length === 0) {
            container.innerHTML = "<p class='text-muted'>尚無上傳範本</p>";
            return;
        }

        list.forEach((item, i) => {
            const div = document.createElement("div");
            div.className = "d-flex justify-content-between align-items-center border rounded p-2 mb-2 bg-white";

            div.innerHTML = `
                <div>${item.templateName}</div>
                <div class="btn-group">
                    <button class="btn btn-sm btn-outline-primary" onclick="previewTemplate(${i})">預覽</button>
                    <button class="btn btn-sm btn-outline-secondary" onclick="editTemplate(${i})">編輯</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteTemplate(${i})">刪除</button>
                </div>
            `;
            container.appendChild(div);
        });
    };

    // 預覽
    window.previewTemplate = async (index) => {
        const list = await getTemplates();
        const item = list[index];
        if (!item) return;

        document.getElementById("pdfPreview").srcdoc = item.templateContent;
        document.getElementById("fileNameInput").value = item.templateName;
        document.getElementById("previewTitle").innerText = "📄 預覽中：" + item.templateName;
    };

    // 編輯
    window.editTemplate = async (index) => {
        const list = await getTemplates();
        const item = list[index];
        if (!item) return;

        document.getElementById("templateEditor").value = item.templateContent;
        document.getElementById("editingTemplateId").value = item.contractTemplateId;
        updateLivePreview(); // ⬅️ 新增這行
    };

    // 儲存編輯
    window.saveTemplateEdit = async () => {
        const id = document.getElementById("editingTemplateId").value;
        const content = document.getElementById("templateEditor").value;

        if (!id || !content) {
            alert("請選擇要編輯的範本並填入內容");
            return;
        }

        const res = await fetch('/Dashboard/UpdateContractTemplate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                contractTemplateId: parseInt(id),
                templateContent: content
            })
        });

        if (res.ok) {
            alert("✅ 修改成功");
            renderTemplateList();
        } else {
            alert("❌ 修改失敗");
        }
    };

    // 刪除
    window.deleteTemplate = async (index) => {
        if (!confirm("確定要刪除這個範本？")) return;

        const list = await getTemplates();
        const deletedItem = list[index];

        const res = await fetch('/Dashboard/DeleteContractTemplate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(deletedItem.contractTemplateId)
        });

        if (res.ok) {
            renderTemplateList();
            cancelUpload();
        } else {
            alert("❌ 刪除失敗");
        }
    };

    // 清空
    window.cancelUpload = () => {
        document.getElementById("fileInput").value = "";
        document.getElementById("fileNameInput").value = "";
        document.getElementById("previewTitle").innerText = "";
        document.getElementById("pdfPreview").removeAttribute("srcdoc");
        document.getElementById("pdfPreview").src = "about:blank";
    };

    // 工具
    async function getTemplates() {
        const res = await fetch('/Dashboard/GetContractTemplates');
        return await res.json();
    }

    window.bindContractUploadEvents = () => {
        const input = document.getElementById("fileInput");
        if (input) {
            input.addEventListener("change", handleFilePreview);
        }
    };
})();