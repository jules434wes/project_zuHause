let passwordAreaAll = document.querySelectorAll(".password-area");

passwordAreaAll.forEach((item) => {

let hiddenPwd = true;
let userPassword = item.querySelector("input[data-type='pwd']");
let togglePwd = item.querySelector(".toggle-pwd");
    item.addEventListener("click", function (e) {
        console.log();
        if (e.target.closest("button")) {
            hiddenPwd = !hiddenPwd;
                if (hiddenPwd) {
                    userPassword.setAttribute("type", "password");
                    item.classList.add("pwd-hidden");
                } else {
                    userPassword.setAttribute("type", "text");
                    item.classList.remove("pwd-hidden");
                }
        }
    });
});
