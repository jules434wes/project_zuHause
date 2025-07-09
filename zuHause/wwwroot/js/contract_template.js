(() => {
    const STORAGE_KEY = "contractTemplates";

    // 初始化
    document.addEventListener("DOMContentLoaded", () => {
        renderTemplateList();

        const input = document.getElementById("fileInput");
        if (input) {
            input.addEventListener("change", handleFilePreview);
        }
    });

    // 立即預覽 HTML
    function handleFilePreview(event) {
        const file = event.target.files[0];
        if (!file || !file.name.endsWith(".html")) {
            alert("請上傳 .html 檔案");
            return;
        }

        const reader = new FileReader();
        reader.onload = (e) => {
            const htmlContent = e.target.result;

            // ✅ 建立 blob URL 作為 iframe 預覽內容
            const blob = new Blob([htmlContent], { type: "text/html" });
            const previewUrl = URL.createObjectURL(blob);

            const iframe = document.getElementById("pdfPreview");

            // 💡 清除舊 URL（避免記憶體泄漏）
            if (iframe.dataset.url) {
                URL.revokeObjectURL(iframe.dataset.url);
            }

            iframe.src = previewUrl;
            iframe.dataset.url = previewUrl; // 儲存目前 URL 做清除用
        };
        reader.readAsText(file);
    }


    // 上傳並儲存
    window.uploadFile = () => {
        const name = document.getElementById("fileNameInput").value.trim();
        const file = document.getElementById("fileInput").files[0];

        if (!name || !file) {
            alert("請輸入檔案名稱並選擇檔案");
            return;
        }

        const reader = new FileReader();
        reader.onload = (e) => {
            const htmlContent = e.target.result;
            const list = getTemplates();
            list.push({ name, content: htmlContent });
            saveTemplates(list);
            renderTemplateList();
            cancelUpload();
        };
        reader.readAsText(file);
    };

    // 渲染列表
    window.renderTemplateList = () => {
        const list = getTemplates();
        const container = document.getElementById("fileList");
        if (!container) return;

        container.innerHTML = "";

        if (list.length === 0) {
            container.innerHTML = "<p class='text-muted'>尚無上傳範本</p>";
            return;
        }

        list.forEach((item, i) => {
            const div = document.createElement("div");
            div.className = "d-flex justify-content-between align-items-center border rounded p-2 mb-2 bg-white";

            div.innerHTML = `
        <div>${item.name}</div>
        <div class="btn-group">
          <button class="btn btn-sm btn-outline-primary" onclick="previewTemplate(${i})">預覽</button>
          <button class="btn btn-sm btn-outline-danger" onclick="deleteTemplate(${i})">刪除</button>
        </div>
      `;
            container.appendChild(div);
        });
    };

    // 預覽
    window.previewTemplate = (index) => {
        const list = getTemplates();
        const item = list[index];
        if (!item) return;

        document.getElementById("pdfPreview").srcdoc = item.content;
        document.getElementById("fileNameInput").value = item.name;
    };

    // 刪除
    window.deleteTemplate = (index) => {
        if (!confirm("確定要刪除這個範本？")) return;

        const list = getTemplates();
        list.splice(index, 1);
        saveTemplates(list);
        renderTemplateList();
        cancelUpload();
    };

    // 清空
    window.cancelUpload = () => {
        document.getElementById("fileInput").value = "";
        document.getElementById("fileNameInput").value = "";

        const iframe = document.getElementById("pdfPreview");

        // 💡 清除 blob URL
        if (iframe.dataset.url) {
            URL.revokeObjectURL(iframe.dataset.url);
            delete iframe.dataset.url;
        }

        iframe.src = "about:blank"; // 重置 iframe
    };


    // 工具
    function getTemplates() {
        return JSON.parse(localStorage.getItem(STORAGE_KEY) || "[]");
    }

    function saveTemplates(list) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(list));
    }
})();
