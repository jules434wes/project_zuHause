
let applyRental = document.querySelector("#applyRental");
let uploadInputArea = applyRental.querySelector(".upload-input-area");
let uploadTemplate = applyRental.querySelector("#upload-template");
let addBtn = applyRental.querySelector(".add-btn");

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

form.addEventListener("submit", function (e) {
    console.log(111111);
    const inputItems = uploadInputArea.querySelectorAll(".input-item");
    let hasError = false;

    inputItems.forEach(item => {
        const input = item.querySelector("input[type='file']");
        const inputFeedbackText = item.querySelector(".input-feedback small");
        const file = input.files[0];

        if (!file) {
            inputFeedbackText.textContent = "";
            inputFeedbackText.classList.remove("text-danger", "text-muted");
            return;
        }

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
        } else {
            inputFeedbackText.textContent = "檔案格式與大小正確";
            inputFeedbackText.classList.remove("text-danger");
            inputFeedbackText.classList.add("text-muted");
        }
    });
    console.log("最外層上傳", hasError)
    let notesError = false;
    let furnitureError = false;
    let signatureError = false;

    if (document.querySelector(".furniture-body")) {
        console.log(1);
        furnitureError = validateFurnitureTable();
    }

    if (document.querySelector(".note-area")) {
        console.log(2);
        notesError = validateNotes();
    }
    if (document.querySelector(".signature-upload-area")) {
        console.log(3);
    signatureError = validateSignatureFile();   
    }
    console.log("==== submit 開始 ====");
    console.log("上傳區塊有錯：", hasError);
    console.log("家具錯誤：", furnitureError);
    console.log("備註錯誤：", notesError);
    console.log("簽名錯誤：", signatureError);

    hasError = hasError || notesError || furnitureError || signatureError;

    console.log("最終 hasError：", hasError);


    if (hasError) {
        console.log("為啥還會近來")
        e.preventDefault();
        alert("請修正檔案上傳錯誤再送出表單！");
    }
});


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
    console.log("validateFurnitureTable", hasError)
    return hasError;
}


function validateNotes() {
    let hasError = false;
    const noteItems = document.querySelectorAll(".note-area .note-item");
    console.log(noteItems)
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

    console.log("validateNotes", hasError)
    return hasError;
}


function validateSignatureFile() {
    const area = document.querySelector(".signature-upload-area");
    if (!area || area.closest(".d-none")) return false; // 若區塊沒出現，不驗證

    const input = area.querySelector("input[type='file']");
    const feedback = area.querySelector(".input-feedback small");
    const fileNameDisplay = area.querySelector(".input-file-name");

    let hasError = false;
    const file = input.files[0];

    if (!file) {
        feedback.textContent = "請選擇簽名檔案";
        feedback.classList.add("text-danger");
        hasError = true;
    } else {
        const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
        const maxSize = 5 * 1024 * 1024;

        if (!allowedTypes.includes(file.type)) {
            feedback.textContent = "檔案格式錯誤，只允許 jpg/png/webp";
            feedback.classList.add("text-danger");
            hasError = true;
        } else if (file.size > maxSize) {
            feedback.textContent = "檔案過大，請小於 5MB";
            feedback.classList.add("text-danger");
            hasError = true;
        } else {
            feedback.textContent = "檔案格式與大小正確";
            feedback.classList.remove("text-danger");
            feedback.classList.add("text-muted");
            fileNameDisplay.textContent = file.name;
        }
    }

    console.log("validateSignatureFile" ,hasError)
    return hasError;
}
