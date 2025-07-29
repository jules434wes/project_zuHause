document.addEventListener("DOMContentLoaded", function () {
    const signatureAreas = document.querySelectorAll(".signature-upload-area");

    signatureAreas.forEach(signatureArea => {
        const form = signatureArea.closest("form");
        const input = signatureArea.querySelector("input[type='file']");
        const feedback = signatureArea.querySelector(".input-feedback small");
        const fileNameDisplay = signatureArea.querySelector(".input-file-name");

        if (!form || !input || !feedback) return;

        input.addEventListener("change", function () {
            const file = input.files[0];

            if (!file) {
                feedback.textContent = "請選擇簽名檔案";
                feedback.classList.add("text-danger");
                feedback.classList.remove("text-muted");
                fileNameDisplay.textContent = "";
                return;
            }

            const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
            const maxSize = 5 * 1024 * 1024;

            if (!allowedTypes.includes(file.type)) {
                feedback.textContent = "檔案格式錯誤，只允許 jpg/png/webp";
                feedback.classList.add("text-danger");
                feedback.classList.remove("text-muted");
                fileNameDisplay.textContent = "";
            } else if (file.size > maxSize) {
                feedback.textContent = "檔案過大，請小於 5MB";
                feedback.classList.add("text-danger");
                feedback.classList.remove("text-muted");
                fileNameDisplay.textContent = "";
            } else {
                feedback.textContent = "檔案格式與大小正確";
                feedback.classList.remove("text-danger");
                feedback.classList.add("text-muted");
                fileNameDisplay.textContent = file.name;
            }
        });

        form.addEventListener("submit", function (e) {
            const file = input.files[0];
            const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
            const maxSize = 5 * 1024 * 1024;

            let hasError = false;

            if (!file) {
                feedback.textContent = "請選擇簽名檔案";
                feedback.classList.add("text-danger");
                feedback.classList.remove("text-muted");
                hasError = true;
            } else if (!allowedTypes.includes(file.type)) {
                feedback.textContent = "檔案格式錯誤，只允許 jpg/png/webp";
                feedback.classList.add("text-danger");
                feedback.classList.remove("text-muted");
                hasError = true;
            } else if (file.size > maxSize) {
                feedback.textContent = "檔案過大，請小於 5MB";
                feedback.classList.add("text-danger");
                feedback.classList.remove("text-muted");
                hasError = true;
            }

            if (hasError) {
                e.preventDefault();
                alert("請確認簽名檔案符合格式與大小！");
            }
        });
    });
});
