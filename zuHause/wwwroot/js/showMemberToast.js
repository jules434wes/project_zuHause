const memberToast = document.getElementById('memberToast')
const toastBootstrap = bootstrap.Toast.getOrCreateInstance(memberToast)
let memberToastTitle = memberToast.querySelector("#memberToastTitle");
let memberToastContent = memberToast.querySelector("#memberToastContent");
function callToast(title = "標題",content = "內文載入中") {

    if (!memberToastTitle || !memberToastContent) {
        return;
    }
    memberToastTitle.textContent = title;
    memberToastContent.textContent = content;
    toastBootstrap.show();

}