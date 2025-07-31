
// 延遲初始化，等待 Modal 元素載入
function initializeApplyRentalUpload() {
    let applyRental = document.querySelector("#applyRental");
    if (!applyRental) return;
    
    let uploadInputArea = applyRental.querySelector(".upload-input-area");
    let uploadTemplate = applyRental.querySelector("#upload-template");
    let addBtn = applyRental.querySelector(".add-btn");
    
    if (!uploadInputArea || !uploadTemplate || !addBtn) return;

    let fileIndex = 1;

    function bindUploadItemEvents(item, index) {
        const inputId = `input-label-${index}`;
        const input = item.querySelector("input");
        const label = item.querySelector("label");
        const inputFileName = item.querySelector(".input-file-name");
        const inputFeedbackText = item.querySelector(".input-feedback small");
        const delBtn = item.querySelector(".del-btn");

        input.name = "UploadFiles";
        input.id = inputId;
        label.setAttribute("for", inputId);

        input.addEventListener("change", function () {
            const file = this.files[0];
            if (!file) {
                inputFileName.textContent = "";
                return;
            }

            inputFileName.textContent = file.name;

            const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
            const maxSize = 5 * 1024 * 1024;

            if (!allowedTypes.includes(file.type)) {
                inputFeedbackText.textContent = "檔案格式錯誤，只允許 jpg/png/webp";
                inputFeedbackText.classList.remove("text-muted");
                inputFeedbackText.classList.add("text-danger");
            } else if (file.size > maxSize) {
                inputFeedbackText.textContent = "檔案過大，請小於 5MB";
                inputFeedbackText.classList.remove("text-muted");
                inputFeedbackText.classList.add("text-danger");
            } else {
                inputFeedbackText.textContent = "檔案格式與大小正確";
                inputFeedbackText.classList.remove("text-danger");
                inputFeedbackText.classList.add("text-muted");
            }
        });

        delBtn.addEventListener("click", function () {
            item.remove();
        });
    }

    const initialItems = uploadInputArea.querySelectorAll(".input-item");
    initialItems.forEach((item, i) => {
        bindUploadItemEvents(item, fileIndex);
        fileIndex++;
    });

    addBtn.addEventListener("click", function () {
        const clone = uploadTemplate.content.cloneNode(true);
        const newItem = clone.querySelector("div.row");
        newItem.classList.add("input-item");

        uploadInputArea.appendChild(newItem);
        bindUploadItemEvents(newItem, fileIndex);
        fileIndex++;
    });

    const form = document.querySelector(".apply-rental-form");
    if (form) {
        // 簽名相關處理
        const canvas = document.getElementById("signatureCanvas");
        if (canvas) {
            const signaturePad = new SignaturePad(canvas);
            
            // 簽名清除功能
            window.clearSignature = function() {
                signaturePad.clear();
            };
            
            // 表單提交時處理簽名
            form.addEventListener("submit", function (e) {
                if (canvas && !signaturePad.isEmpty()) {
                    const dataURL = signaturePad.toDataURL("image/png");
                    const signatureInput = document.getElementById("SignatureDataUrl");
                    if (signatureInput) {
                        signatureInput.value = dataURL;
                    }
                }
            });
        }

        // 表單驗證處理
        form.addEventListener("submit", function (e) {
            const inputItems = uploadInputArea.querySelectorAll(".input-item");
            let hasError = false;
            let furnitureError = false;
            let notesError = false;
            let signatureError = false;

            // 檔案上傳驗證
            inputItems.forEach(item => {
                const input = item.querySelector("input[type='file']");
                const inputFeedbackText = item.querySelector(".input-feedback small");
                
                if (input && input.files.length > 0) {
                    const file = input.files[0];
                    const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
                    const maxSize = 5 * 1024 * 1024;

                    if (!allowedTypes.includes(file.type)) {
                        inputFeedbackText.textContent = "檔案格式錯誤，只允許 jpg/png/webp";
                        inputFeedbackText.classList.remove("text-muted");
                        inputFeedbackText.classList.add("text-danger");
                        hasError = true;
                    } else if (file.size > maxSize) {
                        inputFeedbackText.textContent = "檔案過大，請小於 5MB";
                        inputFeedbackText.classList.remove("text-muted");
                        inputFeedbackText.classList.add("text-danger");
                        hasError = true;
                    }
                }
            });

            // 家具驗證
            if (document.querySelector(".furniture-body")) {
                furnitureError = validateFurnitureTable();
            }

            // 備註驗證
            if (document.querySelector(".note-area")) {
                notesError = validateNotes();
            }

            // 簽名驗證（對於合約頁面）
            const canvas = document.getElementById("signatureCanvas");
            if (canvas) {
                const signaturePad = new SignaturePad(canvas);
                if (signaturePad.isEmpty()) {
                    signatureError = true;
                    alert("請於畫布上簽名再送出合約！");
                    e.preventDefault();
                    return;
                }
            }

            // 總合驗證
            const totalError = hasError || notesError || furnitureError || signatureError;

            if (totalError) {
                e.preventDefault();
                alert("請修正檔案上傳錯誤再送出表單！");
            }
        });
    }
}

function validateFurnitureTable() {
    let hasError = false;
    const rows = document.querySelectorAll(".furniture-body .furniture-row");

    rows.forEach(row => {
        row.querySelectorAll("input").forEach(input => {
            if (!input.value.trim()) {
                input.classList.add("is-invalid");
                hasError = true;
            } else {
                input.classList.remove("is-invalid");
            }
        });
    });

    return hasError;
}

function validateNotes() {
    let hasError = false;
    const noteItems = document.querySelectorAll(".note-area .note-item");
    
    noteItems.forEach(item => {
        const textarea = item.querySelector("textarea");
        const feedback = item.querySelector(".input-feedback small");
        const text = textarea.value.trim();

        if (text.length > 100) {
            feedback.textContent = "備註字數不可超過 100 字";
            feedback.classList.remove("text-muted");
            feedback.classList.add("text-danger");
            hasError = true;
        } else {
            feedback.textContent = "";
            feedback.classList.remove("text-danger");
        }
    });

    return hasError;
}

function validateSignatureFile() {
    const area = document.querySelector(".signature-upload-area");
    if (!area || area.closest(".d-none")) return false;

    const input = area.querySelector("input[type='file']");
    const feedback = area.querySelector(".input-feedback small");
    const file = input.files[0];

    // 檢查 Canvas 簽名
    const canvas = document.getElementById("signatureCanvas");
    if (canvas) {
        const signaturePad = new SignaturePad(canvas);
        const hasCanvasSignature = !signaturePad.isEmpty();

        // 若 Canvas 有簽名，就不再驗證圖片檔案
        if (hasCanvasSignature) {
            feedback.textContent = "已使用 Canvas 簽名";
            feedback.classList.remove("text-danger");
            feedback.classList.add("text-muted");
            return false; // 沒錯誤
        }
    }

    return false;
}

// 在 Modal 顯示時初始化
document.addEventListener('DOMContentLoaded', function() {
    // 監聽 Modal 顯示事件
    document.addEventListener('shown.bs.modal', function(e) {
        if (e.target.id === 'applyRental') {
            initializeApplyRentalUpload();
        }
    });
    
    // 如果 Modal 已經存在於頁面中，直接初始化
    setTimeout(initializeApplyRentalUpload, 500);
});