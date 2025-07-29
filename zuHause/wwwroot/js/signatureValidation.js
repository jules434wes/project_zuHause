const signaturePads = {};

document.addEventListener("DOMContentLoaded", function () {
    const modals = document.querySelectorAll(".modal");

    modals.forEach(modal => {
        modal.addEventListener("shown.bs.modal", function () {
            const form = modal.querySelector("form");
            const canvas = modal.querySelector("canvas");
            const hiddenInput = modal.querySelector("input[name='SignatureDataUrl']");
            const clearBtn = modal.querySelector(".clear-signature-btn");

            if (!form || !canvas || !hiddenInput) return;

            const id = modal.id;
            if (signaturePads[id]) return;

            canvas.width = canvas.offsetWidth;
            canvas.height = canvas.offsetHeight;

            const signaturePad = new SignaturePad(canvas);
            signaturePads[id] = signaturePad;

            form.addEventListener("submit", function (e) {
                if (signaturePad.isEmpty()) {
                    e.preventDefault();
                    alert("請於畫布上簽名後再送出！");
                    return;
                }

                const dataURL = signaturePad.toDataURL("image/png");
                hiddenInput.value = dataURL;
            });

            if (clearBtn) {
                clearBtn.addEventListener("click", () => {
                    signaturePad.clear();
                });
            }
        });
    });
});
