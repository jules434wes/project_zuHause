//GetDistrictsByCity

let AddressArea = document.querySelectorAll(".address-area");
AddressArea.forEach((area) => {
    let cityArea = area.querySelector(".address-area-city");
    let districtArea = area.querySelector(".address-area-district");

    let citySelect = cityArea.querySelector("select");
    let districtSelect = districtArea.querySelector("select");
    citySelect.addEventListener("change", function () {
        const cityId = this.value;
        console.log(cityId);

        fetch(`/Member/GetDistrictsByCity?cityId=${cityId}`)
            .then(res => res.json())
            .then(data => {
                console.log(data);
                let options = `<option selected>區域</option>`;
                let elems = document.createDocumentFragment();
                districtSelect.innerHTML = options;
                data.forEach((item) => {
                    let options = document.createElement("option");
                    options.value = item.value;
                    options.textContent = item.text;
                    elems.appendChild(options);
                });
                districtSelect.appendChild(elems);
            }).catch(err => {
                console.log("區域載入失敗", err);
            })


    });
});