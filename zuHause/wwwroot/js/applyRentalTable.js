const addFurnitureBtn = document.querySelector(".add-furniture-btn");
const furnitureBody = document.querySelector(".furniture-body");
const furnitureTemplate = document.querySelector("#furniture-template");

function updateFurnitureIndexes() {
    const rows = furnitureBody.querySelectorAll(".furniture-row");
    rows.forEach((row, index) => {
        row.querySelector(".index-cell").textContent = index + 1;

        // 將 data-name 改成 name="FurnitureItems[0].欄位"
        row.querySelectorAll("[data-name]").forEach(input => {
            const field = input.getAttribute("data-name");
            input.setAttribute("name", `FurnitureItems[${index}].${field}`);
        });
    });
}

function bindFurnitureRowEvents(row) {
    const delBtn = row.querySelector(".del-furniture-btn");
    delBtn.addEventListener("click", () => {
        row.remove();
        updateFurnitureIndexes();
    });
}

addFurnitureBtn.addEventListener("click", () => {
    const clone = furnitureTemplate.content.cloneNode(true);
    const row = clone.querySelector(".furniture-row");
    furnitureBody.appendChild(row);
    bindFurnitureRowEvents(row);
    updateFurnitureIndexes();
});

document.querySelectorAll(".furniture-row").forEach(row => bindFurnitureRowEvents(row));
updateFurnitureIndexes();
