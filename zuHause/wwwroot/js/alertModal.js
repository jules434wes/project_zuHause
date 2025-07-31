document.addEventListener("DOMContentLoaded", function () {
    const applyRentalBtn = document.querySelector(".apply-rental-btn");

    const noticeModalEl = document.querySelector("#noticeModal");
    const applyRentalModalEl = document.querySelector("#applyRental");

    // 確保元素存在
    if (!applyRentalBtn || !noticeModalEl || !applyRentalModalEl) {
        console.error("元素不存在，請確認 ID 與 class 是否正確");
        return;
    }

    const noticeModal = new bootstrap.Modal(noticeModalEl);
    const applyRentalModal = new bootstrap.Modal(applyRentalModalEl);

    applyRentalBtn.addEventListener("click", function (event) {
        event.preventDefault();

        if (applyRentalBtn.dataset.pass === "True") {
            console.log("通過，開啟申請 Modal");
            applyRentalModal.show();
        } else {
            console.log("不通過，顯示提醒 Modal");
            noticeModal.show();
        }
    });
});
