let allCard = document.querySelectorAll(".card .card-area");

allCard.forEach(card => {
    console.log(card);
    let cardTitle = card.querySelector(".card-title");
    let editBtn = card.querySelector(".btn-edit");
    let confirmBtn = card.querySelector(".btn-confirm");
    let cancelBtn = card.querySelector(".btn-cancel");
    let isEdit = false;
    let contractId = card.querySelector("input[type='hidden'].input-contractId").value;
    let originalContractName = card.querySelector("input[type='hidden'].input-contractName").value;
    editBtn.addEventListener("click", function () {
        isEdit = !isEdit;
        cardTitle.disabled = false;
        cardTitle.classList.remove("border-0");
        cardTitle.classList.add("border-1", "border", "border-dark");
        editBtn.classList.add("d-none");
        confirmBtn.classList.remove("d-none");
        cancelBtn.classList.remove("d-none");
    });
    confirmBtn.addEventListener("click", () => {
        changeBtnStatus(isEdit, editBtn, cardTitle, confirmBtn, cancelBtn);
        console.log(contractId);
        console.log(cardTitle.value);
        fetchChangeName(contractId,cardTitle.value);


    });
    cancelBtn.addEventListener("click", () => {
        changeBtnStatus(isEdit, editBtn, cardTitle, confirmBtn, cancelBtn);
        cardTitle.value = originalContractName;
    });
});


async function fetchChangeName(id, contractName) {
    if (contractName == null) {
        return;
    }
    try {
        const response = await fetch("/MemberContracts/UpdateContractName", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                contractId: id,
                contractName: contractName,
            }),
        });
        if (!response.ok) {
            throw new Error(`${response.status}`);
        }
        const data = await response.json();
        console.log(data);

        callToast('通知', '修改成功');
        originalContractName = contractName;


    } catch (error) {
        console.log(error);
    }
}

function changeBtnStatus(isEdit, editBtn, cardTitle, confirmBtn, cancelBtn) {
    isEdit = !isEdit;
    editBtn.classList.remove("d-none");
    cardTitle.disabled = true;
    confirmBtn.classList.add("d-none");
    cancelBtn.classList.add("d-none");


    cardTitle.classList.add("border-0");
    cardTitle.classList.remove("border-1", "border", "border-dark");


}
