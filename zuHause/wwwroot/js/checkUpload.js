//GetUploadStatus

async function checkUpload() {
    try {
        const response = await fetch("/Member/GetUploadStatus");
        const data = await response.json();
        if (!response.ok) {
            throw new Error(response.status);
        }
        processStatus(data);

    } catch (error)
    {
        console.log(error);
    }
}

let inputIdAreaFront = document.querySelector(".input-id-area-front");
let inputIdAreaBack = document.querySelector(".input-id-area-back");

function processStatus(data) {

    if (data.hasFront) {
        changeInputStatus(inputIdAreaFront, data.frontUploadedAt)
    }
    if (data.hasBack) {
        changeInputStatus(inputIdAreaBack,data.backUploadedAt)
    }
}
function changeInputStatus(item, uploadDate) {
    let label = item.querySelector("label");
    label.classList.add("disabled-label");
    label.textContent = `已於 ${uploadDate} 上傳`;
}
// 尚未處理，現在的邏輯雖然label可以只改一個未上傳，但是身分證上傳邏輯是規定兩個都要上傳，且不能分次提交
// 尚未處理，上傳後沒有改成已於XXXX上傳，有BUG不確定為何

window.addEventListener("load", checkUpload);