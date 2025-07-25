document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".signature-upload-area").forEach(signatureUploadArea => {
        const input = signatureUploadArea.querySelector("input[type='file']");
        const label = signatureUploadArea.querySelector("label");
        const inputFileName = signatureUploadArea.querySelector(".input-file-name");
        const feedback = signatureUploadArea.querySelector(".input-feedback small");

        if (!input || !label || !inputFileName || !feedback) return;

        input.addEventListener("change", function () {
            const file = this.files[0];

            if (!file) {
                inputFileName.textContent = "";
                feedback.textContent = "";
                feedback.classList.remove("text-danger", "text-muted");
                return;
            }

            inputFileName.textContent = file.name;

            const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
            const maxSize = 5 * 1024 * 1024;

            if (!allowedTypes.includes(file.type)) {
                feedback.textContent = "檔案格式錯誤，只允許 jpg/png/webp";
                feedback.classList.remove("text-muted");
                feedback.classList.add("text-danger");
            } else if (file.size > maxSize) {
                feedback.textContent = "檔案過大，請小於 5MB";
                feedback.classList.remove("text-muted");
                feedback.classList.add("text-danger");
            } else {
                feedback.textContent = "檔案格式與大小正確";
                feedback.classList.remove("text-danger");
                feedback.classList.add("text-muted");
            }
        });
    });

});