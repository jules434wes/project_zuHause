let allAccordion = document.querySelectorAll(".accordion-area");

allAccordion.forEach(item => {
    item.addEventListener("show.bs.collapse", function (e) {
        const collapsePanel = e.target;
        if (collapsePanel.classList.contains("is-read")) {
            console.log("已經讀過");
            return;
        }

        const notificationId = item.dataset.notificationId;
        const applicationId = item.dataset.applicationId;
        const moduleCode = item.dataset.moduleCode;

        console.log("未讀過，開始標記與更新狀態");

        if (moduleCode != "System") {
            fetch('/MemberInbox/MarkAsReadApi', {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(parseInt(notificationId))
            })
                .then(response => {
                    if (!response.ok) throw new Error("標記已讀失敗");
                    return response.json();
                })
                .then(data => {
                    console.log("已標記為已讀", data.message);
                    collapsePanel.classList.add("is-read");

                    if ((moduleCode === "ApplyRental" || moduleCode === "ApplyHouse") && applicationId) {
                        return fetch('/MemberContracts/UpdateStatusApi', {
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify({
                                applicationId: parseInt(applicationId),
                                status: "PENDING"
                            })
                        });
                    }
                })
                .then(res => {
                    if (res && res.ok) {
                        return res.json();
                    }
                })
                .then(data => {
                    if (data) {
                        console.log("狀態更新成功", data);
                    }
                })
                .catch(err => {
                    console.error("錯誤：", err);
                });
        }



    });
});
