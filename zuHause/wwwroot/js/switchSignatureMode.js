let signatureType = document.querySelector(".signature-type");
let inputSignType = signatureType.querySelectorAll("input[name='signType']");
let dataSignBlock = signatureType.querySelectorAll("div[data-sign-block]");
inputSignType.forEach((item) => {
    item.addEventListener("change", function (e) {
        console.log(e.target.value);
        toggleSignBlocks(e.target.value);
    });
});
function toggleSignBlocks(selected) {
    dataSignBlock.forEach(item => {
        console.log(item.dataset.signBlock);

        item.classList.toggle("d-none", item.dataset.signBlock !== selected);
    });
}