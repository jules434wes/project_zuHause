document.addEventListener("DOMContentLoaded", function () {
    const noteInputArea = document.querySelector(".note-input-area");
    const noteTemplate = document.querySelector("#note-template");
    const addNoteBtn = document.querySelector(".add-note-btn");

    function updateNoteItemNames() {
        const items = noteInputArea.querySelectorAll(".note-item");
        items.forEach((item, index) => {
            const textarea = item.querySelector("textarea");
            const hiddenInput = item.querySelector("input[type='hidden']");
            textarea.name = `Comments[${index}].CommentText`;
            hiddenInput.name = `Comments[${index}].CommentType`;
        });
    }

    function bindNoteItemEvents(item) {
        const textarea = item.querySelector("textarea");
        const delBtn = item.querySelector(".del-note-btn");
        const feedback = item.querySelector(".input-feedback small");

        textarea.addEventListener("input", function () {
            const text = textarea.value.trim();
            if (text.length > 100) {
                feedback.textContent = "內容超過100字限制";
                feedback.classList.remove("text-muted");
                feedback.classList.add("text-danger");
            } else {
                feedback.textContent = "";
                feedback.classList.remove("text-danger");
            }
        });

        delBtn.addEventListener("click", function () {
            item.remove();
            updateNoteItemNames();
        });
    }

    const initialItems = noteInputArea.querySelectorAll(".note-item");
    initialItems.forEach(item => bindNoteItemEvents(item));
    updateNoteItemNames();

    addNoteBtn.addEventListener("click", function () {
        const clone = noteTemplate.content.cloneNode(true);
        const newItem = clone.querySelector(".note-item");
        noteInputArea.appendChild(newItem);
        bindNoteItemEvents(newItem);
        updateNoteItemNames();
    });

    window.validateNotes = function () {
        let hasError = false;
        const noteItems = noteInputArea.querySelectorAll(".note-item");

        noteItems.forEach(item => {
            const textarea = item.querySelector("textarea");
            const feedback = item.querySelector(".input-feedback small");
            const text = textarea.value.trim();

            if (text.length > 100) {
                feedback.textContent = "內容超過100字限制";
                feedback.classList.remove("text-muted");
                feedback.classList.add("text-danger");
                hasError = true;
            }
        });

        return hasError;
    };
});
