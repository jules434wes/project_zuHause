// 設定內容高度為容器高度
(() => {
    window.addEventListener("load", changeContainerHeight);
    window.addEventListener("resize", changeContainerHeight);

    function changeContainerHeight() {
        let container = document.querySelector(".container");
        let contentContainer;
        if (container) {
            contentContainer = container.querySelector(".content-container");
        }
        if (contentContainer) {
            contentContainer.style.height = `${container.clientHeight}px`;
        }
    }
})();

